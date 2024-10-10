using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace WpfItemsControlSample1.ViewModels
{
    public class ColorToSolidColorBrushValueConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value switch {
                   null        => null                      ,
                   Color color => new SolidColorBrush(color),
                   _           => throw new InvalidOperationException("Unsupported type [" + value.GetType().Name + "]")
               };

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            => throw new NotImplementedException();
    }
}
