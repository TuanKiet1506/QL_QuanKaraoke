using Microsoft.Win32;
using QL_QuanKaraoke_BETA_1.Data;
using QL_QuanKaraoke_BETA_1.Model;
using QL_QuanKaraoke_BETA_1.View;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace QL_QuanKaraoke_BETA_1.ViewModel
{
    public class SuaDichVuViewModel : BaseViewModel
    {
        // ===== THÔNG TIN DỊCH VỤ =====
        private DichVu _dichVu;
        public DichVu DichVu
        {
            get => _dichVu;
            set { _dichVu = value; OnPropertyChanged(); }
        }

        private string _tenDichVu;
        public string TenDichVu
        {
            get => _tenDichVu;
            set { _tenDichVu = value; OnPropertyChanged(); }
        }

        private string _donViTinh;
        public string DonViTinh
        {
            get => _donViTinh;
            set { _donViTinh = value; OnPropertyChanged(); }
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

        private string _canhBaoTonKho;
        public string CanhBaoTonKho
        {
            get => _canhBaoTonKho;
            set { _canhBaoTonKho = value; OnPropertyChanged(); }
        }

        // ===== HÌNH ẢNH =====
        private string _hinhAnhRelative;
        public string HinhAnhRelative
        {
            get => _hinhAnhRelative;
            set { _hinhAnhRelative = value; OnPropertyChanged(); }
        }

        private BitmapImage _hinhAnhPreview;
        public BitmapImage HinhAnhPreview
        {
            get => _hinhAnhPreview;
            set { _hinhAnhPreview = value; OnPropertyChanged(); }
        }

        // ===== COMMANDS =====
        public ICommand SuaHinhAnhCommand { get; set; }
        public ICommand XacNhanCommand { get; set; }
        public ICommand ExitCommand { get; set; }

        // ===== VALIDATION =====
        public bool KiemTraDuLieu()
        {
            if (string.IsNullOrWhiteSpace(TenDichVu))
            {
                MessageBox.Show("Tên dịch vụ không được để trống!",
                                "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(DonViTinh))
            {
                MessageBox.Show("Đơn vị tính không được để trống!",
                                "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!decimal.TryParse(DonGia, out decimal donGiaVal) || donGiaVal < 0)
            {
                MessageBox.Show("Đơn giá phải là số và không được âm!",
                                "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!int.TryParse(TonKho, out int tonKhoVal) || tonKhoVal < 0)
            {
                MessageBox.Show("Tồn kho phải là số nguyên và không được âm!",
                                "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!int.TryParse(CanhBaoTonKho, out int canhBaoVal) || canhBaoVal < 0)
            {
                MessageBox.Show("Cảnh báo tồn kho phải là số nguyên và không được âm!",
                                "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(HinhAnhRelative))
            {
                MessageBox.Show("Vui lòng chọn hình ảnh cho dịch vụ!",
                                "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Kiểm tra tên trùng — ngoại trừ chính nó
            bool trungTen = DataProvider.Ins.DB.DichVus
                .Any(dv => dv.TenDichVu.ToLower() == TenDichVu.Trim().ToLower()
                        && dv.MaDichVu != DichVu.MaDichVu);
            if (trungTen)
            {
                MessageBox.Show($"Tên dịch vụ '{TenDichVu}' đã tồn tại!",
                                "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        // ===== LOAD THÔNG TIN TỪ SELECTED ITEM =====
        public void LoadThongTin()
        {
            QL_DichVuUC ql = new QL_DichVuUC();
            var qlVM = ql.DataContext as QL_DichVuViewModel;

            if (qlVM == null || qlVM.SelectedDichVu == null) return;

            DichVu = qlVM.SelectedDichVu;

            // Điền thông tin lên các TextBox
            TenDichVu = DichVu.TenDichVu;
            DonViTinh = DichVu.DonViTinh;
            DonGia = DichVu.DonGia?.ToString();
            TonKho = DichVu.TonKho?.ToString();
            CanhBaoTonKho = DichVu.CanhBaoTonKho?.ToString();
            HinhAnhRelative = DichVu.HinhAnh;

            // Load preview ảnh hiện tại
            if (!string.IsNullOrEmpty(DichVu.HinhAnh))
            {
                try
                {
                    string fullPath = Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        DichVu.HinhAnh);

                    if (File.Exists(fullPath))
                        HinhAnhPreview = new BitmapImage(new Uri(fullPath, UriKind.Absolute));
                }
                catch { HinhAnhPreview = null; }
            }
        }

        // ===== CONSTRUCTOR =====
        public SuaDichVuViewModel()
        {
            // ── Nút Sửa Hình Ảnh ──
            SuaHinhAnhCommand = new RelayCommand<object>((p) => true, (p) =>
            {
                OpenFileDialog dialog = new OpenFileDialog
                {
                    Title = "Chọn hình ảnh mới",
                    Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif"
                };

                if (dialog.ShowDialog() != true) return;

                string sourceFile = dialog.FileName;
                string fileName = Path.GetFileName(sourceFile);

                string imageFolder = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory, "image");

                if (!Directory.Exists(imageFolder))
                    Directory.CreateDirectory(imageFolder);

                string destFile = Path.Combine(imageFolder, fileName);

                // Tránh trùng tên file với file khác (không phải file hiện tại)
                string currentFile = string.IsNullOrEmpty(HinhAnhRelative)
                    ? ""
                    : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, HinhAnhRelative);

                if (File.Exists(destFile) &&
                    !string.Equals(destFile, currentFile, StringComparison.OrdinalIgnoreCase))
                {
                    string nameNoExt = Path.GetFileNameWithoutExtension(fileName);
                    string ext = Path.GetExtension(fileName);
                    fileName = $"{nameNoExt}_{DateTime.Now:yyyyMMddHHmmss}{ext}";
                    destFile = Path.Combine(imageFolder, fileName);
                }

                // Copy ảnh mới vào folder image
                File.Copy(sourceFile, destFile, overwrite: true);

                // Cập nhật đường dẫn tương đối
                HinhAnhRelative = $"image/{fileName}";

                // Cập nhật preview
                HinhAnhPreview = new BitmapImage(new Uri(destFile, UriKind.Absolute));
            });

            // ── Nút Xác Nhận ──
            XacNhanCommand = new RelayCommand<Window>((p) => true, (p) =>
            {
                if (!KiemTraDuLieu()) return;

                try
                {
                    // Lấy bản ghi từ DB theo mã dịch vụ
                    int maDV = DichVu.MaDichVu;
                    var dv = DataProvider.Ins.DB.DichVus
                        .FirstOrDefault(x => x.MaDichVu == maDV);

                    if (dv == null)
                    {
                        MessageBox.Show("Không tìm thấy dịch vụ trong hệ thống!");
                        return;
                    }

                    MessageBoxResult confirm = MessageBox.Show(
                        "Xác nhận lưu thông tin dịch vụ?",
                        "Xác nhận",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (confirm != MessageBoxResult.Yes) return;

                    // Cập nhật thông tin
                    dv.TenDichVu = TenDichVu.Trim();
                    dv.DonViTinh = DonViTinh.Trim();
                    dv.DonGia = decimal.Parse(DonGia);
                    dv.TonKho = int.Parse(TonKho);
                    dv.CanhBaoTonKho = int.Parse(CanhBaoTonKho);
                    dv.HinhAnh = HinhAnhRelative;

                    DataProvider.Ins.DB.SaveChanges();

                    // Ghi log
                    LoginWindow login = new LoginWindow();
                    var loginVM = login.DataContext as LoginViewModel;

                    SystemLog log = new SystemLog
                    {
                        MaNhanVien = loginVM.UserId,
                        ThoiGian = DateTime.Now,
                        HanhDong = "SUA_DICH_VU",
                        MoTa = $"Nhân viên mã {loginVM.UserId} đã cập nhật " +
                                     $"dịch vụ '{dv.TenDichVu}' vào lúc {DateTime.Now}"
                    };
                    DataProvider.Ins.DB.SystemLogs.Add(log);
                    DataProvider.Ins.DB.SaveChanges();

                    MessageBox.Show("Cập nhật dịch vụ thành công!", "Thông báo",
                                    MessageBoxButton.OK, MessageBoxImage.Information);

                    p?.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            });

            ExitCommand = new RelayCommand<Window>(
                (p) => true,
                (p) => p?.Close());
        }
    }
}
