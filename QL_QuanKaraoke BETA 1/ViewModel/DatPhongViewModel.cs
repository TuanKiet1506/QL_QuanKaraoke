using QL_QuanKaraoke_BETA_1.Data;
using QL_QuanKaraoke_BETA_1.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace QL_QuanKaraoke_BETA_1.ViewModel
{
    public class DatPhongViewModel : BaseViewModel
    {


        private ObservableCollection<Phong> _danhSachPhong;
        public ObservableCollection<Phong> DanhSachPhong
        {
            get => _danhSachPhong;
            set
            {
                _danhSachPhong = value;
                OnPropertyChanged();
                RefreshStats();
            }
        }
        public int TongPhong => DanhSachPhong?
    .Count(p => p.IsEnable == true) ?? 0;

        public int SoPhongTrong => DanhSachPhong?
            .Count(p => p.IsEnable == true && p.TrangThai == "Trống") ?? 0;

        public int SoPhongDangDung => DanhSachPhong?
            .Count(p => p.IsEnable == true && p.TrangThai == "Đang Dùng") ?? 0;

        public int SoPhongKhac => DanhSachPhong?
            .Count(p => p.IsEnable == true &&
                       (p.TrangThai == "Dọn Dẹp" || p.TrangThai == "Bảo Trì")) ?? 0;

        private Phong _selectedPhong;
        public Phong SelectedPhong
        {
            get => _selectedPhong;
            set { _selectedPhong = value; OnPropertyChanged(); }
        }
        private int _maPhong;

        public int MaPhong
        {
            get { return _maPhong; }
            set { _maPhong = value; OnPropertyChanged(); }
        }


        public ICommand MoMenuCommand { get; set; }
        public DatPhongViewModel()
        {
            LoadPhong();
            MoMenuCommand = new RelayCommand<Phong>((p) => { return true; }, (p) =>
            {
                // Mở phòng tùy theo loại phòng 
                if (p.TrangThai.ToString() == "Trống" && p.LoaiPhong != null)
                {
                    SelectedPhong = p;
                    MaPhong = p.MaPhong;

                    MenuPhongTrong menuPhongTrong = new MenuPhongTrong();
                    menuPhongTrong.ShowDialog();

                    var menuPhongTrongVM = menuPhongTrong.DataContext as MenuPhongTrongViewModel;

                    // 1. Thoát -> không làm gì
                    if (menuPhongTrongVM.isExit)
                    {
                        menuPhongTrongVM.isExit = false;
                        return;
                    }

                    // 2. Bảo trì -> xử lý riêng
                    if (menuPhongTrongVM.isBaoTri)
                    {
                        p.TrangThai = "Bảo Trì";
                        DataProvider.Ins.DB.SaveChanges();

                        LoadPhong();
                        RefreshStats();

                        menuPhongTrongVM.isBaoTri = false;
                        return;
                    }

                    // 3. Mở phòng (chỉ khi user chọn mở phòng)
                    if (menuPhongTrongVM.isMoPhong)
                    {
                        MoPhong moPhong = new MoPhong();
                        moPhong.ShowDialog();

                        var moPhongVM = moPhong.DataContext as MoPhongViewModel;

                        // Nếu nhập dang dở rồi thoát -> không làm gì
                        if (moPhongVM.IsExit)
                        {
                            moPhongVM.Reset();
                            return;
                        }

                        // Thành công -> chuyển sang Đang Dùng
                        p.TrangThai = "Đang Dùng";
                        DataProvider.Ins.DB.SaveChanges();

                        LoadPhong();
                        RefreshStats();
                    }
                }
                else if (p.TrangThai.ToString() == "Đang Dùng" && p.LoaiPhong != null)
                {
                    // Nếu phòng đang dùng thì có 3 lựa chọn 
                    // -> Thanh toán, Order, Chuyển Phòng 

                    ChuyenPhong mp = new ChuyenPhong();
                    var mpVM = mp.DataContext as ChuyenPhongViewModel;

                    SelectedPhong = p; 
                    MaPhong = p.MaPhong;


                    mpVM.LoadPhongTrong(MaPhong); 

                    MenuPhongDangDung menu = new MenuPhongDangDung(); 
                    mp.Close();
                    menu.ShowDialog();

                    ThanhToanWindow tt = new ThanhToanWindow();
                    var ttVM = tt.DataContext as ThanhToanViewModel;

                    if (ttVM.isThanhToan)
                    {
                        p.TrangThai = "Dọn Dẹp";
                        DataProvider.Ins.DB.SaveChanges();

                        LoadPhong();
                        RefreshStats();
                        // Reset lại trạng thái thanh toán 
                        ttVM.isThanhToan = false; 
                    }
                }
                else if (p.TrangThai.ToString() == "Dọn Dẹp" && p.LoaiPhong != null)
                {
                    MessageBoxResult message = MessageBox.Show("Xác nhận đã dọn xong phòng ?"
                  , "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (message == MessageBoxResult.Yes)
                    {
                        p.TrangThai = "Trống";
                        DataProvider.Ins.DB.SaveChanges();

                        LoadPhong();
                        RefreshStats();
                    }
                    else
                    {

                    }
                }
                else if (p.TrangThai == "Bảo Trì" && p.LoaiPhong != null)
                {
                    MessageBoxResult message = MessageBox.Show("Xác nhận phòng đã hết bảo trì ?"
                  , "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (message == MessageBoxResult.Yes)
                    {
                        p.TrangThai = "Trống";
                        DataProvider.Ins.DB.SaveChanges();

                        LoadPhong();
                        RefreshStats();
                    }
                    else
                    {

                    }
                }

            });
        }

        public void LoadPhong()
        {
            var data = DataProvider.Ins.DB.Phongs
                                   .Include("LoaiPhong")
                                   .Where(p => p.IsEnable == true) 
                                   .ToList();

            DanhSachPhong = new ObservableCollection<Phong>(data);
        }

        public void RefreshStats()
        {
            OnPropertyChanged(nameof(TongPhong));
            OnPropertyChanged(nameof(SoPhongTrong));
            OnPropertyChanged(nameof(SoPhongDangDung));
            OnPropertyChanged(nameof(SoPhongKhac));
        }
    }
}
