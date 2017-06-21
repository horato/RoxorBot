using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace RoxorBot.Data.Converters
{
    public class RewardTimerLabelContentConverter : MarkupExtension, IMultiValueConverter
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
                return "Timer is running.";
            if (isPaused)
                return "Timer is paused.";

            return "Timer is not running.";
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
