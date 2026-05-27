using Microsoft.Win32;
using QL_QuanKaraoke_BETA_1.Data;
using QL_QuanKaraoke_BETA_1.Model;
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
    public class AddDichVuViewModel : BaseViewModel
    {
        // ===== THÔNG TIN DỊCH VỤ =====
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
        // Đường dẫn tương đối lưu vào DB: "image/tenfile.jpg"
        private string _hinhAnhRelative;
        public string HinhAnhRelative
        {
            get => _hinhAnhRelative;
            set { _hinhAnhRelative = value; OnPropertyChanged(); }
        }

        // BitmapImage để preview ảnh trên giao diện
        private BitmapImage _hinhAnhPreview;
        public BitmapImage HinhAnhPreview
        {
            get => _hinhAnhPreview;
            set { _hinhAnhPreview = value; OnPropertyChanged(); }
        }

        // ===== COMMANDS =====
        public ICommand ThemHinhAnhCommand { get; set; }
        public ICommand XacNhanCommand { get; set; }
        public ICommand ExitCommand { get; set; }


        // ===== VALIDATION =====
        private bool KiemTraHopLe(out string thongBao)
        {
            if (string.IsNullOrWhiteSpace(TenDichVu))
            {
                thongBao = "Vui lòng nhập tên dịch vụ!";
                return false;
            }

            if (string.IsNullOrWhiteSpace(DonViTinh))
            {
                thongBao = "Vui lòng nhập đơn vị tính!";
                return false;
            }

            if (string.IsNullOrWhiteSpace(DonGia) || !decimal.TryParse(DonGia, out decimal donGiaVal) || donGiaVal < 0)
            {
                thongBao = "Đơn giá phải là số và không được âm!";
                return false;
            }

            if (string.IsNullOrWhiteSpace(TonKho) || !int.TryParse(TonKho, out int tonKhoVal) || tonKhoVal < 0)
            {
                thongBao = "Tồn kho phải là số nguyên và không được âm!";
                return false;
            }

            if (string.IsNullOrWhiteSpace(CanhBaoTonKho) || !int.TryParse(CanhBaoTonKho, out int canhBaoVal) || canhBaoVal < 0)
            {
                thongBao = "Cảnh báo tồn kho phải là số nguyên và không được âm!";
                return false;
            }

            if (string.IsNullOrWhiteSpace(HinhAnhRelative))
            {
                thongBao = "Vui lòng thêm hình ảnh cho dịch vụ!";
                return false;
            }

            // Kiểm tra tên dịch vụ đã tồn tại chưa
            bool trungTen = DataProvider.Ins.DB.DichVus
                .Any(dv => dv.TenDichVu.ToLower() == TenDichVu.Trim().ToLower());
            if (trungTen)
            {
                thongBao = $"Dịch vụ '{TenDichVu}' đã tồn tại trong hệ thống!";
                return false;
            }

            thongBao = string.Empty;
            return true;
        }

        // ===== CONSTRUCTOR =====
        public AddDichVuViewModel()
        {
            // ── Nút Thêm Hình Ảnh ──
            ThemHinhAnhCommand = new RelayCommand<Window>((p) => true, (p) =>
            {
                // Mở hộp thoại chọn file ảnh
                OpenFileDialog dialog = new OpenFileDialog
                {
                    Title = "Chọn hình ảnh dịch vụ",
                    Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif"
                };

                if (dialog.ShowDialog() != true) return;

                string sourceFile = dialog.FileName;
                string fileName = Path.GetFileName(sourceFile);

                // Thư mục image nằm cùng với file .exe
                string imageFolder = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory, "image");

                // Tạo folder nếu chưa tồn tại
                if (!Directory.Exists(imageFolder))
                    Directory.CreateDirectory(imageFolder);

                string destFile = Path.Combine(imageFolder, fileName);

                // Nếu file đã tồn tại trong folder thì đặt tên mới tránh trùng
                if (File.Exists(destFile))
                {
                    string nameNoExt = Path.GetFileNameWithoutExtension(fileName);
                    string ext = Path.GetExtension(fileName);
                    string uniqueName = $"{nameNoExt}_{DateTime.Now:yyyyMMddHHmmss}{ext}";
                    destFile = Path.Combine(imageFolder, uniqueName);
                    fileName = uniqueName;
                }

                // Copy ảnh vào folder image
                File.Copy(sourceFile, destFile);

                // Lưu đường dẫn tương đối — đúng format để load lại: "image/tenfile.jpg"
                HinhAnhRelative = $"image/{fileName}";

                // Load preview lên giao diện
                HinhAnhPreview = new BitmapImage(new Uri(destFile, UriKind.Absolute));
            });

            // ── Nút Xác Nhận ──
            XacNhanCommand = new RelayCommand<Window>((p) => true, (p) =>
            {
               
                if (!KiemTraHopLe(out string thongBao))
                {
                    MessageBox.Show(thongBao, "Thông báo",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);
                    return;
                }

                var dichVuMoi = new DichVu
                {
                    TenDichVu = TenDichVu.Trim(),
                    DonViTinh = DonViTinh.Trim(),
                    DonGia = decimal.Parse(DonGia),
                    TonKho = int.Parse(TonKho),
                    CanhBaoTonKho = int.Parse(CanhBaoTonKho),
                    HinhAnh = HinhAnhRelative, // "image/tenfile.jpg"
                    IsEnable = true
                };

                DataProvider.Ins.DB.DichVus.Add(dichVuMoi);
                DataProvider.Ins.DB.SaveChanges();

                // Ghi log
                LoginWindow login = new LoginWindow();
                var loginVM = login.DataContext as LoginViewModel;

                QL_DichVuUC qldv = new QL_DichVuUC();
                var qldvVM = qldv.DataContext as QL_DichVuViewModel;


                SystemLog log = new SystemLog
                {
                    MaNhanVien = loginVM.UserId,
                    ThoiGian = DateTime.Now,
                    HanhDong = "THEM_DICH_VU",
                    MoTa = $"Nhân viên mã {loginVM.UserId} đã thêm " +
                                 $"dịch vụ '{dichVuMoi.TenDichVu}' vào lúc {DateTime.Now}"
                };
                DataProvider.Ins.DB.SystemLogs.Add(log);
                DataProvider.Ins.DB.SaveChanges();

                qldvVM.DSDichVu.Add(dichVuMoi); 
                qldvVM.DSDichVuView.Refresh();


                MessageBox.Show("Thêm dịch vụ thành công! Vui lòng đăng nhập lại hệ thống để cập nhật thông tin !", "Thông báo",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                p?.Close();
            });

            ExitCommand = new RelayCommand<Window>((p) => { return true; }, (p) => {
                p.Close();
            });
        }
    }
}
