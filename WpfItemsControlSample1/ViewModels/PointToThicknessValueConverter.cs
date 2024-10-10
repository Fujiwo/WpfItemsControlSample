using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WpfItemsControlSample1.ViewModels
{
    public class PointToThicknessValueConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value switch {
                   null        => null                                     ,
                   Point point => new Thickness(point.X, point.Y, 0.0, 0.0),
                   _           => throw new InvalidOperationException("Unsupported type [" + value.GetType().Name + "]")
               };

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
