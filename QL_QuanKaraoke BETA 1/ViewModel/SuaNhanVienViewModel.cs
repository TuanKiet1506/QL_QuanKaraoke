using QL_QuanKaraoke_BETA_1.Data;
using QL_QuanKaraoke_BETA_1.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace QL_QuanKaraoke_BETA_1.ViewModel
{
    public class SuaNhanVienViewModel : BaseViewModel
    {
        private NhanVien _nhanVien;

        public NhanVien NhanVien
        {
            get { return _nhanVien; }
            set { _nhanVien = value; OnPropertyChanged();  }
        }
        private ObservableCollection<string> _listChucVu;

        public ObservableCollection<string> ListChucVu
        {
            get { return _listChucVu; }
            set { _listChucVu = value; OnPropertyChanged(); }
        }


        public void LoadThongTin()
        {
            QL_NHANVIENUC ql = new QL_NHANVIENUC();
            var qlVM = ql.DataContext as QL_NhanVienViewModel;

            NhanVien = new NhanVien();
            NhanVien = qlVM.SelectedNhanVien; 
        }
        private bool KiemTraToanDienDuLieu()
        {
            if (NhanVien == null) return false; 
            if (string.IsNullOrWhiteSpace(NhanVien.TenDangNhap)) return false;
            if (string.IsNullOrWhiteSpace(NhanVien.HoTen)) return false;
            if (string.IsNullOrEmpty(NhanVien.SoDienThoai)) return false;
            if (!Regex.IsMatch(NhanVien.SoDienThoai, @"^\d{10}$"))
                return false;

            return true;
        }
        public ICommand ExitCommand { get; set; }
        public ICommand XacNhanThongTinCommand { get; set; }
        public SuaNhanVienViewModel() {
        

            ListChucVu = new ObservableCollection<string>
            {
                "Quản lý", "Nhân viên"
            };

            ExitCommand = new RelayCommand<Window>((p) => { return true; }, (p) => {
                p.Close(); 
            });
            XacNhanThongTinCommand = new RelayCommand<Window>((p) => {
                if (!KiemTraToanDienDuLieu())
                    return false;
                return true; 
            }, (p) => {
                LoginWindow lg = new LoginWindow();
                var lgVM = lg.DataContext as LoginViewModel;

                int manv = NhanVien.MaNhanVien; 
                var nv = DataProvider.Ins.DB.NhanViens
                .Where(x => x.MaNhanVien == manv)
                .FirstOrDefault();

                nv.TenDangNhap = NhanVien.TenDangNhap; 
                nv.ChucVu = NhanVien.ChucVu;
                nv.HoTen = NhanVien.HoTen;
                nv.SoDienThoai = NhanVien.SoDienThoai;

                if (NhanVien.ChucVu == "Quản lý") nv.MaRole = 1;
                else nv.MaRole = 2; 

                DataProvider.Ins.DB.SaveChanges();

                SystemLog system = new SystemLog
                {
                    MaNhanVien = lgVM.UserId,
                    ThoiGian = DateTime.Now,
                    HanhDong = "SUA_THONG_TIN_NHAN_VIEN",
                    MoTa = $"Nhân viên có mã {lgVM.UserId} đã sửa thông tin nhân viên mã {nv.MaNhanVien} vào lúc {DateTime.Now}"
                }; 
                DataProvider.Ins.DB.SystemLogs.Add(system);
                DataProvider.Ins.DB.SaveChanges(); 

                p.Close();
            }); 
        }
    }
}
