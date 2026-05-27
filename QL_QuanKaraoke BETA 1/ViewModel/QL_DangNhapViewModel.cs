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
    public class QL_DangNhapViewModel : BaseViewModel
    {
        private ObservableCollection<LoginLog> _dsLoginLog;
        public ObservableCollection<LoginLog> DSLoginLog
        {
            get { return _dsLoginLog; }
            set { _dsLoginLog = value; OnPropertyChanged(); }
        }

        private LoginLog _selectedLog;
        public LoginLog SelectedLog
        {
            get { return _selectedLog; }
            set
            {
                _selectedLog = value;
                OnPropertyChanged();

                if (value != null)
                {
                    TenNhanVien = value.NhanVien?.HoTen ?? $"Mã NV: {value.MaNhanVien}";
                    ThoiGianDangNhap = value.ThoiGianDangNhap;
                    ThoiGianDangXuat = value.ThoiGianDangXuat;
                }
            }
        }

        // ===== 3 TEXTBOX BINDING =====
        private string _tenNhanVien;
        public string TenNhanVien
        {
            get => _tenNhanVien;
            set { _tenNhanVien = value; OnPropertyChanged(); }
        }

        private DateTime? _thoiGianDangNhap;
        public DateTime? ThoiGianDangNhap
        {
            get => _thoiGianDangNhap;
            set { _thoiGianDangNhap = value; OnPropertyChanged(); }
        }

        private DateTime? _thoiGianDangXuat;
        public DateTime? ThoiGianDangXuat
        {
            get => _thoiGianDangXuat;
            set { _thoiGianDangXuat = value; OnPropertyChanged(); }
        }

        // ===== DATEPICKER LỌC THEO NGÀY =====
        private DateTime? _ngayLoc;
        public DateTime? NgayLoc
        {
            get => _ngayLoc;
            set
            {
                _ngayLoc = value;
                OnPropertyChanged();
                DsSystemLogView.Refresh();
            }
        }

        // ===== TEXTBOX TÌM KIẾM TÊN NHÂN VIÊN =====
        private string _tuKhoa;
        public string TuKhoa
        {
            get => _tuKhoa;
            set
            {
                _tuKhoa = value;
                OnPropertyChanged();
                DsSystemLogView.Refresh();
            }
        }

        // ===== COLLECTION VIEW & FILTER =====
        public ICollectionView DsSystemLogView { get; set; }

        private bool LocSystemLog(object obj)
        {
            if (!(obj is LoginLog log)) return false;

            // Lọc theo tên nhân viên
            bool hopLeTen = string.IsNullOrWhiteSpace(TuKhoa)
                || (log.NhanVien?.HoTen != null &&
                    log.NhanVien.HoTen.ToLower().Contains(TuKhoa.Trim().ToLower()));

            // Lọc theo ngày — chỉ so sánh phần Date, bỏ qua giờ
            bool hopLeNgay = NgayLoc == null
                || (log.ThoiGianDangNhap.HasValue &&
                    log.ThoiGianDangNhap.Value.Date == NgayLoc.Value.Date);

            return hopLeTen && hopLeNgay;
        }

        // ===== COMMANDS =====
        public ICommand XoaLogCommand { get; set; }

        // ===== LOAD =====
        private void LoadSystemLog()
        {
            var data = DataProvider.Ins.DB.LoginLogs
                                   .Include("NhanVien")
                                   .OrderByDescending(log => log.ThoiGianDangNhap) // Mới nhất lên trên
                                   .ToList();

            DSLoginLog = new ObservableCollection<LoginLog>(data);
        }

        // ===== CONSTRUCTOR =====
        public QL_DangNhapViewModel()
        {
            LoadSystemLog();

            DsSystemLogView = CollectionViewSource.GetDefaultView(DSLoginLog);
            DsSystemLogView.Filter = LocSystemLog;

            // ── Nút Xóa Log ──
            XoaLogCommand = new RelayCommand<object>(
                (p) => SelectedLog != null,
                (p) =>
                {
                    MessageBoxResult confirm = MessageBox.Show(
                        "Bạn có chắc muốn xóa log này không?",
                        "Xác nhận",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (confirm != MessageBoxResult.Yes) return;

                    try
                    {
                        var log = DataProvider.Ins.DB.LoginLogs
                            .FirstOrDefault(x => x.MaLog == SelectedLog.MaLog);

                        if (log == null) return;

                        DataProvider.Ins.DB.LoginLogs.Remove(log);
                        DataProvider.Ins.DB.SaveChanges();

                        DSLoginLog.Remove(SelectedLog);

                        // Reset TextBox sau khi xóa
                        TenNhanVien = string.Empty;
                        ThoiGianDangNhap = null;
                        ThoiGianDangXuat = null;

                        MessageBox.Show("Xóa log thành công!", "Thông báo",
                                        MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                });

        }
    }
}
