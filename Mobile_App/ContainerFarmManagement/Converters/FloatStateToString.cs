using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContainerFarmManagement.Converters
{
    public class FloatStateToString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int state = (int)value;
            return state == 0 ? "OFF" : "ON";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string state = (string)value;
            return state.Equals("OFF") ? 0 : 1;
        }
    }
}
