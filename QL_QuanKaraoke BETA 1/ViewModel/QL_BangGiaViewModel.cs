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
    public class QL_BangGiaViewModel : BaseViewModel
    {
        private ObservableCollection<BangGia> _dsBangGia;

        public ObservableCollection<BangGia> DSBangGia
        {
            get { return _dsBangGia; }
            set { _dsBangGia = value;OnPropertyChanged(); }
        }
        private string _loaiPhong;

        public string LoaiPhong
        {
            get { return _loaiPhong; }
            set { _loaiPhong = value; OnPropertyChanged(); }
        }
        private string _donGia;

        public string DonGia
        {
            get { return _donGia; }
            set { _donGia = value; OnPropertyChanged(); }
        }
        private BangGia _selectedBangGia;

        public BangGia SelectedBangGia
        {
            get { return _selectedBangGia; }
            set { _selectedBangGia = value; OnPropertyChanged(); }
        }
        public ICommand SuaGiaCommand { get; set; }
        public ICommand SuaKhungGioCommand { get; set; }
        public ICommand KhoiPhucMacDinhCommand { get; set; }

        public QL_BangGiaViewModel() {

            DSBangGia = new ObservableCollection<BangGia>(DataProvider.Ins.DB.BangGias);

            SuaGiaCommand = new RelayCommand<object>((p) => { return SelectedBangGia != null; }, (p) => {

                SuaGia suaGia = new SuaGia();

                SuaGia ql = new SuaGia();
                var qlVM = ql.DataContext as SuaGiaViewModel;

                qlVM.LoadGia(); 

                suaGia.ShowDialog();

                DSBangGia = new ObservableCollection<BangGia>(DataProvider.Ins.DB.BangGias);

            });
            SuaKhungGioCommand = new RelayCommand<object>((p) => { return SelectedBangGia != null; }, (p) => {

                SuaGio suaGio = new SuaGio();

                SuaGio ql = new SuaGio();
                var qlVM = ql.DataContext as SuaGioViewModel;

                qlVM.LoadGio();

                suaGio.ShowDialog();

                DSBangGia = new ObservableCollection<BangGia>(DataProvider.Ins.DB.BangGias);

            });
            KhoiPhucMacDinhCommand = new RelayCommand<object>(
    (p) =>
    {
        return true; 
    },

    (p) =>
    {
        LoginWindow ql = new LoginWindow();
        var qlVM = ql.DataContext as LoginViewModel;

        var result = MessageBox.Show(
            "Bạn có muốn khôi phục toàn bộ bảng giá mặc định không?",
            "Xác nhận",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes)
            return;

        try
        {
            var dsBangGia = DataProvider.Ins.DB.BangGias.ToList();

            foreach (var bg in dsBangGia)
            {
                switch (bg.MaBangGia)
                {
                    case 1:
                        bg.GioBatDau = new TimeSpan(8, 0, 0);
                        bg.GioKetThuc = new TimeSpan(17, 0, 0);
                        bg.Gia = 200000;
                        break;

                    case 2:
                        bg.GioBatDau = new TimeSpan(17, 0, 0);
                        bg.GioKetThuc = new TimeSpan(23, 59, 0);
                        bg.Gia = 300000;
                        break;

                    case 3:
                        bg.GioBatDau = new TimeSpan(8, 0, 0);
                        bg.GioKetThuc = new TimeSpan(17, 0, 0);
                        bg.Gia = 100000;
                        break;

                    case 4:
                        bg.GioBatDau = new TimeSpan(17, 0, 0);
                        bg.GioKetThuc = new TimeSpan(23, 59, 0);
                        bg.Gia = 150000;
                        break;
                }
            }

            DataProvider.Ins.DB.SaveChanges();

            SystemLog system = new SystemLog
            {
                MaNhanVien = qlVM.UserId,
                ThoiGian = DateTime.Now,
                HanhDong = "RESET_BANG_GIA",
                MoTa = $"Nhân viên có mã {qlVM.UserId} đã reset bảng giá vào lúc {DateTime.Now}"
            };
            DataProvider.Ins.DB.SystemLogs.Add(system);
            DataProvider.Ins.DB.SaveChanges();

            MessageBox.Show(
                "Đã khôi phục dữ liệu mặc định",
                "Thông báo",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            DSBangGia = new ObservableCollection<BangGia>(DataProvider.Ins.DB.BangGias);

        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    });
        }
    }
}
