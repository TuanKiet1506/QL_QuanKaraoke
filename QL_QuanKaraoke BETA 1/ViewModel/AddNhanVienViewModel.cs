using QL_QuanKaraoke_BETA_1.Data;
using QL_QuanKaraoke_BETA_1.Model;
using QL_QuanKaraoke_BETA_1.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace QL_QuanKaraoke_BETA_1.ViewModel
{
    public class AddNhanVienViewModel : BaseViewModel
    {
        private string _soDienThoai;

        public string SoDienThoai
        {
            get { return _soDienThoai; }
            set { _soDienThoai = value; OnPropertyChanged(); }
        }
        private string _hoVaTen;

        public string HoVaTen
        {
            get { return _hoVaTen; }
            set { _hoVaTen = value;OnPropertyChanged(); }
        }
        private string _tenDangNhap;

        public string TenDangNhap
        {
            get { return _tenDangNhap; }
            set { _tenDangNhap = value; OnPropertyChanged(); }
        }
        private ObservableCollection<string> _danhSachChucVu;

        public ObservableCollection<string> DanhSachChucVu
        {
            get { return _danhSachChucVu; }
            set { _danhSachChucVu = value; OnPropertyChanged(); }
        }
        private string _selectedChucVu;

        public string SelectedChucVu
        {
            get { return _selectedChucVu; }
            set { _selectedChucVu = value; OnPropertyChanged(); }
        }

        private bool KiemTraToanDienDuLieu()
        {
            if(string.IsNullOrWhiteSpace(TenDangNhap)) return false;
            if (string.IsNullOrWhiteSpace(HoVaTen)) return false;
            if (string.IsNullOrEmpty(SoDienThoai)) return false;
            if (!Regex.IsMatch(SoDienThoai, @"^\d{10}$"))
                return false;

            return true; 
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
        public ICommand XacNhanThongTinCommand { get; set; }
        public ICommand ExitCommand { get; set; }

        public AddNhanVienViewModel()
        {
            DanhSachChucVu = new ObservableCollection<string>(
            DataProvider.Ins.DB.NhanViens
           .Select(x => x.ChucVu)
           .Distinct()
           .ToList());

            XacNhanThongTinCommand = new RelayCommand<Window>((p) => {

                if (!KiemTraToanDienDuLieu()) return false;
                return true; 
                
            }, (p) => {

                // Xác định Role cho nhân viên trước khi thêm vào 
                int maRole; 
                if(SelectedChucVu.ToString() == "Quản lý")
                {
                    maRole = 1;
                }
                else
                {
                    maRole = 2; 
                }
                // Trường hợp mà nhân viên có số điện thoại trùng với nhân viên cũ 
                int cnt = DataProvider.Ins.DB.NhanViens.Where(n => n.SoDienThoai == SoDienThoai).Count();
                if (cnt > 0)
                {
                    MessageBox.Show("Nhân viên có số điện thoại trùng với nhân viên cũ !");
                    return; 
                }

                    NhanVien nv = new NhanVien
                    {
                        TenDangNhap = TenDangNhap,
                        HoTen = HoVaTen,
                        SoDienThoai = SoDienThoai,
                        MatKhau = MaHoaMatKhau("123456"), // Mật khẩu mặc định cho nhân viên mới 123456
                        ChucVu = SelectedChucVu.ToString(),
                        TrangThai = 1,
                        MaRole = maRole
                    }; 

                DataProvider.Ins.DB.NhanViens.Add(nv);
                DataProvider.Ins.DB.SaveChanges();

                LoginWindow login = new LoginWindow();
                var loginVM = login.DataContext as LoginViewModel;

                SystemLog systemLog = new SystemLog
                {
                    MaNhanVien = loginVM.UserId,
                    ThoiGian = DateTime.Now,
                    HanhDong = "THEM_NHAN_VIEN",
                    MoTa = $"Nhân viên có mã {loginVM.UserId} vừa thêm một nhân viên mới vào lúc {DateTime.Now}"
                }; 
                DataProvider.Ins.DB.SystemLogs.Add(systemLog);
                DataProvider.Ins.DB.SaveChanges(); 

                MessageBox.Show("Đã thêm nhân viên mới thành công !"); 

                QL_NHANVIENUC ql = new QL_NHANVIENUC();
                var qlVM = ql.DataContext as QL_NhanVienViewModel;

                qlVM.DSNhanVien.Add(nv);

                p.Close(); 
            });
            ExitCommand = new RelayCommand<Window>((p) => {

                return true; 
                
            }, (p) => {
                p.Close(); 
            });



        }
    }
}
