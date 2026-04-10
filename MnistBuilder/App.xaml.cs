using MNIST.View;
using MNIST.View.Dialogs;
using System.Configuration;
using System.Windows;
using System.Windows.Media;
using System.Xml;

namespace MNIST;

public partial class App : Application
{
    private const string RepositoryKey = "Repository";
    private const string DestinationKey = "Destination";
    private const string SelectedFontKey = "SelectedFont";
    private const string FontBucketKey = "FontBucket";
    private const string FilterKey = "Filters";
    private const string StyleKey = "Style";
    private const string ValueKey = "value";
    private const string WeightKey = "Weight";
    private const string FilteredPreviewKey = "FilterPreview";
    private const string CategoryKey = "Category";
    private const string FontItemKey = "Font";
    private const string FontPathKey = "path";
    private const string ViewModelKey = "ViewModel";

    public static ViewModel.MainViewModel ViewModel { get; private set; }
    public static string RepositoryPath { get; set; }
    public static string DestinationZipPath { get; set; }

    private static readonly Dictionary<string, FontFamily> font_registry = [];
    private static readonly Dictionary<string, GlyphTypeface> typeface_registry = [];

    protected override void OnActivated(EventArgs e)
    {

        foreach (Window window in Current.Windows.Cast<Window>())
        {
            if (window is not View.MainWindow)
            {
                window.Activate();
            }
        }

        base.OnActivated(e);
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        EnsureConfigurationCreated();

        if (Current.Resources[ViewModelKey] is ViewModel.MainViewModel viewModel)
        {
            ViewModel = viewModel;
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            RepositoryPath = config.AppSettings.Settings[RepositoryKey].Value;
            DestinationZipPath = config.AppSettings.Settings[DestinationKey].Value;
            await viewModel.InitializeFontsAsync();
            string selected_font_path = config.AppSettings.Settings[SelectedFontKey].Value;

            Dictionary<string, FontModel> font_map = [];

            foreach (FontModel font in viewModel.AvailableFonts)
            {
                font_map[font.Path] = font;
            }

            if (font_map.TryGetValue(selected_font_path, out FontModel selected))
            {
                int index = Array.IndexOf(viewModel.AvailableFonts, selected);

                if (index != -1)
                {
                    viewModel.SelectedFontIndex = index;
                }
            }

            XmlDocument configXml = new();
            configXml.Load(config.FilePath);
            XmlElement bucket = configXml.DocumentElement.SelectSingleNode(FontBucketKey) as XmlElement;
            XmlElement filters = configXml.DocumentElement.SelectSingleNode(FilterKey) as XmlElement;

            List<Model.FontStyle> styles = [];
            List<Model.FontWeight> weights = [];
            List<FontCategory> categories = [];

            if (filters.SelectSingleNode(FilteredPreviewKey) is XmlElement preview_element &&
                bool.TryParse(preview_element.GetAttribute(ValueKey), out bool apply_filter))
            {
                viewModel.FilterViewModel.ShowFilteredResults = apply_filter;
            }


            foreach (XmlElement element in filters.SelectNodes(StyleKey))
            {
                string style_text = element.GetAttribute(ValueKey);

                if (Enum.TryParse(style_text, out Model.FontStyle style))
                {
                    styles.Add(style);
                }
            }

            foreach (XmlElement element in filters.SelectNodes(WeightKey))
            {
                string weight_text = element.GetAttribute(ValueKey);

                if (Enum.TryParse(weight_text, out Model.FontWeight weight))
                {
                    weights.Add(weight);
                }
            }

            foreach (XmlElement element in filters.SelectNodes(CategoryKey))
            {
                string category_text = element.GetAttribute(ValueKey);

                if (Enum.TryParse(category_text, out FontCategory category))
                {
                    categories.Add(category);
                }
            }

            foreach (XmlElement element in bucket.SelectNodes(FontItemKey))
            {
                string font_path = element.GetAttribute(FontPathKey);

                if (string.IsNullOrWhiteSpace(font_path) is false && font_map.TryGetValue(font_path, out FontModel font))
                {
                    viewModel.FontBucket.Add(font);
                }
            }

            viewModel.FilterViewModel.SelectedFontStyles = styles;
            viewModel.FilterViewModel.SelectedFontWeights = weights;
            viewModel.FilterViewModel.SelectedFontCategories = categories;
        }

        base.OnStartup(e);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        base.OnExit(e);
        EnsureConfigurationCreated();

        Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        config.AppSettings.Settings[RepositoryKey].Value = RepositoryPath;
        config.AppSettings.Settings[DestinationKey].Value = DestinationZipPath;
        config.AppSettings.Settings[SelectedFontKey].Value = ViewModel.SelectedFont?.Path;
        config.Save();

        XmlDocument configXml = new();
        configXml.Load(config.FilePath);
        XmlElement bucket = configXml.DocumentElement.SelectSingleNode(FontBucketKey) as XmlElement;
        XmlElement filters = configXml.DocumentElement.SelectSingleNode(FilterKey) as XmlElement;
        filters.RemoveAll();
        bucket.RemoveAll();

        XmlElement preview = filters.SelectSingleNode(FilteredPreviewKey) as XmlElement;
        preview = configXml.CreateElement(FilteredPreviewKey);
        preview.SetAttribute(ValueKey, ViewModel.FilterViewModel.ShowFilteredResults.ToString());
        filters.AppendChild(preview);

        foreach (Model.FontStyle style in ViewModel.FilterViewModel.SelectedFontStyles.Cast<Model.FontStyle>())
        {
            XmlElement element = configXml.CreateElement(StyleKey);
            element.SetAttribute(ValueKey, style.ToString());
            filters.AppendChild(element);
        }

        foreach (Model.FontWeight weight in ViewModel.FilterViewModel.SelectedFontWeights.Cast<Model.FontWeight>())
        {
            XmlElement element = configXml.CreateElement(WeightKey);
            element.SetAttribute(ValueKey, weight.ToString());
            filters.AppendChild(element);
        }

        foreach (FontCategory category in ViewModel.FilterViewModel.SelectedFontCategories.Cast<FontCategory>())
        {
            XmlElement element = configXml.CreateElement(CategoryKey);
            element.SetAttribute(ValueKey, category.ToString());
            filters.AppendChild(element);
        }

        foreach (string path in ViewModel.FontBucket.Select(x => x.Path))
        {
            XmlElement element = configXml.CreateElement(FontItemKey);
            element.SetAttribute(FontPathKey, path);
            bucket.AppendChild(element);
        }

        configXml.Save(config.FilePath);
    }


