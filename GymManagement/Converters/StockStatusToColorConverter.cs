using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows;

namespace GymManagement.Converters
{
    public class StockStatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                if (status.Equals("Out of Stock", StringComparison.OrdinalIgnoreCase)) return Brushes.Red;
                if (status.Equals("Low Stock", StringComparison.OrdinalIgnoreCase)) return Brushes.DarkOrange;
                return Brushes.DarkGreen;
            }
            return Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
