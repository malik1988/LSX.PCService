using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace LSX.PCService.ViewModels
{
    class ConverterIsSinglePalletText : IValueConverter
    {
        #region IValueConverter 成员

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool? isSingle = value as bool?;
            if (null == isSingle)
            {
                return "无效栈板号";
            }
            else
            {
                return isSingle == true ? "是" : "否";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
