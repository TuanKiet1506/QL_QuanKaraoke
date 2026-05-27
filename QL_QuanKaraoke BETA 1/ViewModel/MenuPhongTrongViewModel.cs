using QL_QuanKaraoke_BETA_1.Data;
using QL_QuanKaraoke_BETA_1.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace QL_QuanKaraoke_BETA_1.ViewModel
{
    public class MenuPhongTrongViewModel : BaseViewModel
    {
        public bool isMoPhong {  get; set; }
        public bool isExit {  get; set; }
        public bool isBaoTri { get; set; }
        public int maHD {  get; set; }
        public ICommand MoPhongCommand { get; set; }
        public ICommand BaoTriPhongCommand { get; set; }
        public ICommand CloseWindowCommand { get; set; }
        public MenuPhongTrongViewModel()
        {
            // Mở phòng chỉ mở đối với phòng đang trống !
            MoPhongCommand = new RelayCommand<Window>((p) => { return true; }, (p) =>
            {
                isMoPhong = true;
                isBaoTri = false;
                isExit = false;
                p.Close();

            });
            BaoTriPhongCommand = new RelayCommand<Window>((p) => { return true; }, (p) =>
            {
                MessageBoxResult message = MessageBox.Show("Xác nhận phòng sẽ được bảo trì ?", 
                    "Xác nhận", 
                    MessageBoxButton.YesNo, 
                    MessageBoxImage.Question);

                if (message == MessageBoxResult.Yes)
                {
                    isBaoTri = true;
                    isMoPhong = false;
                }
                else
                {
                    isBaoTri = false;
                    isMoPhong = false;
                }

                p.Close();

            });
            CloseWindowCommand = new RelayCommand<Window>((p) => { return true; }, (p) => {
                isExit = true; 
                p.Close();
            });
        }
    }
}
