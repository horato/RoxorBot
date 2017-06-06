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
    public class AutomatedMessagesLabelContentConverter : MarkupExtension, IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2)
                return "";
            if (!(values[0] is bool))
                return "";
            if (!(values[1] is bool))
                return "";

            var isRunning = (bool)values[0];
            var isPaused = (bool)values[1];

            if (isRunning)
                return "Automated messages are being sent.";
            if (isPaused)
                return "Automated messages are paused.";

            return "Automated messages are stopped.";
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
