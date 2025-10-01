using GymManagement.Properties;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace GymManagement.Converters
{
    public class EnumToLocalizedTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return string.Empty;

            Type enumType = value.GetType();

            // Construct the resource key: EnumName_ValueName (e.g., "SubscriptionType_Daily")
            string resourceKey = $"{enumType.Name}_{value}";

            // Look up the localized string in the application resources
            string localizedText = Resources.ResourceManager.GetString(resourceKey, Thread.CurrentThread.CurrentCulture);

            // Return the localized text or the enum's name if the resource key is missing
            return !string.IsNullOrEmpty(localizedText) ? localizedText : value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing; // One-way conversion for display purposes
        }
    }
}
