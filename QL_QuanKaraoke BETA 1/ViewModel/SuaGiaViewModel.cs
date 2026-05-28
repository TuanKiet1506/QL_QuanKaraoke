using QL_QuanKaraoke_BETA_1.Data;
using QL_QuanKaraoke_BETA_1.Model;
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
    public class SuaGiaViewModel : BaseViewModel
    {
        private string _giaMoi;

        public string GiaMoi
        {
            get { return _giaMoi; }
            set { _giaMoi = value;OnPropertyChanged(); }
        }
        private string _giaCu;

        public string GiaCu
        {
            get { return _giaCu; }
            set { _giaCu = value; OnPropertyChanged(); }
        }

        public void LoadGia()
        {
            QL_BangGiaUC ql = new QL_BangGiaUC();
            var qlVM = ql.DataContext as QL_BangGiaViewModel;

            GiaCu = qlVM.SelectedBangGia.Gia.ToString(); 
            
        }
        public bool KiemTraGiaTien(string giaMoi)
        {
            // 1. Kiểm tra rỗng
            if (string.IsNullOrWhiteSpace(giaMoi))
            {
                MessageBox.Show("Giá mới không được để trống!");
                return false;
            }

            // 2. Xóa khoảng trắng đầu cuối
            giaMoi = giaMoi.Trim();

            // 3. Kiểm tra có phải số hay không
            if (!decimal.TryParse(giaMoi, out decimal giaTienMoi))
            {
                MessageBox.Show("Giá tiền phải là số hợp lệ!");
                return false;
            }

            // 4. Không được <= 0
            if (giaTienMoi <= 0)
            {
                MessageBox.Show("Giá tiền phải lớn hơn 0!");
                return false;
            }

            // 5. Không được quá nhỏ
            if (giaTienMoi < 1000)
            {
                MessageBox.Show("Giá tiền quá nhỏ!");
                return false;
            }

            // 6. Không được quá lớn
            if (giaTienMoi > 100000000)
            {
                MessageBox.Show("Giá tiền vượt quá giới hạn cho phép!");
                return false;
            }


            return true;
        }
        public ICommand ExitCommand { get; set; }
        public ICommand XacNhanSuaCommand { get; set; }
        public SuaGiaViewModel() {
        

            ExitCommand = new RelayCommand<Window>((p) => { return true; }, (p) => {
                p.Close(); 
            });

            XacNhanSuaCommand = new RelayCommand<Window>((p) => { return true; }, (p) => {

                if (!KiemTraGiaTien(GiaMoi))
                {
                    return;
                }

                QL_BangGiaUC ql = new QL_BangGiaUC();
                var qlVM = ql.DataContext as QL_BangGiaViewModel;

                LoginWindow lg = new LoginWindow();
                var lgVM = lg.DataContext as LoginViewModel;

                var bg = DataProvider.Ins.DB.BangGias
                .Where(x => x.MaBangGia == qlVM.SelectedBangGia.MaBangGia)
                .FirstOrDefault();

                bg.Gia = decimal.Parse(GiaMoi); 

                DataProvider.Ins.DB.SaveChanges();

                SystemLog system = new SystemLog
                {
                    HanhDong = "SUA_GIA_PHONG",
                    ThoiGian = DateTime.Now,
                    MoTa = $"Nhân viên mã {lgVM.UserId} đã sửa giá phòng mã {bg.MaBangGia} lúc {DateTime.Now}"
                };

                DataProvider.Ins.DB.SystemLogs.Add(system);
                DataProvider.Ins.DB.SaveChanges();

                MessageBox.Show("Đã sửa giá phòng thành công !"); 

                p.Close(); 
            }); 
        
        }
    }
}
