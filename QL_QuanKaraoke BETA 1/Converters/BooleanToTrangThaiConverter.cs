using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace QL_QuanKaraoke_BETA_1.Converters
{
    public class BooleanToTrangThaiConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
                              object parameter, CultureInfo culture)
        {
            if (value is bool isEnable)
            {
                return isEnable
                    ? "Đang Hoạt Động"
                    : "Ngưng Hoạt Động";
            }

            return "Không Xác Định";
        }

        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture)
        {
            if (value is string text)
            {
                return text == "Đang Hoạt Động";
            }

            return false;
        }
    }
}
