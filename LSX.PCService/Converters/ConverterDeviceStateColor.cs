using LSX.PCService.Data;
using System;
using System.Windows.Data;
using System.Windows.Media;

namespace LSX.PCService.Converters
{
    class ConverterDeviceStateColor : IValueConverter
    {
        #region IValueConverter 成员

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (null == value)
            {
                return Brushes.Gray;
            }
            SolidColorBrush color = Brushes.Gray;
            DeviceState state = (DeviceState)Enum.Parse(typeof(DeviceState), (string)value);
            switch(state)
            {
                case DeviceState.初始化:
                    color = Brushes.LightYellow;
                    break;
                case DeviceState.断开:
                    color = Brushes.LightPink;
                    break;
                case DeviceState.已连接:
                    color = Brushes.LightGreen;
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
