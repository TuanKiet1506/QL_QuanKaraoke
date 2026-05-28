using QL_QuanKaraoke_BETA_1.Data;
using QL_QuanKaraoke_BETA_1.Model;
using QL_QuanKaraoke_BETA_1.View;
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
    public class QL_DichVuViewModel : BaseViewModel
    {
        // ===== DANH SÁCH DỊCH VỤ =====
        private ObservableCollection<DichVu> _dsDichVu;
        public ObservableCollection<DichVu> DSDichVu
        {
            get { return _dsDichVu; }
            set { _dsDichVu = value; OnPropertyChanged(); }
        }

        private DichVu _selectedDichVu;
        public DichVu SelectedDichVu
        {
            get { return _selectedDichVu; }
            set
            {
                _selectedDichVu = value;
                OnPropertyChanged();

                // Tự điền thông tin lên 3 TextBox khi click item
                if (value != null)
                {
                    TenDichVu = value.TenDichVu;
                    DonGia = value.DonGia?.ToString("N0");
                    TonKho = value.TonKho?.ToString();
                }
            }
        }

        // ===== 3 TEXTBOX BINDING =====
        private string _tenDichVu;
        public string TenDichVu
        {
            get => _tenDichVu;
            set { _tenDichVu = value; OnPropertyChanged(); }
        }

        private string _donGia;
        public string DonGia
        {
            get => _donGia;
            set { _donGia = value; OnPropertyChanged(); }
        }

        private string _tonKho;
        public string TonKho
        {
            get => _tonKho;
            set { _tonKho = value; OnPropertyChanged(); }
        }

        // ===== COMBOBOX LỌC THEO ĐƠN VỊ TÍNH =====
        // DichVu không có bảng danh mục riêng
        // → dùng DonViTinh làm tiêu chí lọc ComboBox
        private ObservableCollection<string> _dsDonViTinh;
        public ObservableCollection<string> DSDonViTinh
        {
            get { return _dsDonViTinh; }
            set { _dsDonViTinh = value; OnPropertyChanged(); }
        }

        private string _donViTinhDangChon;
        public string DonViTinhDangChon
        {
            get => _donViTinhDangChon;
            set
            {
                _donViTinhDangChon = value;
                OnPropertyChanged();
                DSDichVuView?.Refresh(); // null-safe
            }
        }

        // ===== TÌM KIẾM THEO TÊN DỊCH VỤ =====
        private string _tuKhoa;
        public string TuKhoa
        {
            get => _tuKhoa;
            set
            {
                _tuKhoa = value;
                OnPropertyChanged();
                DSDichVuView.Refresh();
            }
        }

        // ===== COLLECTION VIEW & FILTER =====
        public ICollectionView DSDichVuView { get; set; }
        public ICommand ThemCommand { get; set; }
        public ICommand SuaDichVuCommand { get; set; }
        public ICommand NgungHoatDongCommand { get; set; }

        private bool LocDichVu(object obj)
        {
            if (!(obj is DichVu dv)) return false;

            // Chỉ hiển thị dịch vụ còn hoạt động (IsEnable = true hoặc null)
            bool conHoatDong = dv.IsEnable == true || dv.IsEnable == null;

            // Lọc theo tên dịch vụ
            bool hopLeTen = string.IsNullOrWhiteSpace(TuKhoa)
                || (dv.TenDichVu != null &&
                    dv.TenDichVu.ToLower().Contains(TuKhoa.Trim().ToLower()));

            // Lọc theo đơn vị tính — "Tất cả" thì không lọc
            bool hopLeDonVi = string.IsNullOrEmpty(DonViTinhDangChon)
                || DonViTinhDangChon == "Tất cả"
                || dv.DonViTinh == DonViTinhDangChon;

            return conHoatDong && hopLeTen && hopLeDonVi;
        }

        // ===== LOAD =====
        private void LoadDichVu()
        {
            var data = DataProvider.Ins.DB.DichVus.ToList();
            DSDichVu = new ObservableCollection<DichVu>(data);
        }

        private void LoadDonViTinh()
        {
            // Lấy các đơn vị tính distinct từ DB làm item ComboBox
            var cacDonVi = DataProvider.Ins.DB.DichVus
                .Where(dv => dv.DonViTinh != null)
                .Select(dv => dv.DonViTinh)
                .Distinct()
                .ToList();

            DSDonViTinh = new ObservableCollection<string>(cacDonVi);
            DSDonViTinh.Insert(0, "Tất cả");
            DonViTinhDangChon = DSDonViTinh[0];
        }

        // ===== CONSTRUCTOR =====
        public QL_DichVuViewModel()
        {
            LoadDichVu();

            DSDichVuView = CollectionViewSource.GetDefaultView(DSDichVu);
            DSDichVuView.Filter = LocDichVu;

            LoadDonViTinh();

            ThemCommand = new RelayCommand<object>((p) => { return true; }, (p) => {
                AddDichVu addDichVu = new AddDichVu();
                addDichVu.ShowDialog();
            });
            SuaDichVuCommand = new RelayCommand<object>((p) => { return SelectedDichVu != null; }, (p) => {
                SuaDichVu suaDV = new SuaDichVu();

                var suaDVVM = suaDV.DataContext as SuaDichVuViewModel;

                suaDVVM.LoadThongTin();

                suaDV.ShowDialog();

                DSDichVuView.Refresh();
            });
        }
    }
}
