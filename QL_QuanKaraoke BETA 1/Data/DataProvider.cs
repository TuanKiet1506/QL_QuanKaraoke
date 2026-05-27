using QL_QuanKaraoke_BETA_1.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QL_QuanKaraoke_BETA_1.Data
{
    public class DataProvider
    {
        private static DataProvider _ins;
        public static DataProvider Ins
        {
            get
            {
                if (_ins == null)
                {
                    _ins = new DataProvider();
                }
                return _ins;
            }
            set
            {
                _ins = value;
            }
        }
        public QL_QuanKaraokeEntities DB { get; set; }
        private DataProvider()
        {
            DB = new QL_QuanKaraokeEntities();
        }

    }
}
