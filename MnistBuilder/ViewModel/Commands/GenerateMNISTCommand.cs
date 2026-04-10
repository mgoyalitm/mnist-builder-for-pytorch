namespace MNIST.ViewModel.Commands;
public class GenerateMNISTCommand: IProgress<int>, ICommand, INotifyPropertyChanged
{
    private bool isRunning;
    private int total;
    private int current;
    
    public event PropertyChangedEventHandler PropertyChanged;
    public event EventHandler CanExecuteChanged;

    public bool IsExecuting
    {
        get => isRunning;
        set
        {
            if (value != isRunning)
            {
                isRunning = value;
                OnPropertyChanged(nameof(IsExecuting));
            }
        }
    }

    public int Current
    {
        get => current;
        set
        {
            if (value != current)
            {
                current = value;
                OnPropertyChanged(nameof(Current));
            }
        }
    }

    public int Total
    {
        get => total;
        set
        {
            if (value != total)
            {
                total = value;
                OnPropertyChanged(nameof(Total));
            }
        }
    }

    public bool CanExecute(object _) => IsExecuting == false;
    public async void Execute(object _)
    {
        if (ValidateZipPath() is false)
        {
            if (App.Current.Resources[nameof(BrowseMnistCommand)] is BrowseMnistCommand command)
            {
                command.Execute(null);
            }

            if (ValidateZipPath() is false)
            {
                return;
            }
        }

        try
        {
            if (File.Exists(App.DestinationZipPath))
            {
                File.Delete(App.DestinationZipPath);
            }

            FontModel[] fonts = [.. App.ViewModel.FontBucket];
            Current = 0;
            Total = fonts.Length * FontManager.CharacterCount * FontManager.RotationSteps;
            IsExecuting = true;
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            await FontManager.WriteMNISTAsync(App.DestinationZipPath, fonts, this);
        }
        finally
        {
            await Task.Delay(250);
            IsExecuting = false;
            Current = 0;
            Total = 0;
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new(propertyName));

    public void Report(int value) => Current = value;


    private bool ValidateZipPath()
    {
        try
        {
            return string.IsNullOrWhiteSpace(Path.GetFullPath(App.DestinationZipPath)) is false;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
