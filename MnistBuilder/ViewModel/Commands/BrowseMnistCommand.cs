using Microsoft.Win32;
namespace MNIST.ViewModel.Commands;
public class BrowseMnistCommand : ICommand
{
    private readonly SemaphoreSlim semaphore = new(1);
    public event EventHandler CanExecuteChanged { add { } remove { } }
    public bool CanExecute(object _) => true;

    public async void Execute(object _)
    {
        try
        {
            await semaphore.WaitAsync();
            
            SaveFileDialog dialog = new()
            {
                Filter = "ZIP Archive (*.zip)|*.zip",
                DefaultExt = ".zip",
                AddExtension = true,
                FileName = "mnist.zip"
            };

            if (dialog.ShowDialog(App.Current.MainWindow) is true)
            {
                App.DestinationZipPath = dialog.FileName;
            }
        }
        finally
        {
            semaphore.Release();
        }
    }
}
