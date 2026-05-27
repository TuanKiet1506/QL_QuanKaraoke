using QL_QuanKaraoke_BETA_1.Data;
using QL_QuanKaraoke_BETA_1.Model;
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
    public class SuaPhongViewModel : BaseViewModel
    {
        // ===== THÔNG TIN PHÒNG =====
        private Phong _phong;
        public Phong Phong
        {
            get => _phong;
            set { _phong = value; OnPropertyChanged(); }
        }

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

        // ===== LOẠI PHÒNG =====
        private ObservableCollection<LoaiPhong> _dsLoaiPhong;
        public ObservableCollection<LoaiPhong> DSLoaiPhong
        {
            get => _dsLoaiPhong;
            set { _dsLoaiPhong = value; OnPropertyChanged(); }
        }

        private LoaiPhong _selectedLoaiPhong;
        public LoaiPhong SelectedLoaiPhong
        {
            get => _selectedLoaiPhong;
            set { _selectedLoaiPhong = value; OnPropertyChanged(); }
        }

        // ===== LOAD THÔNG TIN PHÒNG ĐANG CHỌN =====
        public void LoadThongTin()
        {
            // Lấy SelectedPhong từ QL_PhongViewModel
            QL_PhongUC ql = new QL_PhongUC();
            var qlVM = ql.DataContext as QL_PhongViewModel;

            Phong = qlVM.SelectedPhong;

            if (Phong == null) return;

            // Điền thông tin lên các TextBox
            TenPhong = Phong.TenPhong;
            SucChua = Phong.SucChua.ToString();

            // Chọn đúng loại phòng trong ComboBox
            SelectedLoaiPhong = DSLoaiPhong
                .FirstOrDefault(lp => lp.MaLoaiPhong == Phong.MaLoaiPhong);
        }

        // ===== KIỂM TRA HỢP LỆ =====
        private bool KiemTraHopLe(out string thongBao)
        {
            if (string.IsNullOrWhiteSpace(TenPhong))
            {
                thongBao = "Vui lòng nhập tên phòng!";
                return false;
            }

            if (string.IsNullOrWhiteSpace(SucChua))
            {
                thongBao = "Vui lòng nhập sức chứa!";
                return false;
            }

            if (!int.TryParse(SucChua, out int sucChuaInt))
            {
                thongBao = "Sức chứa phải là số nguyên!";
                return false;
            }

            if (sucChuaInt <= 0)
            {
                thongBao = "Sức chứa phải lớn hơn 0!";
                return false;
            }

            if (SelectedLoaiPhong == null)
            {
                thongBao = "Vui lòng chọn loại phòng!";
                return false;
            }

            thongBao = string.Empty;
            return true;
        }

        // ===== COMMANDS =====
        public ICommand XacNhanSuaCommand { get; set; }
        public ICommand ExitCommand { get; set; }

        // ===== CONSTRUCTOR =====
        public SuaPhongViewModel()
        {
            // Load danh sách loại phòng trước để LoadThongTin() dùng được
            DSLoaiPhong = new ObservableCollection<LoaiPhong>(
                DataProvider.Ins.DB.LoaiPhongs.ToList());

            ExitCommand = new RelayCommand<Window>(
                (p) => true,
                (p) => p.Close());

            XacNhanSuaCommand = new RelayCommand<Window>(
                // CanExecute
                (p) => KiemTraHopLe(out _),

                // Execute
                (p) =>
                {
                    if (!KiemTraHopLe(out string thongBao))
                    {
                        MessageBox.Show(thongBao, "Thông báo",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Warning);
                        return;
                    }

                    // Kiểm tra tên phòng trùng (ngoại trừ chính nó)
                    bool trungTen = DataProvider.Ins.DB.Phongs
                        .Any(x => x.TenPhong.ToLower() == TenPhong.Trim().ToLower()
                               && x.MaPhong != Phong.MaPhong);

                    if (trungTen)
                    {
                        MessageBox.Show($"Tên phòng '{TenPhong}' đã tồn tại!",
                                        "Thông báo",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Warning);
                        return;
                    }

                    MessageBoxResult confirm = MessageBox.Show(
                        "Xác nhận lưu thông tin phòng?",
                        "Xác nhận",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (confirm == MessageBoxResult.Yes)
                    {
                        // Lấy bản ghi từ DB theo mã phòng
                        int maPhong = Phong.MaPhong;
                        var phong = DataProvider.Ins.DB.Phongs
                            .FirstOrDefault(x => x.MaPhong == maPhong);

                        if (phong == null)
                        {
                            MessageBox.Show("Không tìm thấy phòng trong hệ thống!");
                            return;
                        }

                        // Cập nhật thông tin
                        phong.TenPhong = TenPhong.Trim();
                        phong.SucChua = int.Parse(SucChua);
                        phong.MaLoaiPhong = SelectedLoaiPhong.MaLoaiPhong;

                        DataProvider.Ins.DB.SaveChanges();

                        // Ghi log
                        LoginWindow lg = new LoginWindow();
                        var lgVM = lg.DataContext as LoginViewModel;

                        SystemLog systemLog = new SystemLog
                        {
                            MaNhanVien = lgVM.UserId,
                            ThoiGian = DateTime.Now,
                            HanhDong = "SUA_THONG_TIN_PHONG",
                            MoTa = $"Nhân viên mã {lgVM.UserId} đã sửa thông tin " +
                                         $"phòng mã {phong.MaPhong} vào lúc {DateTime.Now}"
                        };
                        DataProvider.Ins.DB.SystemLogs.Add(systemLog);
                        DataProvider.Ins.DB.SaveChanges();

                        MessageBox.Show("Cập nhật thông tin phòng thành công!",
                                        "Thông báo",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Information);

                        p.Close();
                    }
                });
        }
    }
}
