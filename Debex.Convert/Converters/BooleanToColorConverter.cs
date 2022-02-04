using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace Debex.Convert.Converters
{
    public class BooleanToColorConverter : IValueConverter
    {
        public static BooleanToColorConverter Instance { get; } = new BooleanToColorConverter();
        
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is Brush[] colors)
            {
                if (value is bool isFirstColor && isFirstColor) return colors[0];
                else return colors[1];
            }

            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
