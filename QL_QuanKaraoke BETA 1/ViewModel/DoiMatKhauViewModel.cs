using QL_QuanKaraoke_BETA_1.Data;
using QL_QuanKaraoke_BETA_1.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;

namespace QL_QuanKaraoke_BETA_1.ViewModel
{
    public class DoiMatKhauViewModel : BaseViewModel
    {
        public ICommand ExitCommand { get; set; }
        public ICommand PasswordChangedCommand { get; set; }
        public ICommand PasswordConfirmChangedCommand { get; set; }
        public ICommand XacNhanCommand { get; set; }
        public int UserId { get; set; }
        public bool IsExit { get; set; }
        private string _matKhau;

        public string MatKhau
        {
            get { return _matKhau; }
            set { _matKhau = value; OnPropertyChanged(); }
        }
        private string _xacNhanMatKhau;

        public string XacNhanMatKhau
        {
            get { return _xacNhanMatKhau; }
            set { _xacNhanMatKhau = value;OnPropertyChanged(); }
        }
        public string MaHoaMatKhau(string password)
        {
            MD5 md5 = MD5.Create();

            byte[] inputBytes = Encoding.UTF8.GetBytes(password);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            StringBuilder builder = new StringBuilder();

            foreach (byte b in hashBytes)
            {
                builder.Append(b.ToString("x2"));
            }

            return builder.ToString();
        }
        private bool KiemTraDoGiongNhauPassword(string password, string passwordConfirm)
        {
            if (passwordConfirm != password)
                return false;
            return true; 
        }
        private bool KiemTraDoDaiPassword(string password)
        {
            // Độ dài mật khẩu nên có ít nhất 5 kí tự 
            return password.Length >= 5; 
        }
        private void GetUserID()
        {
            QuenMatKhauWindow quenMatKhauWindow;
            quenMatKhauWindow = new QuenMatKhauWindow();
            var quenMatKhauVM = quenMatKhauWindow.DataContext as QuenMatKhauViewModel;

            UserId = quenMatKhauVM.UserID;

            quenMatKhauWindow.Close();
        }
        public DoiMatKhauViewModel()
        {
            ExitCommand = new RelayCommand<Window>((p) => {
                return true; 
            }, (p) => {
                IsExit = true;
                p.Close(); 
            });
            PasswordChangedCommand = new RelayCommand<PasswordBox>((p) => {
                return true; 
            }, (p) => {
                MatKhau = p.Password.ToString(); 
            });
            PasswordConfirmChangedCommand = new RelayCommand<PasswordBox>((p) => {
                return true; 
            }, (p) => {
                XacNhanMatKhau = p.Password.ToString(); 
            });
            XacNhanCommand = new RelayCommand<Window>((p) => {
                if (string.IsNullOrWhiteSpace(MatKhau) || string.IsNullOrWhiteSpace(XacNhanMatKhau)
                || !KiemTraDoGiongNhauPassword(MatKhau, XacNhanMatKhau) || !KiemTraDoDaiPassword(XacNhanMatKhau))

                    return false;

                return true; 
            }, (p) => {

                // Tìm đúng nhân viên nào đang cần đổi mật khẩu 
                LoginWindow loginWindow;
                loginWindow = new LoginWindow();
                var loginVM = loginWindow.DataContext as LoginViewModel;

                if(!loginVM.IsLogin)
                GetUserID();
                else
                {
                    // Trường hợp mà nhân viên không quên mật khẩu 
                    // Nhưng đăng nhập lần đầu cần phải đổi mật khẩu 
                    UserId = loginVM.UserId; 
                    loginWindow.Close();
                }


                var user = DataProvider.Ins.DB.NhanViens.Where(sv => sv.MaNhanVien == UserId).FirstOrDefault();

                // Mã hóa mật khẩu trước khi cho vào cơ sở dữ liệu !

                string passwordhash = MaHoaMatKhau(XacNhanMatKhau);

                user.MatKhau = passwordhash; 

                DataProvider.Ins.DB.SaveChanges(); 
                
                MessageBox.Show("Đã đổi mật khẩu thành công, bạn có thể đăng nhập lại !!!"
                    , "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                // Đóng window lại 
                loginWindow.Close();
                p.Close(); 
            });
            
        }
    }
}
