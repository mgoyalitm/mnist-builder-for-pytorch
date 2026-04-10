using System.IO.Compression;

namespace MNIST.Utilities;

public static class FontManager
{
    public const string Characters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    public const int CharacterCount = 62;
    public const int MinRotation = -15;
    public const int MaxRotation = 15;
    public const int RotationStepSize = 5;
    public const int RotationSteps = (MaxRotation - MinRotation) / RotationStepSize;
    public const int CharSize = 21;

    private const string CategoryTag = @"category:";
    private const string NameTag = @"name:";
    private const string FileTag = @"filename:";
    private const string StyleTag = @"style:";
    private const string WeightTag = @"weight:";
    private const int draw_length = CharSize * 12;
    private const int semi_draw_length = draw_length / 2;
    private const int text_draw_length = draw_length / 3;
    private const byte BlackThreshold = 96;

    public static async IAsyncEnumerable<FontModel> DiscoverFontsAsync(string directory, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (Directory.Exists(directory) is false) yield break;
        string[] pb_paths = await Task.Run(() => Directory.EnumerateFiles(directory, "*.pb", SearchOption.AllDirectories).ToArray());

        foreach (string pb_path in pb_paths)
        {
            await foreach (FontModel font in GetFonts(pb_path, cancellationToken))
            {
                yield return font;
            }
        }
    }