    private static void EnsureConfigurationCreated()
    {
        Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

        if (config.AppSettings.Settings[RepositoryKey] is null)
        {
            config.AppSettings.Settings.Add(RepositoryKey, string.Empty);
        }

        if (config.AppSettings.Settings[DestinationKey] is null)
        {
            config.AppSettings.Settings.Add(DestinationKey, string.Empty);
        }

        if (config.AppSettings.Settings[SelectedFontKey] is null)
        {
            config.AppSettings.Settings.Add(SelectedFontKey, string.Empty);
        }

        config.Save();

        XmlDocument configXml = new();
        configXml.Load(config.FilePath);

        if (configXml.DocumentElement.SelectSingleNode(FilterKey) is null)
        {
            XmlElement element = configXml.CreateElement(FilterKey);
            configXml.DocumentElement.AppendChild(element);
        }

        if (configXml.DocumentElement.SelectSingleNode(FontBucketKey) is null)
        {
            XmlElement element = configXml.CreateElement(FontBucketKey);
            configXml.DocumentElement.AppendChild(element);
        }

        configXml.Save(config.FilePath);
    }

    public static bool? GetConfirmation(string title, string body, string warning)
    {
        ConfirmDialog dialog = new(title, body, warning);
        return dialog.ShowDialog();
    }

    public static void ShowFilters()
    {
        FilterWindow filter = new();
        filter.ShowDialog();
    }

    public static FontFamily GetFontFamily(string path)
    {
        if (font_registry.TryGetValue(path, out FontFamily family))
        {
            return family;
        }

        GlyphTypeface glyph = GetTypeface(path);

        if (glyph is null)
        {
            return null;
        }

        bool isValid = true;

        Parallel.ForEach(FontManager.Characters, (character, state) =>
        {
            if (glyph.CharacterToGlyphMap.TryGetValue(character, out ushort index) is false || index == 0)
            {
                isValid = false;
                return;
            }

            if (glyph.AdvanceWidths.TryGetValue(index, out double width) is false || width <= 0d)
            {
                isValid = false;
                return;
            }

            Geometry shape = glyph.GetGlyphOutline(index, 100, 100);

            if (shape.Bounds.Width == 0 || shape.Bounds.Height == 0)
            {
                isValid = false;
                return;
            }
        });

        if (isValid is false)
        {
            return null;
        }

        string name = glyph.FamilyNames.Values.FirstOrDefault();

        if (string.IsNullOrWhiteSpace(name) is false)
        {
            string folder = Path.GetDirectoryName(path);
            Uri uri = new($"file:///{folder.Replace("\\", "/")}/");
            FontFamily font_family = new(uri, $"./#{name}");
            font_registry[path] = font_family;
            return font_family;
        }

        return null;
    }

    public static GlyphTypeface GetTypeface(string path)
    {
        if (typeface_registry.TryGetValue(path, out GlyphTypeface typeface))
        {
            return typeface;
        }

        try
        {
            Uri uri = new(path, UriKind.Absolute);
            typeface = new(uri);
            typeface_registry[path] = typeface;
            return typeface;
        }
        catch (Exception)
        {
            return null;
        }
    }
}
