using QL_QuanKaraoke_BETA_1.Data;
using QL_QuanKaraoke_BETA_1.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace QL_QuanKaraoke_BETA_1.ViewModel
{
    public class LoginViewModel : BaseViewModel
    {
        public bool IsLockedAccount { get; set; }
        public bool IsLogin { get; set; }
        public bool IsExit {  get; set; }
        public int? maRole {  get; set; }
        public int UserId { get; set; }
        public ICommand QuenMatKhauCommand { get; set; }
        public ICommand MaximizeCommand { get; set; }
        public ICommand MinimizeCommand { get; set; }
        public ICommand ExitCommand { get; set; }
        public ICommand LoginCommand { get; set; }
        public ICommand PasswordChangedCommand { get; set; }
        private string _userName;
        public string UserName
        {
            get => _userName;
            set { _userName = value; OnPropertyChanged(); }
        }
        private string _password;
        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(); }
        }
        private LoginLog _loginLogs; 
        public LoginLog LoginLogs
        {
            get => _loginLogs;
            set => _loginLogs = value; 
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
        private void LuuLichSuDangNhap(int maNhanVien)
        {
            // Đăng nhập xong rồi thì phải lưu lại lịch sử đăng nhập trong dữ liệu (Bảng LoginLogs)
            // Tạo mới thuộc tính LoginLogs 
            LoginLogs = new LoginLog()
            {
                MaNhanVien = maNhanVien,
                ThoiGianDangNhap = DateTime.Now,
            }; 
        }
        private bool KiemTraDangNhapLanDau(int maNhanVien)
        {
            int count = DataProvider.Ins.DB.LoginLogs.Where(i => i.MaNhanVien == maNhanVien).Count();
            return count == 0; 
        }
        public LoginViewModel() { 
            IsLogin = false; 
            ExitCommand = new RelayCommand<Window>((p) => { return true; }, (p) => {
                IsExit = true;
                p.Close();
            });
            MinimizeCommand = new RelayCommand<Window>((p) => { return true; }, (p) => {
                p.WindowState = WindowState.Minimized;
            });
            MaximizeCommand = new RelayCommand<Window>((p) => { return true; }, (p) => {
                if (p.WindowState == WindowState.Maximized)
                    p.WindowState = WindowState.Normal;
                else
                    p.WindowState = WindowState.Maximized;
            });

            PasswordChangedCommand = new RelayCommand<PasswordBox>((p) => { return true; }, (p) => {
                Password = p.Password;
            });

            LoginCommand = new RelayCommand<Window>((p) => {
                if (Password == null || UserName == null) return false;
                return true; 
            }, (p) => {
                
                string passwordHash = MaHoaMatKhau(Password);

                var user = DataProvider.Ins.DB.NhanViens.Where(sv => sv.TenDangNhap == UserName
                                                              && sv.MatKhau == passwordHash).FirstOrDefault();


                if(user != null)
                {
                    // Kiểm tra tài khoản xem có bị Admin khóa không ?
                    IsLockedAccount = user.TrangThai == 0 || user.TrangThai == 2;
                    // Nếu acc bị khóa 
                    if (IsLockedAccount)
                    {
                        MessageBox.Show("Tài khoản này đã bị khóa !"
                            , "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Error); 
                        p.Close();
                        return; 
                    }

                    IsLogin = true;

                    maRole = user.MaRole;

                    UserId = user.MaNhanVien; 

                    LuuLichSuDangNhap(user.MaNhanVien);

                    // Nếu nhân viên đăng nhập lần đầu -> Nhân viên mới
                    // Bắt thay đổi mật khẩu ngay 
                    // Hàm kiểm tra xem nhân viên đã đăng nhập lần đầu ?
                    if (KiemTraDangNhapLanDau(user.MaNhanVien))
                    {
                        p.Hide(); 
                        DoiMatKhauWindow doiMatKhauWindow = new DoiMatKhauWindow();
                        doiMatKhauWindow.ShowDialog();

                    }

                    p.Close();
                }
                else
                {
                    IsLogin = false;
                    MessageBox.Show("Sai tên đăng nhập hoặc mật khẩu!",
                       "Thông báo",
                       MessageBoxButton.OK,
                       MessageBoxImage.Warning);
                }
                
            });

            QuenMatKhauCommand = new RelayCommand<object>((p) => { return true; }, (p) => {
                QuenMatKhauWindow quenMatKhauWindow = new QuenMatKhauWindow(); 
                quenMatKhauWindow.ShowDialog();
            });

        }
    }
}
