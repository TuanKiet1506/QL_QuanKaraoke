using QL_QuanKaraoke_BETA_1.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace QL_QuanKaraoke_BETA_1.ViewModel
{
    public class MenuPhongDangDungViewModel : BaseViewModel
    {
        public ICommand GoiMonCommand { get; set; }
        public ICommand DongCuaSoCommand { get; set; }
        public ICommand ChuyenPhongCommand { get; set; }
        public ICommand ThanhToanCommand { get; set; }
        public MenuPhongDangDungViewModel() {

            GoiMonCommand = new RelayCommand<Window>((p) => { return true; }, (p) =>
            {
                GoiMon goiMon = new GoiMon();
                goiMon.ShowDialog();

                p.Close(); 
            });
            DongCuaSoCommand = new RelayCommand<Window>((p) => { return true; }, (p) =>
            {
                p.Close();
            });
            ChuyenPhongCommand = new RelayCommand<Window>((p) => { return true; }, (p) =>
            {
                ChuyenPhong chuyenPhong = new ChuyenPhong();
                chuyenPhong.ShowDialog();
                p.Close();
            });
            ThanhToanCommand = new RelayCommand<Window>((p) => { return true; }, (p) =>
            {

                MessageBoxResult message = MessageBox.Show("Bạn có chắc chắn muốn thanh toán ?"
                   , "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (message == MessageBoxResult.Yes)
                {

                    ThanhToanWindow thanhToan = new ThanhToanWindow();
                    var thanhToanVM = thanhToan.DataContext as ThanhToanViewModel;

                    thanhToanVM.LoadThongTin();

                    ThanhToanWindow thanhToanWindow = new ThanhToanWindow();
                    thanhToanWindow.ShowDialog();
                    p.Close();
                }
                else
                {

                }

            }); 
        }
    }
}
