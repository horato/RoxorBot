using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace RoxorBot.Data.Converters
{
    public class LabelForegroundConverter : MarkupExtension, IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2)
                return Brushes.Transparent;
            if (!(values[0] is bool))
                return Brushes.Transparent;
            if (!(values[1] is bool))
                return Brushes.Transparent;

            var isRunning = (bool)values[0];
            var isPaused = (bool)values[1];

            if (isRunning)
                return Brushes.Green;
            if (isPaused)
                return Brushes.Yellow;

            return Brushes.Red;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
