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
    public class QL_HoaDonViewModel : BaseViewModel
    {
        private ObservableCollection<HoaDon> _listHoaDon;
        public ObservableCollection<HoaDon> ListHoaDon
        {
            get { return _listHoaDon; }
            set { _listHoaDon = value; OnPropertyChanged(); }
        }

        private HoaDon _selectedHoaDon;
        public HoaDon SelectedHoaDon
        {
            get { return _selectedHoaDon; }
            set { _selectedHoaDon = value; OnPropertyChanged(); }
        }

        private ObservableCollection<string> _dsTrangThai;
        public ObservableCollection<string> DSTrangThai
        {
            get { return _dsTrangThai; }
            set { _dsTrangThai = value; OnPropertyChanged(); }
        }

        private string _trangThaiDangChon;
        public string TrangThaiDangChon
        {
            get => _trangThaiDangChon;
            set
            {
                _trangThaiDangChon = value;
                OnPropertyChanged();
                DanhSachHoaDonView.Refresh();
            }
        }

        private string _tuKhoa;
        public string TuKhoa
        {
            get => _tuKhoa;
            set
            {
                _tuKhoa = value;
                OnPropertyChanged();
                DanhSachHoaDonView.Refresh();
            }
        }

        // ===== COLLECTION VIEW & FILTER =====
        public ICollectionView DanhSachHoaDonView { get; set; }
        public ICommand CapNhatCommand { get; set; }
        public ICommand HuyCommand { get; set; }

        private bool LocHoaDon(object obj)
        {
            if (!(obj is HoaDon hd)) return false;

            // Lọc theo tên khách hàng — dùng navigation property KhachHang
            bool hopLeTen = string.IsNullOrWhiteSpace(TuKhoa)
                || (hd.KhachHang != null &&
                    hd.KhachHang.HoTen.ToLower().Contains(TuKhoa.Trim().ToLower()));

            // Lọc theo trạng thái — "Tất cả" hoặc null thì không lọc
            bool hopLeTrangThai = string.IsNullOrEmpty(TrangThaiDangChon)
                || TrangThaiDangChon == "Tất cả"
                || hd.TrangThai == TrangThaiDangChon;

            return hopLeTen && hopLeTrangThai;
        }

        // ===== LOAD =====
        public void LoadHoaDon()
        {
            var data = DataProvider.Ins.DB.HoaDons
                                   .Include("KhachHang")
                                   .Include("Phong")
                                   .Include("NhanVien")
                                   .ToList();

            var hd = data
                .Where(x => x.IsDeleted != true)
                .ToList();

            ListHoaDon = new ObservableCollection<HoaDon>(hd);


            DanhSachHoaDonView =
                CollectionViewSource.GetDefaultView(ListHoaDon);

            DanhSachHoaDonView.Filter = LocHoaDon;
        }

        // ===== CONSTRUCTOR =====
        public QL_HoaDonViewModel()
        {
            LoadHoaDon();

            // Khởi tạo danh sách trạng thái cho ComboBox
            DSTrangThai = new ObservableCollection<string>
        {
            "Tất cả",
            "DANG_MO",
            "DA_THANH_TOAN",
            "CHUA_THANH_TOAN"
        };
            LoginWindow ql = new LoginWindow();
            var qlVM = ql.DataContext as LoginViewModel;

            // Mặc định chọn "Tất cả"
            TrangThaiDangChon = DSTrangThai[0];

            CapNhatCommand = new RelayCommand<object>((p) => { return SelectedHoaDon != null; }, (p) =>
            {
                LoginWindow mp = new LoginWindow();
                var mpVM = mp.DataContext as LoginViewModel;

                if (SelectedHoaDon.TrangThai == "CHUA_THANH_TOAN")
                {
                    MessageBoxResult messageBoxResult = MessageBox.Show("Xác nhận hóa đơn đã thanh toán xong ?"
                        , "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (messageBoxResult == MessageBoxResult.Yes)
                    {
                        var hd = DataProvider.Ins.DB.HoaDons
                        .Where(h => h.MaHoaDon == SelectedHoaDon.MaHoaDon)
                        .FirstOrDefault();

                        hd.TrangThai = "DA_THANH_TOAN";
                        DataProvider.Ins.DB.SaveChanges();

                        SystemLog system = new SystemLog
                        {
                            MaNhanVien = mpVM.UserId,
                            HanhDong = "CAP_NHAT_HOA_DON",
                            ThoiGian = DateTime.Now,
                            MoTa = $"Nhân viên mã {mpVM.UserId} đã cập nhật hóa đơn mã {hd.MaHoaDon} thành trạng thái DA_THANH_TOAN vào lúc {DateTime.Now}"
                        };
                        DataProvider.Ins.DB.SystemLogs.Add(system);
                        DataProvider.Ins.DB.SaveChanges();

                        DanhSachHoaDonView.Refresh();
                    }
                    else
                    {
                        return; 
                    }
                } else if(SelectedHoaDon.TrangThai == "DA_THANH_TOAN")
                {
                    MessageBoxResult messageBoxResult = MessageBox.Show("Xác nhận hóa đơn chưa được thanh toán ?"
                        , "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (messageBoxResult == MessageBoxResult.Yes)
                    {
                        var hd = DataProvider.Ins.DB.HoaDons
                        .Where(h => h.MaHoaDon == SelectedHoaDon.MaHoaDon)
                        .FirstOrDefault();

                        hd.TrangThai = "CHUA_THANH_TOAN";
                        DataProvider.Ins.DB.SaveChanges();

                        SystemLog system = new SystemLog
                        {
                            MaNhanVien = mpVM.UserId,
                            HanhDong = "CAP_NHAT_HOA_DON",
                            ThoiGian = DateTime.Now,
                            MoTa = $"Nhân viên mã {mpVM.UserId} đã cập nhật hóa đơn mã {hd.MaHoaDon} thành trạng thái CHUA_THANH_TOAN vào lúc {DateTime.Now}"
                        };
                        DataProvider.Ins.DB.SystemLogs.Add(system);
                        DataProvider.Ins.DB.SaveChanges();

                        DanhSachHoaDonView.Refresh(); 
                    }
                    else
                    {
                        return;
                    }
                }
            }
            );

            HuyCommand = new RelayCommand<object>((p) => {
                return SelectedHoaDon != null
                && (qlVM.maRole == 3 || qlVM.maRole == 1) && SelectedHoaDon.TrangThai == "DA_THANH_TOAN"; }, (p) => {

                    MessageBoxResult messageBoxResult = MessageBox.Show("Xác nhận hủy hóa đơn này ?"
                , "Xác Nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (messageBoxResult == MessageBoxResult.Yes)
                    {
                        var hd = DataProvider.Ins.DB.HoaDons
                        .Where(h => h.MaHoaDon == SelectedHoaDon.MaHoaDon)
                        .FirstOrDefault();

                        hd.IsDeleted = true;
                        DataProvider.Ins.DB.SaveChanges();


                        SystemLog systemLog = new SystemLog
                        {
                            MaNhanVien = qlVM.UserId,
                            ThoiGian = DateTime.Now,
                            HanhDong = "XOA_HOA_DON",
                            MoTa = $"Nhân viên mã {qlVM.UserId} đã xóa hóa đơn vào lúc {DateTime.Now}"
                        };
                        DataProvider.Ins.DB.SystemLogs.Add(systemLog);
                        DataProvider.Ins.DB.SaveChanges();

                        ListHoaDon.Remove(hd); 
                        LoadHoaDon();
                        DanhSachHoaDonView.Refresh(); 
                    }

            }); 
        }
    }
}
