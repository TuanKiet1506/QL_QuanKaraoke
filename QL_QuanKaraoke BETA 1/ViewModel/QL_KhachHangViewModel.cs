using QL_QuanKaraoke_BETA_1.Data;
using QL_QuanKaraoke_BETA_1.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

namespace QL_QuanKaraoke_BETA_1.ViewModel
{
    public class QL_KhachHangViewModel : BaseViewModel
    {
        // ===== DANH SÁCH KHÁCH HÀNG =====
        private ObservableCollection<KhachHang> _dsKhachHang;
        public ObservableCollection<KhachHang> DanhSachKhachHang
        {
            get { return _dsKhachHang; }
            set { _dsKhachHang = value; OnPropertyChanged(); }
        }

        private KhachHang _selectedKhachHang;
        public KhachHang SelectedKhachHang
        {
            get { return _selectedKhachHang; }
            set { _selectedKhachHang = value; OnPropertyChanged(); }
        }

        private ObservableCollection<HangThanhVien> _dsHangTV;
        public ObservableCollection<HangThanhVien> DSHangTV
        {
            get { return _dsHangTV; }
            set { _dsHangTV = value; OnPropertyChanged(); }
        }

        // null = "Tất cả" (không lọc theo hạng)
        private HangThanhVien _hangDangChon;
        public HangThanhVien HangDangChon
        {
            get => _hangDangChon;
            set
            {
                _hangDangChon = value;
                OnPropertyChanged();
                DanhSachKhachHangView.Refresh();
            }
        }

        // ===== TÌM KIẾM THEO TÊN =====
        private string _tuKhoa;
        public string TuKhoa
        {
            get => _tuKhoa;
            set
            {
                _tuKhoa = value;
                OnPropertyChanged();
                DanhSachKhachHangView.Refresh();
            }
        }

        // ===== COLLECTION VIEW & FILTER =====
        public ICollectionView DanhSachKhachHangView { get; set; }
        public ICommand ThemCommand { get; set; }
        public ICommand SuaCommand { get; set; }

        private bool LocKhachHang(object obj)
        {
            if (!(obj is KhachHang kh)) return false;

            bool hopLeTen = string.IsNullOrWhiteSpace(TuKhoa)
                || kh.HoTen.ToLower().Contains(TuKhoa.Trim().ToLower());

            bool hopLeHang = HangDangChon == null
                || HangDangChon.MaHang == -1
                || kh.MaHang == HangDangChon.MaHang;

            return hopLeTen && hopLeHang;
        }

        // ===== CONSTRUCTOR =====
        public QL_KhachHangViewModel()
        {
            DanhSachKhachHang = new ObservableCollection<KhachHang>(
                DataProvider.Ins.DB.KhachHangs);

            DanhSachKhachHangView = CollectionViewSource.GetDefaultView(DanhSachKhachHang);
            DanhSachKhachHangView.Filter = LocKhachHang;

            var cacHang = DataProvider.Ins.DB.HangThanhViens.ToList();
            DSHangTV = new ObservableCollection<HangThanhVien>(cacHang);

            DSHangTV.Insert(0, new HangThanhVien { MaHang = -1, TenHang = "Tất cả" });

            HangDangChon = DSHangTV[0];

            ThemCommand = new RelayCommand<object>((p) => { return true; }, (p) =>
            {
                AddKhachHang add = new AddKhachHang();
                var aVM = add.DataContext as AddKhachHangViewModel;

                aVM.LoadThongTinHang(); 

                add.ShowDialog(); 
            });
            SuaCommand = new RelayCommand<object>((p) => { return SelectedKhachHang != null; }, (p) =>
            {
                SuaKhachHang suakh = new SuaKhachHang();

                var suaVM = suakh.DataContext as SuaKhachHangViewModel;

                suaVM.LoadThongTin();

                suakh.ShowDialog();

                DanhSachKhachHangView.Refresh();
            });
        }
    }
}