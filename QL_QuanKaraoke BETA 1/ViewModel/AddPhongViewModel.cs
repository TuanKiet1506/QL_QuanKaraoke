using QL_QuanKaraoke_BETA_1.Data;
using QL_QuanKaraoke_BETA_1.Model;
using QL_QuanKaraoke_BETA_1.View;
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
    public class AddPhongViewModel : BaseViewModel
    {
        private string _tenPhong;
        public string TenPhong
        {
            get { return _tenPhong; }
            set { _tenPhong = value; OnPropertyChanged(); }
        }

        private string _sucChua;
        public string SucChua
        {
            get { return _sucChua; }
            set { _sucChua = value; OnPropertyChanged(); }
        }

        private LoaiPhong _selectedLoaiPhong;
        public LoaiPhong SelectedLoaiPhong
        {
            get { return _selectedLoaiPhong; }
            set { _selectedLoaiPhong = value; OnPropertyChanged(); }
        }

        private ObservableCollection<LoaiPhong> _dsLoaiPhong;
        public ObservableCollection<LoaiPhong> DSLoaiPhong
        {
            get { return _dsLoaiPhong; }
            set { _dsLoaiPhong = value; OnPropertyChanged(); }
        }

        public ICommand XacNhanCommand { get; set; }
        public ICommand ExitCommand { get; set; }

        // ===== VALIDATION =====
        private bool KiemTraHopLe(out string thongBao)
        {
            // Kiểm tra tên phòng
            if (string.IsNullOrWhiteSpace(TenPhong))
            {
                thongBao = "Vui lòng nhập tên phòng!";
                return false;
            }

            // Kiểm tra sức chứa không để trống
            if (string.IsNullOrWhiteSpace(SucChua))
            {
                thongBao = "Vui lòng nhập sức chứa!";
                return false;
            }

            // Kiểm tra sức chứa phải là số nguyên
            if (!int.TryParse(SucChua, out int sucChuaInt))
            {
                thongBao = "Sức chứa phải là số nguyên!";
                return false;
            }

            // Kiểm tra sức chứa không âm và lớn hơn 0
            if (sucChuaInt <= 0)
            {
                thongBao = "Sức chứa phải lớn hơn 0!";
                return false;
            }

            // Kiểm tra loại phòng đã chọn chưa
            if (SelectedLoaiPhong == null)
            {
                thongBao = "Vui lòng chọn loại phòng!";
                return false;
            }

            // Kiểm tra tên phòng đã tồn tại chưa
            bool trungTen = DataProvider.Ins.DB.Phongs
                            .Any(p => p.TenPhong.ToLower() == TenPhong.Trim().ToLower());
            if (trungTen)
            {
                thongBao = $"Tên phòng '{TenPhong}' đã tồn tại!";
                return false;
            }

            thongBao = string.Empty;
            return true;
        }

        // ===== LOAD =====
        private void LoadLoaiPhong()
        {
            var data = DataProvider.Ins.DB.LoaiPhongs.ToList();
            DSLoaiPhong = new ObservableCollection<LoaiPhong>(data);
        }

        // ===== CONSTRUCTOR =====
        public AddPhongViewModel()
        {
            LoadLoaiPhong();

            XacNhanCommand = new RelayCommand<Window>((p) => true, (p) =>
            {
                // Kiểm tra hợp lệ trước
                if (!KiemTraHopLe(out string thongBao))
                {
                    MessageBox.Show(thongBao, "Thông báo",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);
                    return;
                }

                // Tạo phòng mới
                var phongMoi = new Phong
                {
                    TenPhong = TenPhong.Trim(),
                    SucChua = int.Parse(SucChua),
                    MaLoaiPhong = SelectedLoaiPhong.MaLoaiPhong,
                    TrangThai = "Trống" // Mặc định khi thêm mới
                };

                DataProvider.Ins.DB.Phongs.Add(phongMoi);
                DataProvider.Ins.DB.SaveChanges();

                MessageBox.Show("Thêm phòng thành công!", "Thông báo",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);

                DatPhongUC mp = new DatPhongUC();
                var mpVM = mp.DataContext as DatPhongViewModel;

                LoginWindow lg = new LoginWindow();
                var lgVM = lg.DataContext as LoginViewModel;

                QL_PhongUC qlp = new QL_PhongUC();
                var qlpVM = qlp.DataContext as QL_PhongViewModel;

                mpVM.LoadPhong();
                mpVM.RefreshStats();

                qlpVM.DanhSachPhong.Add(phongMoi);
                qlpVM.DanhSachPhongView.Refresh(); 

                SystemLog system = new SystemLog
                {
                    MaNhanVien = lgVM.UserId,
                    HanhDong = "THEM_PHONG_MOI",
                    ThoiGian = DateTime.Now,
                    MoTa = $"Nhân viên mã {lgVM.UserId} đã thêm phòng mới vào lúc {DateTime.Now}"
                };

                DataProvider.Ins.DB.SystemLogs.Add(system);
                DataProvider.Ins.DB.SaveChanges();

                p?.Close();
            });

            ExitCommand = new RelayCommand<Window>((p) => { return true; }, (p) => { 
                p.Close();
            });
        }
    }
}
