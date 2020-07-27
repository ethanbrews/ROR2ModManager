using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace ROR2ModManager.Converters
{
    class WidthToTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            System.Diagnostics.Debug.WriteLine(value);
            System.Diagnostics.Debug.WriteLine((int)((double)value / 10));
            return new Duration(new TimeSpan(0, 0, (int)((double)value/10)));
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return 0;
        }
    }
}
