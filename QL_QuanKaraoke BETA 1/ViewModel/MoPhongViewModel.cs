using QL_QuanKaraoke_BETA_1.Data;
using QL_QuanKaraoke_BETA_1.Model;
using QL_QuanKaraoke_BETA_1.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace QL_QuanKaraoke_BETA_1.ViewModel
{
    public class MoPhongViewModel : BaseViewModel
    {
        public int maHD {  get; set; }

        private bool _isExit;
        public bool IsExit
        {
            get => _isExit;
            set
            {
                _isExit = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsEditable));
            }
        }
        private bool _isAutoFill;

        public bool IsAutoFill
        {
            get { return _isAutoFill; }
            set { _isAutoFill = value; OnPropertyChanged(); }
        }
        public bool IsEditable => !IsAutoFill && !IsExit;

        private string _soDienThoai;

        public string SoDienThoai
        {
            get { return _soDienThoai; }
            set { _soDienThoai = value; OnPropertyChanged(); }
        }
        private string _hoTen;

        public string HoTen
        {
            get { return _hoTen; }
            set { _hoTen = value; OnPropertyChanged(); }
        }
        private float _phanTramGiamGia;

        public float PhanTramGiamGia
        {
            get { return _phanTramGiamGia; }
            set { _phanTramGiamGia = value; OnPropertyChanged(); }
        }
        private string _hangThanhVien;

        public string HangThanhVien
        {
            get { return _hangThanhVien; }
            set { _hangThanhVien = value; OnPropertyChanged(); }
        }

        private int _maKH;

        public int MaKH
        {
            get { return _maKH; }
            set { _maKH = value; OnPropertyChanged(); }
        }

        private HoaDon _hd;

        public HoaDon HD
        {
            get { return _hd; }
            set { _hd = value; OnPropertyChanged(); }
        }
        

        public ICommand CloseWindowCommand { get; set; }
        public ICommand AutoFillCommand { get; set; }
        public ICommand XacNhanCommand { get; set; }

        public void Reset()
        {
            IsExit = false;
            IsAutoFill = false;

            SoDienThoai = string.Empty;
            HoTen = string.Empty;
            HangThanhVien = string.Empty;
            PhanTramGiamGia = 0;
            MaKH = 0;

            OnPropertyChanged(nameof(IsEditable));
        }
        private bool KiemTraThongTin()
        {
            if (string.IsNullOrWhiteSpace(SoDienThoai))
                return false;

            // Chỉ cho phép đúng 10 chữ số
            if (!Regex.IsMatch(SoDienThoai, @"^\d{10}$"))
                return false;

            // 2. Kiểm tra họ tên
            if (string.IsNullOrWhiteSpace(HoTen))
                return false;

            return true;
        }
        public int LayMaHoaDon()
        {
            DatPhongUC datPhong = new DatPhongUC();
            var datPhongVM = datPhong.DataContext as DatPhongViewModel;

            int maPhong = datPhongVM.MaPhong; 

            var hd = DataProvider.Ins.DB.HoaDons.
            Where(x => x.MaPhong == maPhong && x.TrangThai == "DANG_MO")
            .FirstOrDefault();


            return hd.MaHoaDon; 

        }
        public MoPhongViewModel()
        {

            CloseWindowCommand = new RelayCommand<Window>((p) => { return true; }, (p) => {
                IsExit = true;
                p.Close();

            });
            AutoFillCommand = new RelayCommand<Window>((p) => { return true; }, (p) => {
                // Tìm khách hàng trong database -> Nếu thấy thì đó là khách quen !
                var customer = DataProvider.Ins.DB.KhachHangs.Where(kh => kh.SoDienThoai == SoDienThoai).FirstOrDefault();
                if (customer != null)
                {
                    IsAutoFill = true;
                    OnPropertyChanged(nameof(IsEditable));
                    HoTen = customer.HoTen;
                    HangThanhVien = customer.HangThanhVien.TenHang;
                    PhanTramGiamGia = (float)customer.HangThanhVien.PhanTramGiamGia * 100;
                    MaKH = customer.MaKhachHang;
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(SoDienThoai)) return; 
                    MessageBox.Show($"Không tìm thấy khách hàng có số điện thoại: {SoDienThoai.ToString()}, Vui lòng nhập tay !");
                    IsAutoFill = false;

                    // Nếu không Autofill được -> Khách mới hoàn toàn -> Khách chắc chắn không phải
                    // là khách VIP/Silver => Hạng luôn là 3, % giảm giá là 0 
                    // => Gắn cứng hai thông tin này

                    HangThanhVien = 3 + "";
                    PhanTramGiamGia = 0;
                }
            });
            XacNhanCommand = new RelayCommand<Window>((p) => { return true; }, (p) => {
                // Thêm thông tin khách hàng đó vào bảng khách hàng 

                // Kiểm tra khách đã Autofill chưa -> Nếu rồi thì không cần truy vấn lại tốn thời gian
                if (IsAutoFill)
                {
                    LoginWindow loginWindow = new LoginWindow();
                    var loginVM = loginWindow.DataContext as LoginViewModel;

                    DatPhongUC datPhong = new DatPhongUC();
                    var datPhongVM = datPhong.DataContext as DatPhongViewModel;

                    QL_HoaDonUC hoadon = new QL_HoaDonUC();
                    var hoadonVM = hoadon.DataContext as QL_HoaDonViewModel;

                    // Nếu Autofill rồi thì không cần insert do có thông tin rồi
                    // Các thứ cần làm sau khi ấn Xác nhận
                    // 1. Tạo một hóa đơn gồm mã phòng, mã kh, mã nv, giờ vào rồi insert vào trước
                    // 2. Giao diện thay đổi: Đổi màu trạng thái, ghi vào số giờ khách đang trong phòng
                    // 3. Lưu vào bảng SystemLogs -> Nhân viên mở phòng vào giờ đó (insert vào)

                    HD = new HoaDon
                    {
                        MaPhong = datPhongVM.MaPhong,
                        MaKhachHang = MaKH,
                        MaNhanVien = loginVM.UserId,
                        GioVao = DateTime.Now,
                        TrangThai = "DANG_MO", 
                        IsDeleted = false,
                    }; 
  
                    DataProvider.Ins.DB.HoaDons.Add(HD);
                    DataProvider.Ins.DB.SaveChanges();

                    hoadonVM.LoadHoaDon(); 

                    // Lưu vào bảng SystemLogs
                    SystemLog systemLog = new SystemLog
                    {
                        MaNhanVien = loginVM.UserId,
                        HanhDong = "MO_PHONG",
                        ThoiGian = DateTime.Now,
                        MoTa = $"Nhân viên mở phòng {datPhongVM.SelectedPhong.TenPhong} vào lúc {DateTime.Now}"
                    }; 
                    DataProvider.Ins.DB.SystemLogs.Add(systemLog);
                    DataProvider.Ins.DB.SaveChanges(); 


                    loginWindow.Close();
                    p.Close(); 
                }
                else
                {

                    if (!KiemTraThongTin())
                    {
                        MessageBox.Show("Vui lòng nhập đúng thông tin !!!"); 
                        return;
                    }

                    LoginWindow loginWindow = new LoginWindow();
                    var loginVM = loginWindow.DataContext as LoginViewModel;

                    DatPhongUC datPhong = new DatPhongUC();
                    var datPhongVM = datPhong.DataContext as DatPhongViewModel;

                    MenuPhongTrong mn = new MenuPhongTrong();
                    var mnVM = mn.DataContext as MenuPhongTrongViewModel;

                    QL_HoaDonUC hoadon = new QL_HoaDonUC();
                    var hoadonVM = hoadon.DataContext as QL_HoaDonViewModel;


                    KhachHang kh = new KhachHang
                    {
                        HoTen = HoTen,
                        SoDienThoai = SoDienThoai,
                        MaHang = 3 // gán cứng mã hạng là 3 do khách là khách mới, hạn chế việc nhân viên tự ý thay đổi
                    }; 
                    DataProvider.Ins.DB.KhachHangs.Add(kh);
                    DataProvider.Ins.DB.SaveChanges();

                    HD = new HoaDon
                    {
                        MaPhong = datPhongVM.MaPhong,
                        MaKhachHang = kh.MaKhachHang,
                        MaNhanVien = loginVM.UserId,
                        GioVao = DateTime.Now,
                        TrangThai = "DANG_MO", 
                        IsDeleted = false,
                    };
                    DataProvider.Ins.DB.HoaDons.Add(HD);
                    DataProvider.Ins.DB.SaveChanges();

                    hoadonVM.LoadHoaDon();


                    SystemLog systemLog = new SystemLog
                    {
                        MaNhanVien = loginVM.UserId,
                        HanhDong = "MO_PHONG",
                        ThoiGian = DateTime.Now,
                        MoTa = $"Nhân viên mở phòng {datPhongVM.SelectedPhong.TenPhong} vào lúc {DateTime.Now}"
                    };
                    DataProvider.Ins.DB.SystemLogs.Add(systemLog);
                    DataProvider.Ins.DB.SaveChanges();

                    MaKH = kh.MaKhachHang;
                //    mnVM.isMoPhong = false; 
                    p.Close(); 
                }
                Reset(); 
            });
        }
    }
}
