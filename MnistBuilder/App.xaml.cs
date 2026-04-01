using MNIST.ViewModel;
using System.Windows;
using System.Configuration;
using System.Xml;

namespace MNIST;

public partial class App : Application
{
    private const string RepositoryKey = "Repository";
    private const string DestinationKey = "Destination";
    private const string SelectedFontKey = "SelectedFont";
    private const string FontBucketKey = "FontBucket";
    private const string FontItemKey = "Font";
    private const string FontPathKey = "path";
    private const string ViewModelKey = "ViewModel";

    public static MainViewModel ViewModel { get; private set; }
    public static string RepositoryPath { get; set; }
    public static string DestinationPath { get; set; }
    
    protected override async void OnStartup(StartupEventArgs e)
    {
        EnsureConfigurationCreated();

        if (Current.Resources[ViewModelKey] is MainViewModel viewModel)
        {
            ViewModel = viewModel;
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            RepositoryPath = config.AppSettings.Settings[RepositoryKey].Value;
            DestinationPath = config.AppSettings.Settings[DestinationKey].Value;
            await viewModel.FontController.InitializeFontsAsync();
            string selected_font_path = config.AppSettings.Settings[SelectedFontKey].Value;

            Dictionary<string, FontModel> font_map = [];
     
            foreach (FontModel font in viewModel.FontController.AvailableFonts)
            {
                font_map[font.Path] = font;
            }

            if (font_map.TryGetValue(selected_font_path, out FontModel selected))
            {
                int index = viewModel.FontController.AvailableFonts.IndexOf(selected);

                if (index != -1)
                {
                    viewModel.FontController.SelectedFontIndex = index;
                }
            }

            XmlDocument configXml = new();
            configXml.Load(config.FilePath);
            XmlElement bucket = configXml.DocumentElement.SelectSingleNode(FontBucketKey) as XmlElement;

            foreach (XmlElement element in bucket.SelectNodes(FontItemKey))
            {
                string font_path = element.GetAttribute(FontPathKey);

                if (string.IsNullOrWhiteSpace(font_path) is false && font_map.TryGetValue(font_path, out FontModel font))
                {
                    viewModel.FontController.FontBucket.Add(font);
                }
            }
        }

        base.OnStartup(e);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        base.OnExit(e);
        EnsureConfigurationCreated();

        Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        config.AppSettings.Settings[RepositoryKey].Value = RepositoryPath;
        config.AppSettings.Settings[DestinationKey].Value = DestinationPath;
        config.AppSettings.Settings[SelectedFontKey].Value = ViewModel.FontController.SelectedFont?.Path;
        config.Save();

        XmlDocument configXml = new();
        configXml.Load(config.FilePath);
        XmlElement bucket = configXml.DocumentElement.SelectSingleNode(FontBucketKey) as XmlElement;
        bucket.RemoveAll();

        foreach (string path in ViewModel.FontController.FontBucket.Select(x => x.Path))
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

        if (configXml.DocumentElement.SelectSingleNode(FontBucketKey) is null)
        {
            XmlElement element = configXml.CreateElement(FontBucketKey);
            configXml.DocumentElement.AppendChild(element);
            configXml.Save(config.FilePath);
        }
    }
}
