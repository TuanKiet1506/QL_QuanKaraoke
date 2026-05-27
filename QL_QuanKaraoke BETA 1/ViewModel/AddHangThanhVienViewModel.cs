using QL_QuanKaraoke_BETA_1.Data;
using QL_QuanKaraoke_BETA_1.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace QL_QuanKaraoke_BETA_1.ViewModel
{
    public class AddHangThanhVienViewModel : BaseViewModel
    {
        private string _tenHang;

        public string TenHang
        {
            get { return _tenHang; }
            set { _tenHang = value; OnPropertyChanged();  }
        }
        private string _ptgg;

        public string Ptgg
        {
            get { return _ptgg; }
            set { _ptgg = value; OnPropertyChanged(); }
        }
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
                    "Phần trăm giảm giá phải là số nguyên",
                    "Thông báo",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return false;
            }

            // > 0 và <= 100
            if (pt <= 0 || pt > 1)
            {
                MessageBox.Show(
                    "Phần trăm giảm giá phải lớn hơn 0 và không vượt quá 1",
                    "Thông báo",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return false;
            }

            return true;
        }
        public ICommand ExitCommand { get; set; }
        public ICommand XacNhanCommand { get; set; }
        public AddHangThanhVienViewModel()
        {
            ExitCommand = new RelayCommand<Window>((p) => { return true; }, (p) => {
                p.Close(); 
            });
            XacNhanCommand = new RelayCommand<Window>((p) => { return true; }, (p) => {
                if (!KiemTraDuLieu()) return;

                HangThanhVien hangThanhVien = new HangThanhVien
                {
                    TenHang = TenHang,
                    PhanTramGiamGia = double.Parse(Ptgg),
                    IsEnable = true
                }; 

                DataProvider.Ins.DB.HangThanhViens.Add(hangThanhVien);
                DataProvider.Ins.DB.SaveChanges();

                LoginWindow ql = new LoginWindow();
                var qlVM = ql.DataContext as LoginViewModel;

                QL_HangThanhVienUC htv = new QL_HangThanhVienUC();
                var htvVM = htv.DataContext as QL_HangThanhVienViewModel;

                SystemLog system = new SystemLog
                {
                    MaNhanVien = qlVM.UserId,
                    HanhDong = "THEM_HANG",
                    ThoiGian = DateTime.Now,
                    MoTa = $"Nhân viên mã {qlVM.UserId} đã thêm một hạng mới vào lúc {DateTime.Now}"
                };
                DataProvider.Ins.DB.SystemLogs.Add(system);
                DataProvider.Ins.DB.SaveChanges();

                MessageBox.Show("Đã thêm hạng mới thành công !!!");  

                htvVM.DSHangThanhVien.Add(hangThanhVien);
                htvVM.DSHangThanhVienView.Refresh();

                p.Close();
            }); 

        }
    }
}
