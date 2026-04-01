using System.Windows;
using System.Windows.Controls;
namespace MNIST.Controls;
public class HelpItemControl : Control
{
    public static readonly DependencyProperty HelpKeyProperty = DependencyProperty.Register(nameof(HelpKey), typeof(string), typeof(HelpItemControl), new PropertyMetadata(string.Empty));
    public static readonly DependencyProperty HelpTextProperty = DependencyProperty.Register(nameof(HelpText), typeof(string), typeof(HelpItemControl), new PropertyMetadata(string.Empty));
    public string HelpKey
    {
        get => (string)GetValue(HelpKeyProperty); 
        set => SetValue(HelpKeyProperty, value);
    }

    public string HelpText
    {
        get => (string)GetValue(HelpTextProperty); 
        set => SetValue(HelpTextProperty, value);
    }
}
