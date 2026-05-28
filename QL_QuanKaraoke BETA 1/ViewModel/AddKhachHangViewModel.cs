using QL_QuanKaraoke_BETA_1.Data;
using QL_QuanKaraoke_BETA_1.Model;
using QL_QuanKaraoke_BETA_1.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace QL_QuanKaraoke_BETA_1.ViewModel
{
    public class AddKhachHangViewModel : BaseViewModel
    {
        private string _hoTen;
        public string HoTen
        {
            get => _hoTen;
            set { _hoTen = value; OnPropertyChanged(); }
        }

        private string _soDienThoai;
        public string SoDienThoai
        {
            get => _soDienThoai;
            set { _soDienThoai = value; OnPropertyChanged(); }
        }

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

        public ICommand XacNhanCommand { get; set; }
        public ICommand ExitCommand { get; set; }

        public void LoadThongTinHang()
        {
            var hangThanhViens = DataProvider.Ins.DB.HangThanhViens.Where(x => x.IsEnable == true);

            DSHangTV = new ObservableCollection<HangThanhVien>(
               hangThanhViens);
        }

        public AddKhachHangViewModel()
        {
            LoadThongTinHang(); 

            HangDangChon = DSHangTV.FirstOrDefault();

            XacNhanCommand = new RelayCommand<Window>(
                (p) => KiemTraHopLe(),
                (p) =>
                {
                    bool trungSDT = DataProvider.Ins.DB.KhachHangs
                        .Any(kh => kh.SoDienThoai == SoDienThoai.Trim());

                    if (trungSDT)
                    {
                        MessageBox.Show(
                            "Số điện thoại này đã được đăng ký !",
                            "Lỗi",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                        return;
                    }

                    MessageBoxResult confirm = MessageBox.Show(
                        "Xác nhận thêm khách hàng mới ?",
                        "Xác nhận",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (confirm == MessageBoxResult.Yes)
                    {
                        var khachHangMoi = new KhachHang
                        {
                            HoTen = HoTen.Trim(),
                            SoDienThoai = SoDienThoai.Trim(),
                            MaHang = HangDangChon.MaHang,
                        };

                        DataProvider.Ins.DB.KhachHangs.Add(khachHangMoi);
                        DataProvider.Ins.DB.SaveChanges();

                        MessageBox.Show(
                            "Thêm khách hàng thành công !",
                            "Thông báo",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);

                        // Load lại danh sách khách hàng
                        QL_KhachHangUC ql = new QL_KhachHangUC();
                        var qlVM = ql.DataContext as QL_KhachHangViewModel;

                        TrangChuUC tc = new TrangChuUC();
                        var tcVM = tc.DataContext as TrangChuViewModel;

                        tcVM.LoadThongKe(); 

                        qlVM.DanhSachKhachHang.Add(khachHangMoi); 

                        p?.Close();
                    }
                });

            ExitCommand = new RelayCommand<Window>((p) => { return true; }, (p) =>
            {
                p.Close();
            }); 
        }

        // ===== KIỂM TRA HỢP LỆ =====
        private bool KiemTraHopLe()
        {
            if (string.IsNullOrWhiteSpace(HoTen))
                return false;

            if (string.IsNullOrWhiteSpace(SoDienThoai))
                return false;

            if (!System.Text.RegularExpressions.Regex.IsMatch(SoDienThoai.Trim(), @"^0\d{9}$"))
                return false;

            // Phải chọn hạng thành viên
            if (HangDangChon == null)
                return false;

            return true;
        }
    }
}
