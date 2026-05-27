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
    public class QL_HeThongViewModel : BaseViewModel
    {
        // ===== DANH SÁCH LOG =====
        private ObservableCollection<SystemLog> _dsSystemLog;
        public ObservableCollection<SystemLog> DsSystemLog
        {
            get { return _dsSystemLog; }
            set { _dsSystemLog = value; OnPropertyChanged(); }
        }

        private SystemLog _selectedLog;
        public SystemLog SelectedLog
        {
            get { return _selectedLog; }
            set
            {
                _selectedLog = value;
                OnPropertyChanged();

                // Tự điền thông tin lên 3 TextBox khi click item
                if (value != null)
                {
                    TenNhanVien = value.NhanVien?.HoTen ?? $"Mã NV: {value.MaNhanVien}";
                    HanhDong = value.HanhDong;
                    ThoiGian = value.ThoiGian?.ToString("dd/MM/yyyy HH:mm:ss");
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

        private string _hanhDong;
        public string HanhDong
        {
            get => _hanhDong;
            set { _hanhDong = value; OnPropertyChanged(); }
        }

        private string _thoiGian;
        public string ThoiGian
        {
            get => _thoiGian;
            set { _thoiGian = value; OnPropertyChanged(); }
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
            if (!(obj is SystemLog log)) return false;

            // Lọc theo tên nhân viên
            bool hopLeTen = string.IsNullOrWhiteSpace(TuKhoa)
                || (log.NhanVien?.HoTen != null &&
                    log.NhanVien.HoTen.ToLower().Contains(TuKhoa.Trim().ToLower()));

            // Lọc theo ngày — chỉ so sánh phần Date, bỏ qua giờ
            bool hopLeNgay = NgayLoc == null
                || (log.ThoiGian.HasValue &&
                    log.ThoiGian.Value.Date == NgayLoc.Value.Date);

            return hopLeTen && hopLeNgay;
        }

        // ===== COMMANDS =====
        public ICommand XoaLogCommand { get; set; }

        // ===== LOAD =====
        private void LoadSystemLog()
        {
            var data = DataProvider.Ins.DB.SystemLogs
                                   .Include("NhanVien")
                                   .OrderByDescending(log => log.ThoiGian) // Mới nhất lên trên
                                   .ToList();

            DsSystemLog = new ObservableCollection<SystemLog>(data);
        }

        // ===== CONSTRUCTOR =====
        public QL_HeThongViewModel()
        {
            LoadSystemLog();

            DsSystemLogView = CollectionViewSource.GetDefaultView(DsSystemLog);
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
                        var log = DataProvider.Ins.DB.SystemLogs
                            .FirstOrDefault(x => x.MaLog == SelectedLog.MaLog);

                        if (log == null) return;

                        DataProvider.Ins.DB.SystemLogs.Remove(log);
                        DataProvider.Ins.DB.SaveChanges();

                        DsSystemLog.Remove(SelectedLog);

                        // Reset TextBox sau khi xóa
                        TenNhanVien = string.Empty;
                        HanhDong = string.Empty;
                        ThoiGian = string.Empty;

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
