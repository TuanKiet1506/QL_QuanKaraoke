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
    public class ThanhToanViewModel : BaseViewModel
    {
        public bool isThanhToan {  get; set; }
        public DateTime GioRa {  get; set; }

        private double _tienPhong;

        public double TienPhong
        {
            get { return _tienPhong; }
            set { _tienPhong = value; OnPropertyChanged(); }
        }
        private double _tienDichVu;

        public double TienDichVu
        {
            get { return _tienDichVu; }
            set { _tienDichVu = value; OnPropertyChanged(); }
        }
        private string _hangThanhVien;

        public string HangThanhVien
        {
            get { return _hangThanhVien; }
            set { _hangThanhVien = value; OnPropertyChanged(); }
        }

        private float _phanTramGiamGia;

        public float PhanTramGiamGia
        {
            get { return _phanTramGiamGia; }
            set { _phanTramGiamGia = value; OnPropertyChanged(); }
        }
        private decimal _tongTien;

        public decimal TongTien
        {
            get { return _tongTien; }
            set { _tongTien = value; OnPropertyChanged(); }
        }
        private ObservableCollection<Phong> _danhSachPhong;

        public ObservableCollection<Phong> DanhSachPhong
        {
            get { return _danhSachPhong; }
            set { _danhSachPhong = value; }
        }

        private double TinhTienTheoDoan(int? maPhong, DateTime? batDau, DateTime? ketThuc)
        {
            if (maPhong == null || batDau == null || ketThuc == null)
                return 0;

            var db = DataProvider.Ins.DB;

            var phong = db.Phongs.FirstOrDefault(x => x.MaPhong == maPhong);
            if (phong == null || phong.MaLoaiPhong == null) return 0;

            int maLoaiPhong = phong.MaLoaiPhong.Value;

            var bangGia = db.BangGias
                .Where(x => x.MaLoaiPhong == maLoaiPhong)
                .ToList();

            if (bangGia.Count == 0) return 0;

            double tien = 0;

            DateTime current = batDau.Value;
            DateTime endTime = ketThuc.Value;

            while (current < endTime)
            {
                var khung = bangGia.FirstOrDefault(bg =>
                    bg.GioBatDau != null &&
                    bg.GioKetThuc != null &&
                    current.TimeOfDay >= bg.GioBatDau &&
                    current.TimeOfDay < bg.GioKetThuc
                );

                if (khung == null)
                {
                    // fallback tránh loop vô hạn
                    current = current.AddMinutes(1);
                    continue;
                }

                DateTime endKhung = current.Date.Add(khung.GioKetThuc.Value);

                if (khung.GioKetThuc < khung.GioBatDau)
                {
                    endKhung = endKhung.AddDays(1);
                }

                DateTime end = endTime < endKhung ? endTime : endKhung;

                double soPhut = (end - current).TotalMinutes;

                tien += (soPhut / 60.0) * (double)khung.Gia;

                current = end;
            }

            return tien;
        }
        public double TinhTienPhong(int maHoaDon)
        {
            var db = DataProvider.Ins.DB;

            var hoaDon = db.HoaDons.FirstOrDefault(x => x.MaHoaDon == maHoaDon);
            if (hoaDon == null || hoaDon.GioVao == null)
                return 0;

            DateTime gioBatDau = hoaDon.GioVao.Value;
            DateTime gioKetThuc = hoaDon.GioRa ?? DateTime.Now;
            GioRa = gioKetThuc; 

            var lichSu = db.LichSuChuyenPhongs
                .Where(x => x.MaHoaDon == maHoaDon)
                .OrderBy(x => x.ThoiGian)
                .ToList();

            var dsDoan = new List<(int? MaPhong, DateTime? BatDau, DateTime? KetThuc)>();

            DateTime? currentTime = gioBatDau;
            int? currentPhong;

            if (lichSu.Count > 0)
            {
                currentPhong = lichSu.First().MaPhongCu;
            }
            else
            {
                currentPhong = hoaDon.MaPhong;
            }

            foreach (var ls in lichSu)
            {
                if (currentPhong != null && currentTime < ls.ThoiGian)
                {
                    dsDoan.Add((currentPhong, currentTime, ls.ThoiGian));
                }

                currentPhong = ls.MaPhongMoi;
                currentTime = ls.ThoiGian;
            }

            // đoạn cuối
            if (currentPhong != null && currentTime < gioKetThuc)
            {
                dsDoan.Add((currentPhong, currentTime, gioKetThuc));
            }

            double tongTien = 0;

            foreach (var doan in dsDoan)
            {
                tongTien += TinhTienTheoDoan(doan.MaPhong, doan.BatDau, doan.KetThuc);
            }

            return tongTien;
        }
        public int LamTronTien(double soTien)
        {
            return (int)(Math.Round(soTien / 1000.0) * 1000);
        }
        public void LoadThongTin()
        {

            MoPhong moPhong = new MoPhong();
            var mpVM = moPhong.DataContext as MoPhongViewModel;

            int maHD = mpVM.LayMaHoaDon();

            moPhong.Close();

            TienPhong = LamTronTien(TinhTienPhong(maHD)); 

            LoadDSPhong();

            var hd = DataProvider.Ins.DB.HoaDons.Where(k => k.MaHoaDon == maHD).FirstOrDefault();
            if (hd != null)
            {
                int? makh = hd.MaKhachHang; 

                var kh = DataProvider.Ins.DB.KhachHangs.Where(x => x.MaKhachHang == makh).FirstOrDefault();

                HangThanhVien = kh.HangThanhVien.TenHang;
                PhanTramGiamGia =(float)kh.HangThanhVien.PhanTramGiamGia * 100; 
            }

            var tiendv = DataProvider.Ins.DB.ChiTietHoaDons
                .Where(x => x.MaHoaDon == maHD)
                .Sum(k => k.ThanhTien);

            if(tiendv != null)
            {
                TienDichVu = (double)tiendv; 
            }

            double tienTruocKhiGiamGia = TienPhong + TienDichVu;
            double tienGiamGia = tienTruocKhiGiamGia * (PhanTramGiamGia * 0.01);

            TongTien = LamTronTien(tienTruocKhiGiamGia -  tienGiamGia); 
        }
        public void LoadDSPhong()
        {
            // Hàm để lấy ra danh sách các phòng mà khách đã thuê 

            MoPhong moPhong = new MoPhong();
            var mpVM = moPhong.DataContext as MoPhongViewModel;

            int maHD = mpVM.LayMaHoaDon();

            var lichSuCP = DataProvider.Ins.DB.LichSuChuyenPhongs
                .Where(ls => ls.MaHoaDon == maHD);

            var maPhong_All = lichSuCP
                .Select(ls => ls.MaPhongCu)
                .Union(lichSuCP.Select(ls => ls.MaPhongMoi))
                .ToList();

            DanhSachPhong = new ObservableCollection<Phong>(); 

            foreach (var p in maPhong_All)
            {
                var phong = DataProvider.Ins.DB.Phongs.FirstOrDefault(x => x.MaPhong == p);
                if (phong != null)
                {
                    DanhSachPhong.Add(phong);
                }
            }

        }
        public ICommand XacNhanThanhToanCommand { get; set; }
        public ICommand TraPhongThanhToanSauCommand { get; set; }

        public ThanhToanViewModel() {
            

            XacNhanThanhToanCommand = new RelayCommand<Window>((p) => { return true; }, (p) => {

                MessageBoxResult message = MessageBox.Show("Bạn có chắc chắn muốn thanh toán ?"
                    , "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (message == MessageBoxResult.Yes)
                {
                    // 1. Lấy hóa đơn và chốt giờ ra 
                    // 2. Thay đổi trạng thái của hóa đơn thành đã thanh toán 
                    // 3. Cập nhật tổng tiền vào trong hóa đơn 
                    // 4. Cập nhật SystemLogs 
                    isThanhToan = true; 

                    MoPhong moPhong = new MoPhong();
                    var mpVM = moPhong.DataContext as MoPhongViewModel;

                    LoginWindow login = new LoginWindow();
                    var loginVM = login.DataContext as LoginViewModel;

                    QL_HoaDonUC hd = new QL_HoaDonUC();
                    var hdVM = hd.DataContext as QL_HoaDonViewModel;

                    TrangChuUC tc = new TrangChuUC();
                    var tcVM = tc.DataContext as TrangChuViewModel;

                    int maHD = mpVM.LayMaHoaDon();
                    tcVM.LoadThongKe(); 

                    var hoadon = DataProvider.Ins.DB.HoaDons.Where(h => h.MaHoaDon == maHD).FirstOrDefault();

                    hoadon.GioRa = GioRa;
                    hoadon.TrangThai = "DA_THANH_TOAN";
                    hoadon.TongTien = TongTien; 

                    DataProvider.Ins.DB.SaveChanges();

                    hdVM.LoadHoaDon(); 

                    SystemLog systemLog = new SystemLog
                    {
                        MaNhanVien = loginVM.UserId,
                        HanhDong = "THANH_TOAN",
                        ThoiGian = DateTime.Now,
                        MoTa = $"Nhân viên có mã {loginVM.UserId} đã thanh toán phòng {hoadon.Phong.TenPhong} vào lúc {DateTime.Now}"
                    }; 
                    DataProvider.Ins.DB.SystemLogs.Add(systemLog);
                    DataProvider.Ins.DB.SaveChanges(); 

                    p.Close();
                }
                else
                {
                    isThanhToan = false; 
                    p.Close(); 
                }
            });
            TraPhongThanhToanSauCommand = new RelayCommand<Window>((p) => { return true; }, (p) => {

                MessageBoxResult message = MessageBox.Show("Bạn có chắc chắn muốn thanh toán ?"
                    , "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (message == MessageBoxResult.Yes)
                {
                    // 1. Lấy hóa đơn và chốt giờ ra 
                    // 2. Thay đổi trạng thái của hóa đơn thành đã thanh toán 
                    // 3. Cập nhật tổng tiền vào trong hóa đơn 
                    // 4. Cập nhật SystemLogs 
                    isThanhToan = true; 

                    MoPhong moPhong = new MoPhong();
                    var mpVM = moPhong.DataContext as MoPhongViewModel;

                    LoginWindow login = new LoginWindow();
                    var loginVM = login.DataContext as LoginViewModel;

                    QL_HoaDonUC hd = new QL_HoaDonUC();
                    var hdVM = hd.DataContext as QL_HoaDonViewModel;

                    int maHD = mpVM.LayMaHoaDon();

                    var hoadon = DataProvider.Ins.DB.HoaDons.Where(h => h.MaHoaDon == maHD).FirstOrDefault();

                    hoadon.GioRa = GioRa;
                    hoadon.TrangThai = "CHUA_THANH_TOAN";
                    hoadon.TongTien = TongTien; 

                    DataProvider.Ins.DB.SaveChanges();

                    hdVM.LoadHoaDon();

                    SystemLog systemLog = new SystemLog
                    {
                        MaNhanVien = loginVM.UserId,
                        HanhDong = "HEN_THANH_TOAN",
                        ThoiGian = DateTime.Now,
                        MoTa = $"Nhân viên có mã {loginVM.UserId} đã hẹn thanh toán phòng {hoadon.Phong.TenPhong} vào lúc {DateTime.Now}"
                    }; 
                    DataProvider.Ins.DB.SystemLogs.Add(systemLog);
                    DataProvider.Ins.DB.SaveChanges(); 



                    p.Close();
                }
                else
                {
                    isThanhToan = false;     
                    p.Close(); 
                }
            });


        }
    }
}
