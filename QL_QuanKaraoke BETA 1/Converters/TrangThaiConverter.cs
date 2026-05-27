using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace QL_QuanKaraoke_BETA_1.Converters
{
    public class TrangThaiToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
                              object parameter, CultureInfo culture)
        {
            // Màu gốc theo từng trạng thái (C# 7.3 — dùng switch statement)
            Color main;
            switch (value?.ToString())
            {
                case "Trống":
                    main = (Color)ColorConverter.ConvertFromString("#22C55E"); break; // xanh lá
                case "Đang Dùng":
                    main = (Color)ColorConverter.ConvertFromString("#EF4444"); break; // đỏ
                case "Dọn Dẹp":
                    main = (Color)ColorConverter.ConvertFromString("#F59E0B"); break; // vàng
                case "Bảo Trì":
                    main = (Color)ColorConverter.ConvertFromString("#6B7280"); break; // xám
                default:
                    main = (Color)ColorConverter.ConvertFromString("#7C3AED"); break; // tím fallback
            }

            // Nếu parameter = "dim" thì trả về màu mờ (dùng làm nền badge trạng thái)
            if (parameter?.ToString() == "dim")
                return new SolidColorBrush(Color.FromArgb(45, main.R, main.G, main.B));

            return new SolidColorBrush(main);
        }

        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture)
            => throw new NotImplementedException();

    }

    // ═══════════════════════════════════════════════════
    // Converter 2: TrangThai → Nhãn hiển thị
    // ═══════════════════════════════════════════════════
    public class TrangThaiToLabelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
                              object parameter, CultureInfo culture)
        {
            switch (value?.ToString())
            {
                case "Trống": return "Đang trống";
                case "Đang Dùng": return "Đang dùng";
                case "Dọn Dẹp": return "Chờ dọn";
                case "Bảo Trì": return "Bảo trì";
                default: return value?.ToString() ?? "";
            }
        }

        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    // ═══════════════════════════════════════════════════
    // Converter 3: TrangThai → Ký tự chấm tròn ●
    //   Dùng để hiện dấu ● trước nhãn trạng thái
    //   (màu của ● được bind riêng qua TrangThaiToBrush)
    // ═══════════════════════════════════════════════════
    public class TrangThaiToDotConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
                              object parameter, CultureInfo culture)
            => "●";

        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class TrangThaiToDropShadowConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
                              object parameter, CultureInfo culture)
        {
            Color shadowColor;
            switch (value?.ToString())
            {
                case "Trống":
                    shadowColor = (Color)ColorConverter.ConvertFromString("#22C55E"); break;
                case "Đang Dùng":
                    shadowColor = (Color)ColorConverter.ConvertFromString("#EF4444"); break;
                case "Dọn Dẹp":
                    shadowColor = (Color)ColorConverter.ConvertFromString("#F59E0B"); break;
                case "Bảo Trì":
                    shadowColor = (Color)ColorConverter.ConvertFromString("#6B7280"); break;
                default:
                    shadowColor = (Color)ColorConverter.ConvertFromString("#7C3AED"); break;
            }

            return new DropShadowEffect
            {
                Color = shadowColor,
                BlurRadius = 18,
                ShadowDepth = 0,
                Opacity = 0.75
            };
        }

        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
