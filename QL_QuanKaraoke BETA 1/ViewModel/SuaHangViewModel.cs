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
    public class SuaHangViewModel : BaseViewModel
    {
        private string _tenHang;
        public string TenHang
        {
            get { return _tenHang; }
            set { _tenHang = value; OnPropertyChanged(); }
        }

        private string _ptgg;
        public string Ptgg
        {
            get { return _ptgg; }
            set { _ptgg = value; OnPropertyChanged(); }
        }

        private ObservableCollection<string> _trangThai;
        public ObservableCollection<string> TrangThai
        {
            get { return _trangThai; }
            set { _trangThai = value; OnPropertyChanged(); }
        }

        private string _trangThaiDangChon;
        public string TrangThaiDangChon
        {
            get { return _trangThaiDangChon; }
            set { _trangThaiDangChon = value; OnPropertyChanged(); }
        }

        private HangThanhVien _hangThanhVien;
        public HangThanhVien HangThanhVien
        {
            get { return _hangThanhVien; }
            set { _hangThanhVien = value; OnPropertyChanged(); }
        }

        public ICommand XacNhanCommand { get; set; }
        public ICommand ExitCommand { get; set; }

        // =====================================================
        // VALIDATE
        // =====================================================

        public bool KiemTraDuLieu()
        {
            if (string.IsNullOrWhiteSpace(TenHang))
            {
                MessageBox.Show(
                    "Tên hạng không được để trống",
                    "Thông báo",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return false;
            }

            if (string.IsNullOrWhiteSpace(Ptgg))
            {
                MessageBox.Show(
                    "Phần trăm giảm giá không được để trống",
                    "Thông báo",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return false;
            }

            // Phải là số
            if (!float.TryParse(Ptgg, out float pt))
            {
                MessageBox.Show(
                    "Phần trăm giảm giá phải là số",
                    "Thông báo",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return false;
            }

            // 0 -> 1
            if (pt < 0 || pt > 1)
            {
                MessageBox.Show(
                    "Phần trăm giảm giá phải nằm trong khoảng từ 0 -> 1",
                    "Thông báo",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return false;
            }

            return true;
        }

        // =====================================================
        // LOAD DỮ LIỆU
        // =====================================================

        public void LoadThongTin()
        {
            QL_HangThanhVienUC ql = new QL_HangThanhVienUC();
            var qlVM = ql.DataContext as QL_HangThanhVienViewModel;

            if (qlVM == null || qlVM.SelectedHangThanhVien == null)
                return;

            HangThanhVien = qlVM.SelectedHangThanhVien;

            TenHang = HangThanhVien.TenHang;

            Ptgg = HangThanhVien.PhanTramGiamGia.ToString();
        }

        // =====================================================
        // CONSTRUCTOR
        // =====================================================

        public SuaHangViewModel()
        {

            LoadThongTin();

            ExitCommand = new RelayCommand<Window>(
                (p) => true,

                (p) =>
                {
                   p.Close();
                });
            XacNhanCommand = new RelayCommand<Window>(
                (p) => true,

                (p) =>
                {
                    if (!KiemTraDuLieu())
                        return;

                    try
                    {
                        LoginWindow ql = new LoginWindow();
                        var qlVM = ql.DataContext as LoginViewModel;

                        HangThanhVien.TenHang = TenHang;

                        HangThanhVien.PhanTramGiamGia =
                            double.Parse(Ptgg);


                        DataProvider.Ins.DB.SaveChanges();

                        SystemLog systemLog = new SystemLog
                        {
                            MaNhanVien = qlVM.UserId,
                            ThoiGian = DateTime.Now,
                            HanhDong = "SUA_HANG",
                            MoTa = $"Nhân viên mã {qlVM.UserId} đã cập nhật thông tin hạng vào lúc {DateTime.Now}"
                        }; 
                        DataProvider.Ins.DB.SystemLogs.Add(systemLog);
                        DataProvider.Ins.DB.SaveChanges();


                        MessageBox.Show(
                            "Cập nhật hạng thành viên thành công",
                            "Thông báo",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);

                   

                        p?.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                });
        }
    }
}
