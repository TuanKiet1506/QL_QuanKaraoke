using QL_QuanKaraoke_BETA_1.Data;
using QL_QuanKaraoke_BETA_1.Model;
using QL_QuanKaraoke_BETA_1.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace QL_QuanKaraoke_BETA_1.ViewModel
{
    public class QL_NhanVienViewModel : BaseViewModel
    {
        private ObservableCollection<NhanVien> _dsNhanVien;

        public ObservableCollection<NhanVien> DSNhanVien
        {
            get { return _dsNhanVien; }
            set { _dsNhanVien = value; OnPropertyChanged(); }
        }
        private NhanVien _selectedNhanVien;

        public NhanVien SelectedNhanVien
        {
            get { return _selectedNhanVien; }
            set { _selectedNhanVien = value; OnPropertyChanged(); }
        }
        private ObservableCollection<string> _dsTrangThai;

        public ObservableCollection<string> DSTrangThai
        {
            get { return _dsTrangThai; }
            set { _dsTrangThai = value; OnPropertyChanged(); }
        }
        private string _trangThaiDangChon;

        public string TrangThaiDangChon
        {
            get => _trangThaiDangChon;
            set
            {
                _trangThaiDangChon = value;
                OnPropertyChanged();

                DanhSachNhanVienView.Refresh();
            }
        }
        private string _tuKhoa;
        public string TuKhoa
        {
            get => _tuKhoa;
            set
            {
                _tuKhoa = value;
                OnPropertyChanged();
                DanhSachNhanVienView.Refresh();
            }
        }
        private bool LocNhanVien(object obj)
        {
            if (obj is NhanVien nv)
            {
                // FILTER TỪ KHÓA
                bool hopLeTen = true;

                if (!string.IsNullOrWhiteSpace(TuKhoa))
                {
                    hopLeTen = nv.HoTen
                        .ToLower()
                        .Contains(TuKhoa.ToLower());
                }

                // FILTER TRẠNG THÁI
                bool hopLeTrangThai = true;

                switch (TrangThaiDangChon)
                {
                    case "Đang làm việc":
                        hopLeTrangThai = nv.TrangThai == 1;
                        break;

                    case "Đã nghỉ việc":
                        hopLeTrangThai = nv.TrangThai == 2;
                        break;

                    case "Bị khóa tài khoản":
                        hopLeTrangThai = nv.TrangThai == 0;
                        break;

                    default:
                        break;
                }

                return hopLeTen && hopLeTrangThai;
            }

            return false;
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
        public ICollectionView DanhSachNhanVienView { get; set; }
        public ICommand XacNhanNghiViecCommand { get; set; }
        public ICommand ResetMatKhauCommand { get; set; }
        public ICommand KhoaTaiKhoanCommand { get; set; }
        public ICommand MoKhoaCommand { get; set; }
        public ICommand ThemCommand { get; set; }
        public ICommand SuaCommand { get; set; }

        public QL_NhanVienViewModel()
        {
            DSNhanVien = new ObservableCollection<NhanVien>(DataProvider.Ins.DB.NhanViens);
            DanhSachNhanVienView = CollectionViewSource.GetDefaultView(DSNhanVien);

            LoginWindow login = new LoginWindow();
            var loginVM = login.DataContext as LoginViewModel;

            DanhSachNhanVienView.Filter = LocNhanVien; 

            DSTrangThai = new ObservableCollection<string>
            {
                "Tất cả", "Đang làm việc", "Đã nghỉ việc", "Bị khóa tài khoản"
            };

            TrangThaiDangChon = "Tất cả";

            XacNhanNghiViecCommand = new RelayCommand<object>((p) => {
                return SelectedNhanVien != null
            && SelectedNhanVien.TrangThai != 2
            && SelectedNhanVien.MaRole != 3
            && SelectedNhanVien.MaRole != loginVM.maRole;
            }
            , (p) =>
            {
                MessageBoxResult messageBoxResult = MessageBox.Show("Xác nhận nghỉ việc cho nhân viên này ?"
                    , "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    int manv = SelectedNhanVien.MaNhanVien;

                    var nv = DataProvider.Ins.DB.NhanViens
                    .Where(x => x.MaNhanVien == manv)
                    .FirstOrDefault();

                    nv.TrangThai = 2; 

                    DataProvider.Ins.DB.SaveChanges();

                    // Nếu role là admin thì không có chuyện nghỉ việc  
                    // Nếu nhân viên đã nghỉ việc rồi thì không cần nghỉ việc nữa

                    SystemLog systemLog = new SystemLog
                    {
                        MaNhanVien = loginVM.UserId,
                        ThoiGian = DateTime.Now,
                        HanhDong = "NGHI_VIEC_NHAN_VIEN",
                        MoTa = $"Nhân viên có mã {loginVM.UserId} vừa cho nhân viên mã {nv.MaNhanVien} nghỉ việc vào lúc {DateTime.Now}"
                    };
                    DataProvider.Ins.DB.SystemLogs.Add(systemLog);
                    DataProvider.Ins.DB.SaveChanges(); 

                }
            });
            ResetMatKhauCommand = new RelayCommand<object>((p) => {
                return SelectedNhanVien != null
           && SelectedNhanVien.TrangThai != 2
           && SelectedNhanVien.MaRole != 3
           && SelectedNhanVien.MaRole != loginVM.maRole;
            }
            , (p) =>
            {
                MessageBoxResult messageBoxResult = MessageBox.Show("Xác nhận đổi mật khẩu cho nhân viên ?"
                    , "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    int manv = SelectedNhanVien.MaNhanVien;

                    var nv = DataProvider.Ins.DB.NhanViens
                    .Where(x => x.MaNhanVien == manv)
                    .FirstOrDefault();

                    // Reset mật khẩu mặc định là 123456 
                    string passhash = MaHoaMatKhau("123456"); 

                    nv.MatKhau = passhash;
                    DataProvider.Ins.DB.SaveChanges();

                    SystemLog systemLog = new SystemLog
                    {
                        MaNhanVien = loginVM.UserId,
                        ThoiGian = DateTime.Now,
                        HanhDong = "RESET_MAT_KHAU",
                        MoTa = $"Nhân viên có mã {loginVM.UserId} vừa reset mật khẩu cho nhân viên mã {nv.MaNhanVien} vào lúc {DateTime.Now}"
                    };
                    DataProvider.Ins.DB.SystemLogs.Add(systemLog);
                    DataProvider.Ins.DB.SaveChanges();

                    MessageBox.Show("Đã reset mật khẩu lại thành công ! Mật khẩu mặc định là 123456"); 
                }
            });
            KhoaTaiKhoanCommand = new RelayCommand<object>((p) => {
                return SelectedNhanVien != null
           && SelectedNhanVien.TrangThai != 0
           && SelectedNhanVien.MaRole != 3
           && SelectedNhanVien.MaRole != loginVM.maRole;
            }
            , (p) =>
            {
                MessageBoxResult messageBoxResult = MessageBox.Show("Xác nhận khóa tài khoản của nhân viên ?"
                    , "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    int manv = SelectedNhanVien.MaNhanVien;

                    var nv = DataProvider.Ins.DB.NhanViens
                    .Where(x => x.MaNhanVien == manv)
                    .FirstOrDefault();

                    nv.TrangThai = 0;
                    DataProvider.Ins.DB.SaveChanges();

                    SystemLog systemLog = new SystemLog
                    {
                        MaNhanVien = loginVM.UserId,
                        ThoiGian = DateTime.Now,
                        HanhDong = "KHOA_TAI_KHOAN",
                        MoTa = $"Nhân viên có mã {loginVM.UserId} vừa cho khóa tài khoản của nhân viên mã {nv.MaNhanVien} vào lúc {DateTime.Now}"
                    };
                    DataProvider.Ins.DB.SystemLogs.Add(systemLog);
                    DataProvider.Ins.DB.SaveChanges();


                    MessageBox.Show("Đã khóa tài khoản của nhân viên này"); 
                }
            });
            MoKhoaCommand = new RelayCommand<object>((p) => {
                return SelectedNhanVien != null
                 && (SelectedNhanVien.TrangThai == 0);
            }
            , (p) =>
            {
                MessageBoxResult messageBoxResult = MessageBox.Show("Xác nhận mở khóa tài khoản của nhân viên ?"
                    , "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    int manv = SelectedNhanVien.MaNhanVien;

                    var nv = DataProvider.Ins.DB.NhanViens
                    .Where(x => x.MaNhanVien == manv)
                    .FirstOrDefault();

                    nv.TrangThai = 1;
                    DataProvider.Ins.DB.SaveChanges();

                    SystemLog systemLog = new SystemLog
                    {
                        MaNhanVien = loginVM.UserId,
                        ThoiGian = DateTime.Now,
                        HanhDong = "MO_KHOA_NHAN_VIEN",
                        MoTa = $"Nhân viên có mã {loginVM.UserId} vừa mở khóa cho nhân viên mã {nv.MaNhanVien} vào lúc {DateTime.Now}"
                    };
                    DataProvider.Ins.DB.SystemLogs.Add(systemLog);
                    DataProvider.Ins.DB.SaveChanges();


                    MessageBox.Show("Đã mở khóa tài khoản của nhân viên này"); 
                }
            });
            ThemCommand = new RelayCommand<object>((p) => {
                return true;
            }
            , (p) =>
            {
                AddNhanVien addNhanVien = new AddNhanVien();
                addNhanVien.ShowDialog();

            });
            SuaCommand = new RelayCommand<object>((p) => {
                return SelectedNhanVien != null
           && SelectedNhanVien.TrangThai != 2
           && SelectedNhanVien.MaRole != 3
           && SelectedNhanVien.MaRole != loginVM.maRole;
            }
            , (p) =>
            {
                SuaNhanVien suaNhanVien = new SuaNhanVien();

                var qlVM = suaNhanVien.DataContext as SuaNhanVienViewModel;

                qlVM.LoadThongTin();

                suaNhanVien.ShowDialog();

                DanhSachNhanVienView.Refresh();
            }); 
        }
    }
}