    static async IAsyncEnumerable<FontModel> GetFonts(string pb_path, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (Path.Exists(pb_path) && Path.GetDirectoryName(pb_path) is string directory)
        {
            FontCategory category = FontCategory.Undefined;
            string name = string.Empty;
            FontStyle style = FontStyle.Regular;
            string path = string.Empty;
            FontWeight weight = FontWeight.Undefined;
            bool in_font_block = false;

            await foreach (string line in File.ReadLinesAsync(pb_path, cancellationToken))
            {
                string line_trimmed = line.Trim();
                if (in_font_block == false && line.StartsWith(CategoryTag, StringComparison.CurrentCultureIgnoreCase))
                {
                    category = GetValue(line).ToUpper() switch
                    {
                        "SANS_SERIF" => FontCategory.Sans_Serif,
                        "SERIF" => FontCategory.Serif,
                        "HANDWRITING" => FontCategory.Handwriting,
                        "DISPLAY" => FontCategory.Display,
                        "MONOSPACE" => FontCategory.Monospace,
                        _ => FontCategory.Undefined
                    };
                }

                if (in_font_block)
                {
                    if (line_trimmed.StartsWith(NameTag, StringComparison.CurrentCultureIgnoreCase))
                    {
                        name = GetValue(line);
                    }
                    else if (line_trimmed.StartsWith(StyleTag, StringComparison.CurrentCultureIgnoreCase))
                    {
                        style = GetValue(line).ToLower() switch
                        {
                            "normal" => FontStyle.Regular,
                            "italic" => FontStyle.Italic,
                            _ => FontStyle.Undefined,
                        };
                    }
                    else if (line_trimmed.StartsWith(FileTag, StringComparison.CurrentCultureIgnoreCase))
                    {
                        path = Path.Combine(directory, GetValue(line));
                    }
                    else if (line_trimmed.StartsWith(WeightTag, StringComparison.CurrentCultureIgnoreCase))
                    {
                        weight = int.Parse(GetValue(line)) switch
                        {
                            <= 150 => FontWeight.Thin,
                            <= 250 => FontWeight.ExtraLight,
                            <= 350 => FontWeight.Light,
                            <= 450 => FontWeight.Normal,
                            <= 550 => FontWeight.Medium,
                            <= 650 => FontWeight.SemiBold,
                            <= 750 => FontWeight.Bold,
                            <= 850 => FontWeight.ExtraBold,
                            _ => FontWeight.Black
                        };

                    }
                }

                if (line_trimmed.StartsWith("fonts {", StringComparison.CurrentCultureIgnoreCase))
                {
                    in_font_block = true;
                }

                if (line_trimmed == "}" && in_font_block)
                {
                    if (string.IsNullOrWhiteSpace(name) is false &&
                        File.Exists(path) && weight > 0)
                    {
                        yield return new(name, path, style, category, weight);
                    }

                    in_font_block = false;
                    name = string.Empty;
                    path = string.Empty;
                    style = FontStyle.Undefined;
                    weight = FontWeight.Undefined;
                }
            }
        }

        yield break;
        static string GetValue(string entry)
        {
            try
            {
                string[] pair = entry.Split(':');
                return pair[1].Trim().Trim('"');
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }


    public static SKColorFilter CreateContrastFilter(float intensity)
    {
        intensity = Math.Max(0f, intensity);
        float bias = (1f - intensity) * 128f;
        float[] matrix = [intensity, 0, 0, 0, bias, 0, intensity, 0, 0, bias, 0, 0, intensity, 0, bias, 0, 0, 0, 1, 0];
        return SKColorFilter.CreateColorMatrix(matrix);
    }

    public static async Task WriteMNISTAsync(string font_directory, FontModel[] fonts, IProgress<int> progress, CancellationToken cancellationToken = default)
    {
        int current = 0;

        using SKPaint paint = new()
        {
            Color = SKColors.Black,
            IsAntialias = false,
            ColorFilter = CreateContrastFilter(32)
        };

        await using Stream zip_stream = new FileStream(App.DestinationZipPath, FileMode.Create);
        using ZipArchive zip = new(zip_stream, ZipArchiveMode.Create, leaveOpen: false);

        for (int i = 0; i < fonts.Length; i++)
        {
            FontModel font = fonts[i];

            if (File.Exists(font.Path) is false)
            {
                continue;
            }

            string category = font.Category.ToString().ToLower().Replace('_', '-');
            string style = font.Style.ToString().ToLower();
            string font_name = Path.GetFileNameWithoutExtension(font.Path);

            using SKTypeface typeface = SKTypeface.FromFile(font.Path);
            using SKFont sk_font = new(typeface, text_draw_length);
            float offset_rotation = font.Style is FontStyle.Italic ? -RotationStepSize : 0f;

            for (int n = 0; n < CharacterCount; n++)
            {
                char character = Characters[n];

                for (int m = 0; m < RotationSteps; m++)
                {
                    progress?.Report(++current);

                    float rotation = MinRotation + m * RotationStepSize + offset_rotation;
                    string char_folder = char.IsDigit(character) ? $"digit_{character}" : (char.IsUpper(character) ? $"upper_{char.ToUpperInvariant(character)}" : $"lower_{char.ToLowerInvariant(character)}");
                    string file_name = $"{font_name}-({rotation}°)[{category},{style}, {font.Weight}].png";
                    string folder_name = Random.Shared.NextDouble() >= 0.1 ? "train" : "test";
                    string character_file = $"{folder_name}/{char_folder}/{file_name}";
                    
                    byte[] char_data = await Task.Run(() => GetCharData(character, rotation, sk_font, paint), cancellationToken);
                    ZipArchiveEntry char_entry = zip.CreateEntry(character_file, CompressionLevel.Optimal);
                    await using Stream char_stream = char_entry.Open();
                    char_stream.Write(char_data, 0, char_data.Length);
                }
            }
        }
    }

    private static byte[] GetCharData(char character, float rotation, SKFont sk_font, SKPaint paint)
    {
        using SKBitmap bitmap = new(draw_length, draw_length);
        using SKCanvas canvas = new(bitmap);

        canvas.Clear(SKColors.White);
        canvas.Save();
        canvas.Translate(semi_draw_length, semi_draw_length);
        canvas.RotateDegrees(rotation);
        canvas.DrawText(character.ToString(), 0, 0, sk_font, paint);
        canvas.Restore();

        SKRectI bounding_box = GetBounds(bitmap);
        float scale = Math.Min((float)CharSize / bounding_box.Width, (float)CharSize / bounding_box.Height);

        int width = Math.Min(CharSize, (int)(scale * bounding_box.Width));
        int height = Math.Min(CharSize, (int)(scale * bounding_box.Height));

        Binarize(bitmap, bounding_box);

        using SKBitmap char_bitmap = new(CharSize, CharSize);
        using SKCanvas char_canvas = new(char_bitmap);
        char_canvas.Clear(SKColors.White);

        int x_shift = (CharSize - width) / 2;
        int y_shift = (CharSize - height) / 2;

        SKRectI placement = new(x_shift, y_shift, x_shift + width, y_shift + height);
        char_canvas.DrawBitmap(bitmap, bounding_box, placement);
        Binarize(bitmap, new SKRectI(0, 0, CharSize, CharSize));
        using SKImage image = SKImage.FromBitmap(char_bitmap);
        using SKData image_data = image.Encode(SKEncodedImageFormat.Png, 100);
        return image_data.ToArray();
    }

    private static SKRectI GetBounds(SKBitmap bitmap)
    {
        int x_min = bitmap.Width;
        int y_min = bitmap.Height;
        int x_max = 0;
        int y_max = 0;

        for (int x = 0; x < bitmap.Width; x++)
        {
            for (int y = 0; y < bitmap.Height; y++)
            {
                SKColor color = bitmap.GetPixel(x, y);
                byte brightness = (byte)((color.Alpha * (0.299f * color.Red + 0.587f * color.Green + 0.114f * color.Blue)) / 255);

                if (brightness < BlackThreshold)
                {
                    x_min = Math.Min(x, x_min);
                    y_min = Math.Min(y, y_min);
                    x_max = Math.Max(x, x_max);
                    y_max = Math.Max(y, y_max);
                }
            }
        }

        return x_max < x_min || y_max < y_min ? SKRectI.Empty : new SKRectI(x_min, y_min, x_max + 1, y_max + 1);
    }

    private static void Binarize(SKBitmap bitmap, SKRectI bounding_box)
    {
        for (int x = bounding_box.Left; x < bounding_box.Right; x++)
        {
            for (int y = bounding_box.Top; y < bounding_box.Bottom; y++)
            {
                SKColor color = bitmap.GetPixel(x, y);
                byte brightness = (byte)((color.Alpha * (0.299f * color.Red + 0.587f * color.Green + 0.114f * color.Blue)) / 255);
                SKColor binarized_color = brightness < BlackThreshold ? SKColors.Black : SKColors.White;
                bitmap.SetPixel(x, y, binarized_color);
            }
        }
    }
}
