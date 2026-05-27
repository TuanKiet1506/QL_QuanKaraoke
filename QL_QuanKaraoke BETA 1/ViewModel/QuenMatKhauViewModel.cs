using QL_QuanKaraoke_BETA_1.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace QL_QuanKaraoke_BETA_1.ViewModel
{
    public class QuenMatKhauViewModel : BaseViewModel
    {
        public ICommand ExitCommandQuenMatKhau { get; set; }
        public ICommand XacNhanThongTinCommand {  get; set; }
        public int UserID {  get; set; }

        private string _userName;

        public string UserName
        {
            get { return _userName; }
            set { _userName = value; OnPropertyChanged(); }
        }
        private string _phoneNumber;

        public string PhoneNumber
        {
            get { return _phoneNumber; }
            set { _phoneNumber = value; OnPropertyChanged(); }
        }
        private string _name;

        public string Name
        {
            get { return _name; }
            set { _name = value; OnPropertyChanged(); }
        }

        public QuenMatKhauViewModel() 
        {

            ExitCommandQuenMatKhau = new RelayCommand<Window>((p) => {
                if (string.IsNullOrWhiteSpace(UserName) || string.IsNullOrWhiteSpace(PhoneNumber)
                    || string.IsNullOrWhiteSpace(Name))
                    return false;
                return true; 
            }, (p) => {
                p.Close(); 
            });
            XacNhanThongTinCommand = new RelayCommand<Window>((p) => { return true; }, (p) => {
                var user = DataProvider.Ins.DB.NhanViens.Where(sv => sv.TenDangNhap == UserName
                && sv.SoDienThoai == PhoneNumber && sv.HoTen == Name).FirstOrDefault();
                if (user != null)
                {
                    UserID = user.MaNhanVien; 
                    p.Close();
                    DoiMatKhauWindow doiMatKhauWindow = new DoiMatKhauWindow();
                    doiMatKhauWindow.ShowDialog();
                }
                else
                {
                    MessageBox.Show("Thông tin nhập sai !!!"); 
                }
            });

        }
    }
}
