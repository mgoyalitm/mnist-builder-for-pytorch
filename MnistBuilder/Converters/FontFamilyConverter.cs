using System.Globalization;
using System.Windows.Data;
namespace MNIST.Converters;
public class FontFamilyConverter : IValueConverter
{
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) 
        => throw new NotImplementedException();
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) 
        => value is string path ? App.GetFontFamily(path) : (object)null;
}
