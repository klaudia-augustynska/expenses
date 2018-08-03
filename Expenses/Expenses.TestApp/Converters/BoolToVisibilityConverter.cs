using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Expenses.TestApp.Converters
{
    class BoolToVisibilityConverter : IValueConverter
    {
        public bool CollapsedNotHidden { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var positive = Visibility.Visible;
            var negative = CollapsedNotHidden ? Visibility.Collapsed : Visibility.Hidden;
            return (bool)value ? positive : negative;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
