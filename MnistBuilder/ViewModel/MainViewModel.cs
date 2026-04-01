namespace MNIST.ViewModel;
public partial class MainViewModel : INotifyPropertyChanged
{
    private string statusMessage;

    public event PropertyChangedEventHandler PropertyChanged;

    public MainViewModel()
    {
        FontController = new(this);
    }

    public FontController FontController { get; }


    public string StatusMessage
    {
        get => statusMessage;
        set
        {
            if (value != statusMessage)
            {
                statusMessage = value;
                OnPropertyChanged(nameof(StatusMessage));
            }
        }
    }


    private void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public void ShowNotification(string message)
    {
    }
}
