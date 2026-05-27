using LiveCharts;
using LiveCharts.Wpf;
using QL_QuanKaraoke_BETA_1.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace QL_QuanKaraoke_BETA_1.ViewModel
{
    public class TrangChuViewModel : BaseViewModel
    {
        // =====================================================
        // CARD THỐNG KÊ
        // =====================================================

        private string _doanhThuThang;
        public string DoanhThuThang
        {
            get => _doanhThuThang;
            set
            {
                _doanhThuThang = value;
                OnPropertyChanged();
            }
        }

        private int _luotDatPhong;
        public int LuotDatPhong
        {
            get => _luotDatPhong;
            set
            {
                _luotDatPhong = value;
                OnPropertyChanged();
            }
        }

        private string _dichVuBanChay;
        public string DichVuBanChay
        {
            get => _dichVuBanChay;
            set
            {
                _dichVuBanChay = value;
                OnPropertyChanged();
            }
        }

        private int _phongTrong;
        public int PhongTrong
        {
            get => _phongTrong;
            set
            {
                _phongTrong = value;
                OnPropertyChanged();
            }
        }

        // =====================================================
        // DATE PICKER
        // =====================================================

        private DateTime _ngayThongKe = DateTime.Now;

        public DateTime NgayThongKe
        {
            get => _ngayThongKe;
            set
            {
                _ngayThongKe = value;
                OnPropertyChanged();

                LoadBieuDoDoanhThu();
            }
        }

        // =====================================================
        // BIỂU ĐỒ DOANH THU
        // =====================================================

        public SeriesCollection SeriesCollection { get; set; }

        public List<string> Labels { get; set; }

        public Func<double, string> Formatter { get; set; }

        // =====================================================
        // BIỂU ĐỒ KHÁCH HÀNG
        // =====================================================

        public SeriesCollection PieSeriesCollection { get; set; }

        // =====================================================
        // CONSTRUCTOR
        // =====================================================

        public TrangChuViewModel()
        {
            LoadThongKe();

            LoadBieuDoDoanhThu();

            LoadBieuDoKhachHang();
        }

        // =====================================================
        // LOAD THỐNG KÊ CARD
        // =====================================================

        public void LoadThongKe()
        {
            try
            {
                var thangHienTai = DateTime.Now.Month;
                var namHienTai = DateTime.Now.Year;

                // =========================================
                // DOANH THU THÁNG
                // =========================================

                decimal doanhThu = DataProvider.Ins.DB.HoaDons
                    .Where(x =>
                        x.TrangThai == "DA_THANH_TOAN" &&
                        x.GioRa.HasValue &&
                        x.GioRa.Value.Month == thangHienTai &&
                        x.GioRa.Value.Year == namHienTai)
                    .Sum(x => (decimal?)x.TongTien) ?? 0;

                if (doanhThu >= 1000000)
                {
                    DoanhThuThang =
                        (doanhThu / 1000000m).ToString("0.#") + " tr";
                }
                else
                {
                    DoanhThuThang =
                        doanhThu.ToString("N0") + "đ";
                }

                // =========================================
                // LƯỢT ĐẶT PHÒNG
                // =========================================

                LuotDatPhong = DataProvider.Ins.DB.HoaDons
                    .Count(x =>
                        x.GioVao.HasValue &&
                        x.GioVao.Value.Month == thangHienTai &&
                        x.GioVao.Value.Year == namHienTai);

                // =========================================
                // DỊCH VỤ BÁN CHẠY
                // =========================================

                var dichVu = DataProvider.Ins.DB.ChiTietHoaDons
                    .GroupBy(x => x.MaDichVu)
                    .Select(g => new
                    {
                        MaDichVu = g.Key,
                        TongSoLuong = g.Sum(x => x.SoLuong)
                    })
                    .OrderByDescending(x => x.TongSoLuong)
                    .FirstOrDefault();

                if (dichVu != null)
                {
                    DichVuBanChay = DataProvider.Ins.DB.DichVus
                        .Where(x => x.MaDichVu == dichVu.MaDichVu)
                        .Select(x => x.TenDichVu)
                        .FirstOrDefault();
                }
                else
                {
                    DichVuBanChay = "Không có";
                }

                // =========================================
                // PHÒNG TRỐNG
                // =========================================

                PhongTrong = DataProvider.Ins.DB.Phongs
                    .Count(x => x.TrangThai == "Trống");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        // =====================================================
        // LOAD BIỂU ĐỒ DOANH THU
        // =====================================================

        public void LoadBieuDoDoanhThu()
        {
            try
            {
                DateTime ngayBatDau = NgayThongKe.Date;

                DateTime ngayKetThuc =
                    ngayBatDau.AddMonths(1);

                var data = DataProvider.Ins.DB.HoaDons
                    .Where(x =>
                        x.TrangThai == "DA_THANH_TOAN" &&
                        x.GioRa.HasValue &&
                        x.GioRa.Value >= ngayBatDau &&
                        x.GioRa.Value <= ngayKetThuc)
                    .ToList()
                    .GroupBy(x => x.GioRa.Value.Date)
                    .Select(g => new
                    {
                        Ngay = g.Key,
                        TongTien = g.Sum(x => x.TongTien)
                    })
                    .OrderBy(x => x.Ngay)
                    .ToList();

                Labels = data
                    .Select(x => x.Ngay.ToString("dd/MM"))
                    .ToList();

                SeriesCollection = new SeriesCollection
                {
                    new ColumnSeries
                    {
                        Title = "Doanh thu",

                        Values = new ChartValues<decimal>(
                            data.Select(x => x.TongTien ?? 0)
                        ),

                        MaxColumnWidth = 40,

                        Fill = new SolidColorBrush(
                            (Color)ColorConverter.ConvertFromString("#8B5CF6"))
                    }
                };

                Formatter = value =>
                    (value / 1000000).ToString("0.#") + "tr";

                OnPropertyChanged(nameof(SeriesCollection));
                OnPropertyChanged(nameof(Labels));
                OnPropertyChanged(nameof(Formatter));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        // =====================================================
        // LOAD BIỂU ĐỒ KHÁCH HÀNG
        // =====================================================

        public void LoadBieuDoKhachHang()
        {
            try
            {
                var data = DataProvider.Ins.DB.KhachHangs
                    .GroupBy(x => x.HangThanhVien.TenHang)
                    .Select(g => new
                    {
                        TenHang = g.Key,
                        SoLuong = g.Count()
                    })
                    .ToList();

                PieSeriesCollection = new SeriesCollection();

                foreach (var item in data)
                {
                    PieSeriesCollection.Add(
                        new PieSeries
                        {
                            Title = item.TenHang,

                            Values = new ChartValues<int>
                            {
                                item.SoLuong
                            },

                            DataLabels = true
                        });
                }

                OnPropertyChanged(nameof(PieSeriesCollection));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}