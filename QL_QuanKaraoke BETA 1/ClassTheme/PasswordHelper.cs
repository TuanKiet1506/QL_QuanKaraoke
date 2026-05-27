using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace QL_QuanKaraoke_BETA_1.ClassTheme
{
    public static class PasswordHelper
    {
        // Attached property theo dõi trạng thái rỗng
        public static readonly DependencyProperty IsEmptyProperty =
            DependencyProperty.RegisterAttached(
                "IsEmpty",
                typeof(bool),
                typeof(PasswordHelper),
                new PropertyMetadata(true));

        public static bool GetIsEmpty(DependencyObject obj)
            => (bool)obj.GetValue(IsEmptyProperty);

        public static void SetIsEmpty(DependencyObject obj, bool value)
            => obj.SetValue(IsEmptyProperty, value);

        // Attached property để hook sự kiện PasswordChanged
        public static readonly DependencyProperty MonitorPasswordProperty =
            DependencyProperty.RegisterAttached(
                "MonitorPassword",
                typeof(bool),
                typeof(PasswordHelper),
                new PropertyMetadata(false, OnMonitorPasswordChanged));

        public static bool GetMonitorPassword(DependencyObject obj)
            => (bool)obj.GetValue(MonitorPasswordProperty);

        public static void SetMonitorPassword(DependencyObject obj, bool value)
            => obj.SetValue(MonitorPasswordProperty, value);

        private static void OnMonitorPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PasswordBox passwordBox)
            {
                if ((bool)e.NewValue)
                    passwordBox.PasswordChanged += PasswordBox_PasswordChanged;
                else
                    passwordBox.PasswordChanged -= PasswordBox_PasswordChanged;
            }
        }

        private static void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox)
                SetIsEmpty(passwordBox, passwordBox.Password.Length == 0);
        }
    }
}
