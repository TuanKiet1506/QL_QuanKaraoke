using QL_QuanKaraoke_BETA_1;
using QL_QuanKaraoke_BETA_1.Data;
using QL_QuanKaraoke_BETA_1.Model;
using QL_QuanKaraoke_BETA_1.ViewModel;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace QL_QuanKaraoke_BETA_1.ViewModel
{
    public class SuaGioViewModel : BaseViewModel
    {
        // =========================
        // GIỜ CŨ
        // =========================

        private TimeSpan? _gioVaoCu;
        public TimeSpan? GioVaoCu
        {
            get => _gioVaoCu;
            set
            {
                _gioVaoCu = value;
                OnPropertyChanged();
            }
        }

        private TimeSpan? _gioRaCu;
        public TimeSpan? GioRaCu
        {
            get => _gioRaCu;
            set
            {
                _gioRaCu = value;
                OnPropertyChanged();
            }
        }

        // =========================
        // DANH SÁCH GIỜ
        // =========================

        private ObservableCollection<TimeSpan> _danhSachGio; 

        public ObservableCollection<TimeSpan> DanhSachGio
        {
            get => _danhSachGio; 
            set
            {
                _danhSachGio = value;
                OnPropertyChanged(); 
            }
        }

        // =========================
        // GIỜ MỚI
        // =========================

        private TimeSpan? _gioVaoMoi;
        public TimeSpan? GioVaoMoi
        {
            get => _gioVaoMoi;
            set
            {
                _gioVaoMoi = value;
                OnPropertyChanged();
            }
        }

        private TimeSpan? _gioRaMoi;
        public TimeSpan? GioRaMoi
        {
            get => _gioRaMoi;
            set
            {
                _gioRaMoi = value;
                OnPropertyChanged();
            }
        }

        // =========================
        // COMMAND
        // =========================

        public ICommand XacNhanSuaGioCommand { get; set; }

        // =========================
        // CONSTRUCTOR
        // =========================

        public SuaGioViewModel()
        {
            LoadDanhSachGio();

            LoadGio();

            XacNhanSuaGioCommand = new RelayCommand<Window>(
                (p) =>
                {
                    return GioRaMoi > GioVaoMoi;
                },

                (p) =>
                {
                    try
                    {
                        QL_BangGiaUC ql = new QL_BangGiaUC();
                        var qlVM = ql.DataContext as QL_BangGiaViewModel;

                        LoginWindow lg = new LoginWindow();
                        var lgVM = lg.DataContext as LoginViewModel;

                        var bangGia = qlVM.SelectedBangGia;

                        bangGia.GioBatDau = GioVaoMoi;
                        bangGia.GioKetThuc = GioRaMoi;

                        DataProvider.Ins.DB.SaveChanges();

                        SystemLog system = new SystemLog
                        {
                            MaNhanVien = lgVM.UserId,
                            HanhDong = "SUA_KHUNG_GIO",
                            ThoiGian = DateTime.Now,
                            MoTa = $"Nhân viên mã {lgVM.UserId} đã thay đổi khung giờ quán cho mã bảng giá {bangGia.MaBangGia} vào lúc {DateTime.Now}"
                        }; 

                        DataProvider.Ins.DB.SystemLogs.Add(system);
                        DataProvider.Ins.DB.SaveChanges(); 

                        MessageBox.Show(
                            "Cập nhật khung giờ thành công",
                            "Thông báo",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information
                        );

                        p?.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                });
        }

        // =========================
        // LOAD GIỜ CŨ
        // =========================

        public void LoadGio()
        {
            QL_BangGiaUC ql = new QL_BangGiaUC();
            var qlVM = ql.DataContext as QL_BangGiaViewModel;

            GioVaoCu = qlVM.SelectedBangGia.GioBatDau;
            GioRaCu = qlVM.SelectedBangGia.GioKetThuc;

            GioVaoMoi = qlVM.SelectedBangGia.GioBatDau;
            GioRaMoi = qlVM.SelectedBangGia.GioKetThuc;
        }

        // =========================
        // TẠO DANH SÁCH GIỜ
        // =========================

        public void LoadDanhSachGio()
        {
            DanhSachGio = new ObservableCollection<TimeSpan>();

            // 00:00 -> 23:30
            for (int hour = 0; hour < 24; hour++)
            {
                DanhSachGio.Add(new TimeSpan(hour, 0, 0));
                DanhSachGio.Add(new TimeSpan(hour, 30, 0));
            }
        }
    }
}