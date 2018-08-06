using Expenses.TestApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Expenses.TestApp.Converters
{
    class BoolToPlecConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;
            var plec = (Plec)value;
            var paramPlec = (Plec)parameter;
            if (plec == paramPlec)
            {
                return true;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var isChecked = (bool)value;
            var paramPlec = (Plec)parameter;
            if (isChecked && paramPlec == Plec.Kobieta)
                return Plec.Kobieta;
            return Plec.Mezczyzna;
        }
    }
}
