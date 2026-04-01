using System.Globalization;
using System.Windows;
using System.Windows.Data;
namespace MNIST.Converters;
public class BoolToVisibilityConverter : IValueConverter
{
    public bool IsInverted { get; set; }
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) 
        => value is bool boolean ? boolean ^ IsInverted ? Visibility.Visible : Visibility.Collapsed : Visibility.Visible;
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) 
        => throw new NotImplementedException();
}
