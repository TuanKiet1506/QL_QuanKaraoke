using QL_QuanKaraoke_BETA_1.Data;
using QL_QuanKaraoke_BETA_1.Model;
using QL_QuanKaraoke_BETA_1.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace QL_QuanKaraoke_BETA_1.ViewModel
{
    public class MainViewModel : BaseViewModel
    {
        public ICommand LoadedWindowCommand { get; set; }
        public ICommand MaximizeCommand { get; set; }
        public ICommand MinimizeCommand { get; set; }
        public ICommand ExitCommand { get; set; }
        public ICommand TrangChuCommand { get; set; }
        public ICommand DatPhongCommand { get; set; }
        public ICommand LogoutCommand { get; set; }
        public ICommand QLNhanVienCommand { get; set; }
        public ICommand QLKhachHangCommand { get; set; }
        public ICommand QLHoaDonCommand { get; set; }
        public ICommand QLPhongCommand { get; set; }
        public ICommand QLBangGiaCommand { get; set; }
        public ICommand QLHangThanhVienCommand { get; set; }
        public ICommand QLDichVuCommand { get; set; }
        public ICommand QLHeThongCommand { get; set; }
        public ICommand QLDangNhapCommand { get; set; }

        DatPhongViewModel DatPhongViewModel { get; set; }
        TrangChuViewModel TrangChuViewModel { get; set; }
        QL_NhanVienViewModel QLNV_ViewModel { get; set; }
        QL_KhachHangViewModel QLKH_ViewModel { get; set; }
        QL_HoaDonViewModel QLHD_ViewModel { get; set; }
        QL_PhongViewModel QLP_ViewModel { get; set; }
        QL_BangGiaViewModel QLBG_ViewModel { get; set; }
        QL_HangThanhVienViewModel QLHTV_ViewModel { get; set; }
        QL_DichVuViewModel QLDV_ViewModel { get; set; }
        QL_HeThongViewModel QLHT_ViewModel { get; set; }
        QL_DangNhapViewModel QLDN_ViewModel { get; set; }

        private object _currentView;

        public object CurrentView
        {
            get { return _currentView; }
            set { _currentView = value; OnPropertyChanged(); }
        }
        private bool _IsAdmin; 
        public bool IsAdmin
        {
            get => _IsAdmin;
            set { _IsAdmin = value; OnPropertyChanged(); }
        }
        private void LuuLichSuDangXuat(LoginViewModel loginVM)
        {
            loginVM.LoginLogs.ThoiGianDangXuat = DateTime.Now;
            DataProvider.Ins.DB.LoginLogs.Add(loginVM.LoginLogs);
            DataProvider.Ins.DB.SaveChanges();
        }
        public MainViewModel() {
            LoadedWindowCommand = new RelayCommand<Window>((p) => { return true; }, (p) => {
                p.Hide();
                LoginWindow loginWindow = new LoginWindow();
                loginWindow.ShowDialog();

                var loginVM = loginWindow.DataContext as LoginViewModel;

                if (loginVM.IsLogin)
                {
                    DoiMatKhauWindow doiMatKhauWindow = new DoiMatKhauWindow();
                    var doiMatKhauVM = doiMatKhauWindow.DataContext as DoiMatKhauViewModel;
                    if (doiMatKhauVM.IsExit)
                    {
                        doiMatKhauWindow.Close();
                        p.Close();
                        return; 
                    }

                    p.Show();
                    
                    if(loginVM.maRole == 1 || loginVM.maRole == 3)
                    {
                        IsAdmin = true;
                    }
                    else
                    {
                        IsAdmin = false;
                    }
                }
                else
                {
                    p.Close(); 
                } 
            });


            DatPhongViewModel = new DatPhongViewModel();
            TrangChuViewModel = new TrangChuViewModel();
            QLNV_ViewModel = new QL_NhanVienViewModel();
            QLKH_ViewModel = new QL_KhachHangViewModel();
            QLHD_ViewModel = new QL_HoaDonViewModel();
            QLP_ViewModel = new QL_PhongViewModel();
            QLBG_ViewModel = new QL_BangGiaViewModel();
            QLHTV_ViewModel = new QL_HangThanhVienViewModel();
            QLDV_ViewModel = new QL_DichVuViewModel();
            QLHT_ViewModel = new QL_HeThongViewModel();
            QLDN_ViewModel = new QL_DangNhapViewModel();
            

            CurrentView = TrangChuViewModel; 

            ExitCommand = new RelayCommand<Window>((p) => { return true; }, (p) => {
                LoginWindow loginWindow;
                loginWindow = new LoginWindow();
                var loginVM = loginWindow.DataContext as LoginViewModel;

                LuuLichSuDangXuat(loginVM); 
                loginWindow.Close();
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
            TrangChuCommand = new RelayCommand<object>((p) => { return true; }, (p) => {
                CurrentView = TrangChuViewModel; 
            }); 
            DatPhongCommand = new RelayCommand<object>((p) => { return true; }, (p) => {
                CurrentView = DatPhongViewModel; 
            });
            QLNhanVienCommand = new RelayCommand<object>((p) => { return true; }, (p) =>
            {
                CurrentView = QLNV_ViewModel; 
            });
            QLKhachHangCommand = new RelayCommand<object>((p) => { return true; }, (p) =>
            {
                CurrentView = QLKH_ViewModel; 
            });
            QLHoaDonCommand = new RelayCommand<object>((p) => { return true; }, (p) =>
            {
                CurrentView = QLHD_ViewModel; 
            });
            QLPhongCommand = new RelayCommand<object>((p) => { return true; }, (p) =>
            {
                CurrentView = QLP_ViewModel; 
            });
            QLBangGiaCommand = new RelayCommand<object>((p) => { return true; }, (p) =>
            {
                CurrentView = QLBG_ViewModel; 
            });
            QLHangThanhVienCommand = new RelayCommand<object>((p) => { return true; }, (p) =>
            {
                CurrentView = QLHTV_ViewModel; 
            });
            QLDichVuCommand = new RelayCommand<object>((p) => { return true; }, (p) =>
            {
                CurrentView = QLDV_ViewModel; 
            });
            QLHeThongCommand = new RelayCommand<object>((p) => { return true; }, (p) =>
            {
                CurrentView = QLHT_ViewModel; 
            });
            QLDangNhapCommand = new RelayCommand<object>((p) => { return true; }, (p) =>
            {
                CurrentView = QLDN_ViewModel; 
            });
            LogoutCommand = new RelayCommand<Window>((p) => true, (p) =>
            {
                MessageBoxResult result = MessageBox.Show(
                    "Bạn có chắc chắn muốn đăng xuất ?",
                    "Xác nhận",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    LoginWindow loginWindow;

                    loginWindow = new LoginWindow();
                    var loginVM = loginWindow.DataContext as LoginViewModel;

                    LuuLichSuDangXuat(loginVM);
                    p.Hide();

                  //  LoginWindow loginWindow = new LoginWindow();
                    loginWindow.ShowDialog();

                    // Nếu người dùng tự nhiên nhấn Thoát giữa chừng thì đóng luôn MainWindow 
                    // Phân tích: Đóng luôn -> Thoát -> Lưu lại lịch sử đăng xuất 

                    if (loginVM.IsExit)
                    {
                        p.Close();
                        return; 
                    }

                    if (loginVM.IsLogin)
                    {
                        p.Show();

                        // Liên tục khai báo IsAdmin để phân quyền ngay sau khi đăng nhập 
                        if (loginVM.maRole == 1 || loginVM.maRole == 3)
                            IsAdmin = true;
                        else
                            IsAdmin = false;

                    }
                    else
                    {
                        p.Close();
                    }
                }
            });
        }
    }
}
