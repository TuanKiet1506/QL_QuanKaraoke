using QL_QuanKaraoke_BETA_1.Data;
using QL_QuanKaraoke_BETA_1.Model;
using QL_QuanKaraoke_BETA_1.View;
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
    public class GoiMonViewModel : BaseViewModel
    {
		private int _tongSoMon;

		public int TongSoMon
        {
			get { return _tongSoMon; }
			set { _tongSoMon = value; OnPropertyChanged(); }
		}
		private int _tongSoMonDaGoi;

		public int TongSoMonDaGoi
        {
			get { return _tongSoMonDaGoi; }
			set { _tongSoMonDaGoi = value; OnPropertyChanged(); }
		}

		private string _tenPhong;

		public string TenPhong
		{
			get { return _tenPhong; }
			set { _tenPhong = value; OnPropertyChanged(); }
		}
		private string _loaiPhong;

		public string LoaiPhong
		{
			get { return _loaiPhong; }
			set { _loaiPhong = value; OnPropertyChanged(); }
		}
		private decimal? _tongTien;

		public decimal? TongTien
		{
			get { return _tongTien; }
			set { _tongTien = value; OnPropertyChanged(); }
		}

		private ObservableCollection<DichVu> _danhSachDichVu;

		public ObservableCollection<DichVu> DanhSachDichVu
        {
			get { return _danhSachDichVu; }
			set { _danhSachDichVu = value; OnPropertyChanged(); }
		}
        public IEnumerable<DichVu> DanhSachDaChon
        {
            get => DanhSachDichVu.Where(x => x.SoLuong > 0);
        }
        private bool _tonKhoThap;

        public bool TonKhoThap
        {
            get { return _tonKhoThap; }
            set { _tonKhoThap = value; OnPropertyChanged(); }
        }
        private string _tuKhoa;
        public string TuKhoa
        {
            get => _tuKhoa;
            set
            {
                _tuKhoa = value;
                OnPropertyChanged();
                DanhSachDichVuView.Refresh(); 
            }
        }
        public ICollectionView DanhSachDichVuView { get; set; }
        public ICommand GiamSoLuongCommand { get; set; }
		public ICommand TangSoLuongCommand { get; set; }
		public ICommand DatLaiCommand { get; set; }
		public ICommand XacNhanCommand { get; set; }


        public void LoadDichVu()
		{
            var data = DataProvider.Ins.DB.DichVus.ToList();

            DanhSachDichVu = new ObservableCollection<DichVu>(data);
        }
		private void LoadTenPhong()
		{
			MoPhong mp = new MoPhong();
			var mpVM = mp.DataContext as MoPhongViewModel;

            int mahd = mpVM.LayMaHoaDon();
			var hoadon = DataProvider.Ins.DB.HoaDons.Where(hd => hd.MaHoaDon == mahd).FirstOrDefault();
			if (hoadon != null)
			{
				TenPhong = hoadon.Phong.TenPhong.ToString(); 
				LoaiPhong = hoadon.Phong.LoaiPhong.TenLoai.ToString();
			}
			mp.Close();
		}
		private bool DieuKienXacNhan()
		{
            foreach (var dv in DanhSachDichVu)
            {
                if (dv.SoLuong > 0)
                    return true;
            }
            return false;
        }
        private bool LocDichVu(object obj)
        {
            if (obj is DichVu dv)
            {
                if (string.IsNullOrWhiteSpace(TuKhoa))
                    return true;

                return dv.TenDichVu.ToLower().Contains(TuKhoa.ToLower());
            }
            return false;
        }

        public GoiMonViewModel() {
			LoadDichVu();
			LoadTenPhong();

			TongSoMon = DanhSachDichVu.Count();
			TongTien = 0;
			TongSoMonDaGoi = 0;

            DanhSachDichVuView = CollectionViewSource.GetDefaultView(DanhSachDichVu);
            DanhSachDichVuView.Filter = LocDichVu;

            GiamSoLuongCommand = new RelayCommand<object>((p) => {
                var dv = p as DichVu;
                return dv != null && dv.SoLuong > 0;
            }, (p) =>
			{
                var dv = p as DichVu;
                dv.SoLuong--;
                OnPropertyChanged(nameof(DanhSachDaChon));

				// Nên tính tổng tiền sau mỗi lần ấn 
				TongTien -= dv.DonGia;
				// Tính số món đã gọi 
				TongSoMonDaGoi--; 
				
            });
            TangSoLuongCommand = new RelayCommand<object>((p) => {
                var dv = p as DichVu;
				return dv != null && dv.SoLuong < dv.TonKho; 
            }, (p) =>
			{
                var dv = p as DichVu;
                dv.SoLuong++;
                OnPropertyChanged(nameof(DanhSachDaChon));

                // Nên tính tổng tiền sau mỗi lần ấn 
                TongTien += dv.DonGia;
                TongSoMonDaGoi++;

            });
            DatLaiCommand = new RelayCommand<object>((p) => {
				return true; 
            }, (p) =>
			{
                foreach (var dv in DanhSachDichVu)
                {
                    dv.SoLuong = 0;
                }
				// Reset lại tổng tiền và số món đã gọi 
				OnPropertyChanged(nameof(DanhSachDaChon));
				TongTien = 0; 
				TongSoMonDaGoi = 0;
            });
            XacNhanCommand = new RelayCommand<Window>((p) => {
				if (DieuKienXacNhan()) return true;
				return false; 
            }, (p) =>
			{
                // 1. Thêm hoàn toàn vào bảng chi tiết hóa đơn 
                // 2. Trừ và cập nhật lại số lượng tồn kho 
                MoPhong mp = new MoPhong();
                var mpVM = mp.DataContext as MoPhongViewModel;

                LoginWindow login = new LoginWindow();
                var loginVM = login.DataContext as LoginViewModel;


                var danhSachChon = DanhSachDichVu.Where(x => x.SoLuong > 0).ToList();

                foreach (var dv in danhSachChon)
                {
                    var ct = new ChiTietHoaDon
                    {
                        MaHoaDon = mpVM.LayMaHoaDon(),
                        MaDichVu = dv.MaDichVu,
                        SoLuong = dv.SoLuong,
                        DonGia = dv.DonGia,
						ThanhTien = dv.SoLuong * dv.DonGia
                    };

                    DataProvider.Ins.DB.ChiTietHoaDons.Add(ct);

                    // Cập nhật tồn kho 
                    var dichvu = DataProvider.Ins.DB.DichVus
                    .Where(d => d.MaDichVu == dv.MaDichVu)
                    .FirstOrDefault();

                    dichvu.TonKho = dichvu.TonKho - dv.SoLuong; 

                }

                DataProvider.Ins.DB.SaveChanges();


                // Lưu lại lịch sử SystemLogs: 
                SystemLog systemLog = new SystemLog
                {
                    MaNhanVien = loginVM.UserId,
                    MoTa = $"Nhân viên có mã {loginVM.UserId} đã phục vụ {TongSoMonDaGoi} món ăn vào phòng {TenPhong} vào lúc {DateTime.Now} với tổng tiền {TongTien}",
                    HanhDong = "PHUC_VU_DICH_VU",
                    ThoiGian = DateTime.Now
                }; 
                DataProvider.Ins.DB.SystemLogs.Add(systemLog);
                DataProvider.Ins.DB.SaveChanges();

                // Reset lại danh sách
                foreach (var dv in DanhSachDichVu)
                {
                    dv.SoLuong = 0;
                }
                TongTien = 0;
                TongSoMonDaGoi = 0;

                mp.Close();
				p.Close();
            }); 


        }
    }
}
