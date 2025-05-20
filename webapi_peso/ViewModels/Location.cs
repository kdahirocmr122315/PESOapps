using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace webapi_peso.ViewModels
{
    public class Location
    {
        public List<RefBrgy> RefBrgys { get; set; }
        public List<RefCityMun> RefCityMuns { get; set; }
    }
    public class RefBrgy
    {
        public int id { get; set; }
        public string brgyCode { get; set; }
        public string brgyDesc { get; set; }
        public string regCode { get; set; }
        public string provCode { get; set; }
        public string citymunCode { get; set; }

    }
    public class RefCityMun
    {
        public int id { get; set; }
        public string psgcCode { get; set; }
        public string citymunDesc { get; set; }
        public string regDesc { get; set; }
        public string provCode { get; set; }
        public string citymunCode { get; set; }
    }
    public class RefProvince
    {
        public int id { get; set; }
        public string psgcCode { get; set; }
        public string provDesc { get; set; }
        public string regCode { get; set; }
        public string provCode { get; set; }
    }
    public class RefRegion
    {
        public int id { get; set; }
        public string psgcCode { get; set; }
        public string regDesc { get; set; }
        public string regCode { get; set; }
    }
}
