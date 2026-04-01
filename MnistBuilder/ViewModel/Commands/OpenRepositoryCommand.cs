using Microsoft.Win32;

namespace MNIST.ViewModel.Commands;
public class OpenRepositoryCommand: ICommand
{
    private readonly SemaphoreSlim semaphore = new(1);
    public event EventHandler CanExecuteChanged { add { } remove { } }
    public bool CanExecute(object _) => true;

    public async void Execute(object _)
    {
        try
        {
            await semaphore.WaitAsync();
            
            OpenFolderDialog dialog = new()
            {
                Title = "Open Google Font Repository",
                Multiselect = false,
            };

            if (dialog.ShowDialog(App.Current.MainWindow) is true)
            {
                App.RepositoryPath = dialog.FolderName;
                await App.ViewModel.FontController.InitializeFontsAsync();
            }

        }
        finally
        {
            semaphore.Release();
        }
    }
}
