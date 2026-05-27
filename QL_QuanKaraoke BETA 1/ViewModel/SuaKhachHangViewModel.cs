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
    public class SuaKhachHangViewModel : BaseViewModel
    {
        // ===== THÔNG TIN KHÁCH HÀNG =====
        private KhachHang _khachHang;
        public KhachHang KhachHang
        {
            get => _khachHang;
            set { _khachHang = value; OnPropertyChanged(); }
        }

        // ===== HẠNG THÀNH VIÊN =====
        private ObservableCollection<HangThanhVien> _dsHangTV;
        public ObservableCollection<HangThanhVien> DSHangTV
        {
            get => _dsHangTV;
            set { _dsHangTV = value; OnPropertyChanged(); }
        }

        private HangThanhVien _hangDangChon;
        public HangThanhVien HangDangChon
        {
            get => _hangDangChon;
            set { _hangDangChon = value; OnPropertyChanged(); }
        }

        public void LoadThongTin()
        {
            QL_KhachHangUC ql = new QL_KhachHangUC();
            var qlVM = ql.DataContext as QL_KhachHangViewModel;

            KhachHang = new KhachHang();
            KhachHang = qlVM.SelectedKhachHang;

            HangDangChon = DSHangTV.FirstOrDefault(h => h.MaHang == KhachHang.MaHang);
        }
        public void LoadDanhSachHang()
        {
            var hangThanhViens = DataProvider.Ins.DB.HangThanhViens.Where(x => x.IsEnable == true);
            // Load hạng thành viên từ DB
            DSHangTV = new ObservableCollection<HangThanhVien>(
                hangThanhViens);

        }

        // ===== KIỂM TRA HỢP LỆ =====
        private bool KiemTraHopLe()
        {
            if (KhachHang == null) return false;

            if (string.IsNullOrWhiteSpace(KhachHang.HoTen)) return false;

            if (string.IsNullOrWhiteSpace(KhachHang.SoDienThoai)) return false;

            if (!Regex.IsMatch(KhachHang.SoDienThoai.Trim(), @"^0\d{9}$")) return false;

            if (HangDangChon == null) return false;

            return true;
        }

        // ===== COMMAND =====
        public ICommand ExitCommand { get; set; }
        public ICommand XacNhanSuaCommand { get; set; }

        // ===== CONSTRUCTOR =====
        public SuaKhachHangViewModel()
        {
            LoadDanhSachHang(); 

            ExitCommand = new RelayCommand<Window>(
                (p) => true,
                (p) => p.Close());

            XacNhanSuaCommand = new RelayCommand<Window>(
                // CanExecute
                (p) => KiemTraHopLe(),

                // Execute
                (p) =>
                {
                    bool trungSDT = DataProvider.Ins.DB.KhachHangs
                        .Any(kh => kh.SoDienThoai == KhachHang.SoDienThoai.Trim()
                                && kh.MaKhachHang != KhachHang.MaKhachHang);

                    if (trungSDT)
                    {
                        MessageBox.Show(
                            "Số điện thoại này đã được đăng ký bởi khách hàng khác !",
                            "Lỗi",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                        return;
                    }

                    MessageBoxResult confirm = MessageBox.Show(
                        "Xác nhận lưu thông tin khách hàng ?",
                        "Xác nhận",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (confirm == MessageBoxResult.Yes)
                    {
                        // Lấy nhân viên đang đăng nhập để ghi log
                        LoginWindow lg = new LoginWindow();
                        var lgVM = lg.DataContext as LoginViewModel;

                        // Lấy bản ghi từ DB theo mã khách
                        int maKH = KhachHang.MaKhachHang;
                        var kh = DataProvider.Ins.DB.KhachHangs
                            .FirstOrDefault(x => x.MaKhachHang == maKH);

                        if (kh == null)
                        {
                            MessageBox.Show("Không tìm thấy khách hàng trong hệ thống !");
                            return;
                        }

                        // Cập nhật thông tin
                        kh.HoTen = KhachHang.HoTen.Trim();
                        kh.SoDienThoai = KhachHang.SoDienThoai.Trim();
                        kh.MaHang = HangDangChon.MaHang;

                        DataProvider.Ins.DB.SaveChanges();

                        SystemLog systemLog = new SystemLog
                        {
                            MaNhanVien = lgVM.UserId,
                            ThoiGian = DateTime.Now,
                            HanhDong = "SUA_THONG_TIN_KHACH_HANG",
                            MoTa = $"Nhân viên mã {lgVM.UserId} đã sửa thông tin " +
                                         $"khách hàng mã {kh.MaKhachHang} vào lúc {DateTime.Now}"
                        };
                        DataProvider.Ins.DB.SystemLogs.Add(systemLog);
                        DataProvider.Ins.DB.SaveChanges();

                        MessageBox.Show(
                            "Cập nhật thông tin khách hàng thành công !",
                            "Thông báo",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);

                        p.Close();
                    }
                });
        }
    }
}
