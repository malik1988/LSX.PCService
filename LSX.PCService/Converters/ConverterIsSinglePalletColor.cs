using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace LSX.PCService.Converters
{
    class ConverterIsSinglePalletColor : IValueConverter
    {
        #region IValueConverter 成员

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool? isSingle = value as bool?;
            if (null == isSingle)
            {
                return Brushes.Red;
            }
            else
            {
                return isSingle == true ? Brushes.Yellow : Brushes.Green;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
