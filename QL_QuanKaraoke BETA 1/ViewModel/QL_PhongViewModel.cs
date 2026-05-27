using QL_QuanKaraoke_BETA_1.Data;
using QL_QuanKaraoke_BETA_1.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace QL_QuanKaraoke_BETA_1.ViewModel
{
    public class QL_PhongViewModel : BaseViewModel
    {
        // ===== DANH SÁCH PHÒNG =====
        private ObservableCollection<Phong> _danhSachPhong;
        public ObservableCollection<Phong> DanhSachPhong
        {
            get { return _danhSachPhong; }
            set { _danhSachPhong = value; OnPropertyChanged(); }
        }

        private Phong _selectedPhong;
        public Phong SelectedPhong
        {
            get { return _selectedPhong; }
            set
            {
                _selectedPhong = value;
                OnPropertyChanged();

                // Tự điền thông tin lên TextBox khi click vào item
                if (value != null)
                {
                    TenPhong = value.TenPhong;
                    SucChua = value.SucChua.ToString();
                    LoaiPhong = value.LoaiPhong?.TenLoai;
                }
            }
        }

        // ===== TEXTBOX BINDING =====
        private string _tenPhong;
        public string TenPhong
        {
            get => _tenPhong;
            set { _tenPhong = value; OnPropertyChanged(); }
        }

        private string _sucChua;
        public string SucChua
        {
            get => _sucChua;
            set { _sucChua = value; OnPropertyChanged(); }
        }

        private string _loaiPhong;
        public string LoaiPhong
        {
            get => _loaiPhong;
            set { _loaiPhong = value; OnPropertyChanged(); }
        }

        // ===== COMBOBOX LOẠI PHÒNG =====
        private ObservableCollection<LoaiPhong> _dsLoaiPhong;
        public ObservableCollection<LoaiPhong> DSLoaiPhong
        {
            get { return _dsLoaiPhong; }
            set { _dsLoaiPhong = value; OnPropertyChanged(); }
        }

        private LoaiPhong _loaiPhongDangChon;
        public LoaiPhong LoaiPhongDangChon
        {
            get => _loaiPhongDangChon;
            set
            {
                _loaiPhongDangChon = value;
                OnPropertyChanged();
                DanhSachPhongView?.Refresh();
            }
        }

        // ===== TÌM KIẾM THEO TÊN PHÒNG =====
        private string _tuKhoa;
        public string TuKhoa
        {
            get => _tuKhoa;
            set
            {
                _tuKhoa = value;
                OnPropertyChanged();
                DanhSachPhongView?.Refresh();
            }
        }

        // ===== COLLECTION VIEW & FILTER =====
        public ICollectionView DanhSachPhongView { get; set; }
        public ICommand ThemCommand { get; set; }
        public ICommand SuaCommand { get; set; }
        public ICommand NgungHoatDongPhongCommand { get; set; }

        private bool LocPhong(object obj)
        {
            if (!(obj is Phong p)) return false;

            // Lọc theo tên phòng
            bool hopLeTen = string.IsNullOrWhiteSpace(TuKhoa)
                || p.TenPhong.ToLower().Contains(TuKhoa.Trim().ToLower());

            // Lọc theo loại phòng — null hoặc MaLoaiPhong = -1 là "Tất cả"
            bool hopLeLoai = LoaiPhongDangChon == null
                || LoaiPhongDangChon.MaLoaiPhong == -1
                || p.MaLoaiPhong == LoaiPhongDangChon.MaLoaiPhong;

            return hopLeTen && hopLeLoai;
        }

        // ===== LOAD =====
        private void LoadPhong()
        {
            var data = DataProvider.Ins.DB.Phongs
                                   .Include("LoaiPhong")
                                   .ToList();

            // Chỉ lấy phòng còn hoạt động
            var phongHoatDong = data
                .Where(x => x.IsEnable != false)
                .ToList();

            DanhSachPhong = new ObservableCollection<Phong>(phongHoatDong);

            // Tạo lại CollectionView
            DanhSachPhongView =
                CollectionViewSource.GetDefaultView(DanhSachPhong);

            DanhSachPhongView.Filter = LocPhong;
        }

        private void LoadLoaiPhong()
        {
            var data = DataProvider.Ins.DB.LoaiPhongs.ToList();
            DSLoaiPhong = new ObservableCollection<LoaiPhong>(data);

            // Thêm "Tất cả" vào đầu ComboBox
            DSLoaiPhong.Insert(0, new LoaiPhong { MaLoaiPhong = -1, TenLoai = "Tất cả" });
            LoaiPhongDangChon = DSLoaiPhong[0];
        }

        // ===== CONSTRUCTOR =====
        public QL_PhongViewModel()
        {
            LoadPhong();
            LoadLoaiPhong();

            LoginWindow ql = new LoginWindow();
            var qlVM = ql.DataContext as LoginViewModel;

            ThemCommand = new RelayCommand<object>((p) => { return true; }, (p) => {
                AddPhong addPhong = new AddPhong();
                addPhong.ShowDialog(); 
            
            });

            SuaCommand = new RelayCommand<object>((p) => { return SelectedPhong != null; }, (p) => {
                SuaPhong suap = new SuaPhong();

                var suaVM = suap.DataContext as SuaPhongViewModel;

                suaVM.LoadThongTin();

                suap.ShowDialog();

                DanhSachPhongView.Refresh();

            });
            NgungHoatDongPhongCommand = new RelayCommand<object>((p) => { return SelectedPhong != null
                && (SelectedPhong.TrangThai == "Trống" || SelectedPhong.TrangThai == "Bảo Trì")
                && qlVM.maRole == 3; }, (p) => {

            

                MessageBoxResult messageBoxResult = MessageBox.Show("Xác ngưng hoạt động phòng vĩnh viễn ?"
              , "Xác Nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    var hd = DataProvider.Ins.DB.Phongs
                    .Where(h => h.MaPhong == SelectedPhong.MaPhong)
                    .FirstOrDefault();

                    hd.IsEnable = false;
                    DataProvider.Ins.DB.SaveChanges();


                    SystemLog systemLog = new SystemLog
                    {
                        MaNhanVien = qlVM.UserId,
                        ThoiGian = DateTime.Now,
                        HanhDong = "XOA_PHONG",
                        MoTa = $"Nhân viên mã {qlVM.UserId} đã xóa phòng vào lúc {DateTime.Now}"
                    };
                    DataProvider.Ins.DB.SystemLogs.Add(systemLog);
                    DataProvider.Ins.DB.SaveChanges();

                    DanhSachPhong.Remove(hd);
                    LoadPhong();
                    DanhSachPhongView.Refresh();
                }
            });
        }
    }
}
