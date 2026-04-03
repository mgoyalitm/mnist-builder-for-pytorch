namespace MNIST.ViewModel;

public partial class MainViewModel : INotifyPropertyChanged
{
    private FontModel[] availableFonts = [];
    private int selectedFontIndex;
    private bool fontsLoaded;
    private bool fontLoading;
    private readonly SemaphoreSlim semaphoreInitialize = new(1);
    public event PropertyChangedEventHandler PropertyChanged;

    public MainViewModel()
    {
        FontBucket = [];
        FontBucket.CollectionChanged += (s, e) => OnPropertyChanged(nameof(FontBucketSize));
        ChangeFontCommand = new ChangeFontCommand();
        AddFontToBucketCommand = new AddFontToBucketCommand();
        DeleteFontCommand = new DeleteFontCommand();
        ClearBucketCommand = new ClearBucketCommand();
        GenerateMNISTCommand = new GenerateMNISTCommand();
        FilterViewModel = new();
    }

    public FilterViewModel FilterViewModel { get; }
    public ICommand ChangeFontCommand { get; }
    public ICommand AddFontToBucketCommand { get; }
    public ICommand DeleteFontCommand { get; }
    public ICommand ClearBucketCommand { get; }
    public ICommand GenerateMNISTCommand { get; }

    public bool FontsLoaded
    {
        get => fontsLoaded;
        set
        {
            if (value != fontsLoaded)
            {
                fontsLoaded = value;
                OnPropertyChanged(nameof(FontsLoaded));
            }
        }
    }

    public bool FontLoading
    {
        get => fontLoading;
        set
        {
            if (value != fontLoading)
            {
                fontLoading = value;
                OnPropertyChanged(nameof(FontLoading));
            }
        }
    }

    public int SelectedFontIndex
    {
        get => selectedFontIndex;
        set
        {
            value = availableFonts.Length == 0 ? 0 : value % availableFonts.Length;

            if (value < 0)
            {
                value = availableFonts.Length < 1 ? 0 : value + availableFonts.Length;
            }

            if (value != selectedFontIndex)
            {
                selectedFontIndex = value;
                OnPropertyChanged(nameof(SelectedFontIndex));
                OnPropertyChanged(nameof(SerialNumber));
                OnPropertyChanged(nameof(SelectedFont));
            }
        }
    }

    public int SerialNumber => SelectedFontIndex + 1;
    public int FontsCountTotal => availableFonts.Length;

    public FontModel[] AvailableFonts
    {
        get => availableFonts;
        set
        {
            if (value != availableFonts)
            {
                availableFonts = value;
                OnPropertyChanged(nameof(AvailableFonts));
                OnPropertyChanged(nameof(FontsCountTotal));
            }
        }
    }

    public FontModel SelectedFont => availableFonts.Length == 0 ? null : availableFonts[selectedFontIndex];

    public ObservableCollection<FontModel> FontBucket { get; }
    public int FontBucketSize => FontBucket.Count;

    public async Task InitializeFontsAsync(CancellationToken cancellationToken = default)
    {
        if (Path.Exists(App.RepositoryPath) is false)
        {
            return;
        }

        try
        {
            await semaphoreInitialize.WaitAsync(cancellationToken);
            FontsLoaded = false;
            FontLoading = true;
            List<FontModel> fonts = [];

            await foreach (FontModel font in FontManager.DiscoverFontsAsync(App.RepositoryPath, cancellationToken))
            {
                if (await Task.Run(() => App.GetFontFamily(font.Path)) is not null)
                {
                    fonts.Add(font);
                }
            }

            AvailableFonts = [.. fonts.OrderBy(x => x.Name)];
            await Task.Delay(10, cancellationToken);
            OnPropertyChanged(nameof(SelectedFont));
        }
        catch (Exception)
        {
            // Action
        }
        finally
        {
            semaphoreInitialize.Release();
            FontLoading = false;
            FontsLoaded = AvailableFonts.Length > 0;

        }
    }


    private void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
