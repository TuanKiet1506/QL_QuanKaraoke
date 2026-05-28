using QL_QuanKaraoke_BETA_1.Data;
using QL_QuanKaraoke_BETA_1.Model;
using QL_QuanKaraoke_BETA_1.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace QL_QuanKaraoke_BETA_1.ViewModel
{
    public class QL_HangThanhVienViewModel : BaseViewModel
    {
        // ===== DANH SÁCH HẠNG THÀNH VIÊN =====
        private ObservableCollection<HangThanhVien> _dsHangThanhVien;
        public ObservableCollection<HangThanhVien> DSHangThanhVien
        {
            get { return _dsHangThanhVien; }
            set { _dsHangThanhVien = value; OnPropertyChanged(); }
        }

        private HangThanhVien _selectedHangThanhVien;
        public HangThanhVien SelectedHangThanhVien
        {
            get { return _selectedHangThanhVien; }
            set
            {
                _selectedHangThanhVien = value;
                OnPropertyChanged();

                // Tự điền thông tin lên TextBox khi click item
                if (value != null)
                {
                    TenHang = value.TenHang;
                    Ptgg = value.PhanTramGiamGia.ToString();


                    bool isUsed = DataProvider.Ins.DB.KhachHangs
                .Any(x => x.MaHang == value.MaHang);

                    QuyenNgungHoatDongHang = !isUsed;
                }
            }
        }

        // ===== TEXTBOX BINDING =====
        private string _tenHang;
        public string TenHang
        {
            get { return _tenHang; }
            set { _tenHang = value; OnPropertyChanged(); }
        }

        private string _ptgg;
        public string Ptgg
        {
            get { return _ptgg; }
            set { _ptgg = value; OnPropertyChanged(); }
        }

        // ===== COMBOBOX LỌC THEO HẠNG =====
        private ObservableCollection<HangThanhVien> _dsHangFilter;
        public ObservableCollection<HangThanhVien> DSHangFilter
        {
            get { return _dsHangFilter; }
            set { _dsHangFilter = value; OnPropertyChanged(); }
        }

        private HangThanhVien _hangDangChon;
        public HangThanhVien HangDangChon
        {
            get => _hangDangChon;
            set
            {
                _hangDangChon = value;
                OnPropertyChanged();
                DSHangThanhVienView.Refresh();
            }
        }

        // ===== TÌM KIẾM THEO TÊN HẠNG =====
        private string _tuKhoa;
        public string TuKhoa
        {
            get => _tuKhoa;
            set
            {
                _tuKhoa = value;
                OnPropertyChanged();
                DSHangThanhVienView.Refresh();
            }
        }
        private bool _quyenNgungHoatDongHang;

        public bool QuyenNgungHoatDongHang
        {
            get { return _quyenNgungHoatDongHang; }
            set { _quyenNgungHoatDongHang = value; OnPropertyChanged(); }
        }


        // ===== COLLECTION VIEW & FILTER =====
        public ICollectionView DSHangThanhVienView { get; set; }
        public ICommand ThemCommand { get; set; }
        public ICommand SuaCommand { get; set; }
        public ICommand NgungHoatDongHangCommand { get; set; }
        public ICommand HoatDongTroLaiCommand { get; set; }

        private bool LocHangThanhVien(object obj)
        {
            if (!(obj is HangThanhVien h)) return false;

            // Lọc theo tên hạng (tìm kiếm liên tục)
            bool hopLeTen = string.IsNullOrWhiteSpace(TuKhoa)
                || h.TenHang.ToLower().Contains(TuKhoa.Trim().ToLower());

            // Lọc theo ComboBox — MaHang = -1 là "Tất cả"
            bool hopLeHang = HangDangChon == null
                || HangDangChon.MaHang == -1
                || h.MaHang == HangDangChon.MaHang;

            return hopLeTen && hopLeHang;
        }

        // ===== CONSTRUCTOR =====
        public QL_HangThanhVienViewModel()
        {
            // Load danh sách chính
            DSHangThanhVien = new ObservableCollection<HangThanhVien>(
                DataProvider.Ins.DB.HangThanhViens.ToList());

            // Gắn CollectionView và filter
            DSHangThanhVienView = CollectionViewSource.GetDefaultView(DSHangThanhVien);
            DSHangThanhVienView.Filter = LocHangThanhVien;

            // Load ComboBox — dùng bản sao riêng để không ảnh hưởng ListView
            var cacHang = DataProvider.Ins.DB.HangThanhViens.ToList();
            DSHangFilter = new ObservableCollection<HangThanhVien>(cacHang);
            DSHangFilter.Insert(0, new HangThanhVien { MaHang = -1, TenHang = "Tất cả" });
            HangDangChon = DSHangFilter[0];

            ThemCommand = new RelayCommand<object>((p) => { return true; }, (p) =>
            {
                AddHangTV addHangTV = new AddHangTV(); 
                addHangTV.ShowDialog();
            });
            SuaCommand = new RelayCommand<object>((p) => { return SelectedHangThanhVien != null; }, (p) =>
            {
                SuaHangThanhVien suahang = new SuaHangThanhVien();

                var suaVM = suahang.DataContext as SuaHangViewModel;

                suaVM.LoadThongTin();

                suahang.ShowDialog();

                DSHangThanhVienView.Refresh(); 

            });
            HoatDongTroLaiCommand = new RelayCommand<object>((p) => {
                return SelectedHangThanhVien != null && SelectedHangThanhVien.IsEnable == false; }, (p) =>
            {
            MessageBoxResult messageBoxResult = MessageBox.Show("Thiết lập hoạt động trở lại cho hạng này ?"
               , "Xác Nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (messageBoxResult == MessageBoxResult.Yes)
                {

                    var ranking = DataProvider.Ins.DB.HangThanhViens
                    .Where(r => r.MaHang == SelectedHangThanhVien.MaHang)
                    .FirstOrDefault();

                    ranking.IsEnable = true;

                    LoginWindow ql = new LoginWindow();
                    var qlVM = ql.DataContext as LoginViewModel;

                    AddKhachHang a = new AddKhachHang();
                    var aVM = a.DataContext as AddKhachHangViewModel;

                    SuaKhachHang s = new SuaKhachHang();
                    var sVM = s.DataContext as SuaKhachHangViewModel;

                    SystemLog systemLog = new SystemLog
                    {
                        MaNhanVien = qlVM.UserId,
                        ThoiGian = DateTime.Now,
                        HanhDong = "SUA_HANG",
                        MoTa = $"Nhân viên mã {qlVM.UserId} đã cập nhật thông tin hạng vào lúc {DateTime.Now}"
                    };
                    DataProvider.Ins.DB.SystemLogs.Add(systemLog);
                    DataProvider.Ins.DB.SaveChanges();

                    DSHangThanhVienView.Refresh();

                    aVM.LoadThongTinHang();
                    sVM.LoadDanhSachHang();
                }

            });
            NgungHoatDongHangCommand = new RelayCommand<object>((p) => {
                return SelectedHangThanhVien != null
                && QuyenNgungHoatDongHang && SelectedHangThanhVien.IsEnable == true; }, (p) =>
            {
                MessageBoxResult messageBoxResult = MessageBox.Show("Thiết lập ngưng hoạt động cho hạng này ?"
                    , "Xác Nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    var ranking = DataProvider.Ins.DB.HangThanhViens
                    .Where(r => r.MaHang == SelectedHangThanhVien.MaHang)
                    .FirstOrDefault();

                    ranking.IsEnable = false;

                    LoginWindow ql = new LoginWindow();
                    var qlVM = ql.DataContext as LoginViewModel;

                    AddKhachHang a = new AddKhachHang();
                    var aVM = a.DataContext as AddKhachHangViewModel;

                    SuaKhachHang s = new SuaKhachHang();
                    var sVM = s.DataContext as SuaKhachHangViewModel;

                    SystemLog systemLog = new SystemLog
                    {
                        MaNhanVien = qlVM.UserId,
                        ThoiGian = DateTime.Now,
                        HanhDong = "SUA_HANG",
                        MoTa = $"Nhân viên mã {qlVM.UserId} đã cập nhật thông tin hạng vào lúc {DateTime.Now}"
                    };
                    DataProvider.Ins.DB.SystemLogs.Add(systemLog);
                    DataProvider.Ins.DB.SaveChanges();

                    DSHangThanhVienView.Refresh();

                    aVM.LoadThongTinHang();
                    sVM.LoadDanhSachHang();
                }

            }); 
        }
    }
}
