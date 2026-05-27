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
    public class ChuyenPhongViewModel : BaseViewModel
    {

        private ObservableCollection<Phong> _danhSachPhongTrong;

        public ObservableCollection<Phong> DanhSachPhongTrong
        {
            get { return _danhSachPhongTrong; }
            set { _danhSachPhongTrong = value; OnPropertyChanged(); }
        }
        private Phong _phongCu;

        public Phong PhongCu
        {
            get { return _phongCu; }
            set { _phongCu = value; OnPropertyChanged();  }
        }

        private Phong _selectedPhongMoi;

        public Phong SelectedPhongMoi
        {
            get { return _selectedPhongMoi; }
            set { _selectedPhongMoi = value; OnPropertyChanged(); }
        }
        public ICommand XacNhanChuyenPhongCommand { get; set; }

        public void LoadPhongTrong(int maPhong)
        {
            // Phòng nào trống mới được chuyển qua, các phòng bảo trì/đợi dọn cũng không được chuyển 
            var phong = DataProvider.Ins.DB.Phongs.Where(i => i.TrangThai.Trim() == "Trống");

            DanhSachPhongTrong = new ObservableCollection<Phong>(phong);


            var phong_cu = DataProvider.Ins.DB.Phongs.Where(p => p.MaPhong == maPhong).FirstOrDefault();

            if (phong_cu != null)
            {
                PhongCu = phong_cu;
            }

        }
        public ChuyenPhongViewModel()
        {
            XacNhanChuyenPhongCommand = new RelayCommand<Window>((p) => { 
                return SelectedPhongMoi != null;
            }, (p) => { 
            
                MessageBoxResult messageBoxResult = MessageBox.Show("Bạn có chắc chắn muốn chuyển phòng ?"
                    , "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    // 1. Lấy cái phòng mà người ta định chuyển qua 
                    // 2. Thay đổi trạng thái của phòng đó -> Đang Dùng 
                    // 3. Lấy phòng cũ 
                    // 4. Chuyển trạng thái phòng cũ -> Trống 
                    // 5. Thay đổi mã phòng của hóa đơn sang phòng mới 
                    // 6. Lưu lịch sử chuyển phòng 
                    // 7. Lưu SystemLogs 

                    var phong_Moi = DataProvider.Ins.DB.Phongs
                    .Where(x => x.MaPhong == SelectedPhongMoi.MaPhong)
                    .FirstOrDefault();

                    phong_Moi.TrangThai = "Đang Dùng"; 
                    DataProvider.Ins.DB.SaveChanges();

                    var phong_cu = DataProvider.Ins.DB.Phongs
                    .Where(x => x.MaPhong == PhongCu.MaPhong)
                    .FirstOrDefault();

                    phong_cu.TrangThai = "Trống"; 
                    DataProvider.Ins.DB.SaveChanges();

                    DatPhongUC datPhongUC = new DatPhongUC();
                    var dpVM = datPhongUC.DataContext as DatPhongViewModel;

                    MoPhong moPhong = new MoPhong();
                    var mpVM = moPhong.DataContext as MoPhongViewModel;

                    var hoaDonCu = phong_cu.HoaDons.Where(x => x.MaHoaDon == mpVM.LayMaHoaDon()).FirstOrDefault(); 


                    LichSuChuyenPhong lichSuChuyenPhong = new LichSuChuyenPhong
                    {
                        MaHoaDon = hoaDonCu.MaHoaDon,
                        MaPhongCu = phong_cu.MaPhong,
                        MaPhongMoi = phong_Moi.MaPhong,
                        ThoiGian = DateTime.Now
                    };  

                    DataProvider.Ins.DB.LichSuChuyenPhongs.Add(lichSuChuyenPhong);
                    DataProvider.Ins.DB.SaveChanges();

                    hoaDonCu.MaPhong = phong_Moi.MaPhong;
                    DataProvider.Ins.DB.SaveChanges();


                    SystemLog systemLog = new SystemLog
                    {
                        MaNhanVien = hoaDonCu.MaNhanVien,
                        HanhDong = "CHUYEN_PHONG",
                        ThoiGian = DateTime.Now,
                        MoTa = $"Nhân viên có mã {hoaDonCu.MaNhanVien} đã chuyển phòng {phong_cu.TenPhong} sang phòng {phong_Moi.TenPhong} vào lúc {DateTime.Now}"
                    }; 
                    DataProvider.Ins.DB.SystemLogs.Add(systemLog);
                    DataProvider.Ins.DB.SaveChanges(); 

                    dpVM.LoadPhong();
                    dpVM.RefreshStats();

                    moPhong.Close();

                    p.Close();
                }
                else
                {

                }
           
            }); 
        }
    }
}
