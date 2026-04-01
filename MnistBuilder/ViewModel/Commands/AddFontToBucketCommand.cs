namespace MNIST.ViewModel.Commands;
public class AddFontToBucketCommand : ICommand
{
    private readonly FontController _controller;
    public event EventHandler CanExecuteChanged { add { } remove { } }

    [SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "<Pending>")]
    public AddFontToBucketCommand(FontController controller)
    {
        _controller = controller;
    }

    public bool CanExecute(object parameter) => parameter is FontModel;
    public void Execute(object parameter)
    {
        if (parameter is FontModel font)
        {
            if (_controller.FontBucket.FirstOrDefault(x => x.Path == font.Path) is null)
            {
                _controller.FontBucket.Add(font);
                _controller.MainViewModel.ShowNotification($"Font '{font}' added to bucket.");

            }
            else
            {
                _controller.MainViewModel.ShowNotification($"Font '{font}'already exist in bucket.");
            }
        }
    }
}
