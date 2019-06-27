using LSX.PCService.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media;

namespace LSX.PCService.ViewModels
{
    class ConverterLightStateColor : IValueConverter
    {
        #region IValueConverter 成员

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (null == value)
            {
                return Brushes.Gray;
            }
            SolidColorBrush color = Brushes.Gray;
            LightState state = (LightState)value;
            switch(state)
            {
                case LightState.OFF:
                    break;
                case LightState.BLINK_GREEN:
                case LightState.ON_GREEN:
                    color = Brushes.Green;
                    break;
                case LightState.BLINK_RED:
                case LightState.ON_RED:
                    color = Brushes.Red;
                    break;
            }
            return color;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
