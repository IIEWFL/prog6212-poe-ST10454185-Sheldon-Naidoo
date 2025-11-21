using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace ContractMonthlyClaimSystem.Converters
{
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Check if the parameter specifies inversion
            bool isInverse = parameter != null && parameter.ToString().Equals("Inverse", StringComparison.OrdinalIgnoreCase);

            if (value == null)
            {
                // If value is null, return Visible (Standard) or Collapsed (Inverse)
                return isInverse ? Visibility.Collapsed : Visibility.Visible;
            }
            else
            {
                // If value is not null, return Collapsed (Standard) or Visible (Inverse)
                return isInverse ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
