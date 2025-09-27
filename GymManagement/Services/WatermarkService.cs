using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Globalization;
using System.Windows.Data;

namespace GymManagement.Services
{
    public static class WatermarkService
    {
        public static readonly DependencyProperty WatermarkProperty =
            DependencyProperty.RegisterAttached("Watermark", typeof(string), typeof(WatermarkService), new PropertyMetadata(null, OnWatermarkChanged));

        public static string GetWatermark(DependencyObject d)
        {
            return (string)d.GetValue(WatermarkProperty);
        }

        public static void SetWatermark(DependencyObject d, string value)
        {
            d.SetValue(WatermarkProperty, value);
        }

        private static void OnWatermarkChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox textBox)
            {
                var binding = new Binding("Watermark")
                {
                    Source = textBox,
                    Converter = new PlaceholderConverter(),
                    FallbackValue = string.Empty,
                    IsAsync = true // Prevents UI thread blocking
                };

                textBox.SetBinding(TextBox.BackgroundProperty, binding);
            }
        }

        private class PlaceholderConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value is TextBox textBox)
                {
                    if (string.IsNullOrEmpty(textBox.Text))
                    {
                        var watermark = GetWatermark(textBox);
                        if (!string.IsNullOrEmpty(watermark))
                        {
                            return new VisualBrush(new Label
                            {
                                Content = watermark,
                                Foreground = Brushes.Gray,
                                Margin = new Thickness(5, 0, 0, 0),
                                VerticalAlignment = VerticalAlignment.Center
                            })
                            { Opacity = 0.5, Stretch = Stretch.None, AlignmentX = AlignmentX.Left };
                        }
                    }
                }
                return Brushes.White;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return DependencyProperty.UnsetValue;
            }
        }
    }
}
