namespace MNIST.ViewModel.Commands;

public class GenerateMNISTCommand(FontController controller) : IProgress<int>, ICommand, INotifyPropertyChanged
{
    private readonly FontController _controller = controller;
    
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
        if (Path.Exists(App.DestinationPath) is false)
        {
            if (App.Current.Resources[nameof(BrowseMnistDirectoryCommand)] is BrowseMnistDirectoryCommand command)
            {
                command.Execute(null);
            }
        }

        if (Path.Exists(App.DestinationPath) is false)
        {
            return;
        }

        try
        {
            FontModel[] fonts = [.. _controller.FontBucket];
            Current = 0;
            Total = fonts.Length * FontManager.CharacterCount * FontManager.RotationSteps;
            IsExecuting = true;
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);

            await Task.Run(() => 
            {
                foreach (string file in Directory.GetFiles(App.DestinationPath))
                {
                    File.SetAttributes(file, FileAttributes.Normal);
                    File.Delete(file);
                }

                foreach (string directory in Directory.GetDirectories(App.DestinationPath))
                {
                    Directory.Delete(directory, true);
                }
            });
            await FontManager.WriteMNISTAsync(App.DestinationPath,fonts, this);
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
}
