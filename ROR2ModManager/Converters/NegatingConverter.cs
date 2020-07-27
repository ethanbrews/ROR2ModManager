using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace ROR2ModManager.Converters
{
    public class NegatingConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is double)
            {
                return -((double)value);
            }
            else if (value is int)
            {
                return -((int)value);
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is double)
            {
                return -(double)value;
            }
            else if (value is int)
            {
                return -((int)value);
            }
            return value;
        }
    }
}
