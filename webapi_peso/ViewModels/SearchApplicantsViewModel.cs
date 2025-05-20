using webapi_peso.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace webapi_peso.ViewModels
{
    public class SearchApplicantsViewModel
    {
        public string EmployerId { get; set; }
        public string TxtSearch { get; set; }
        public List<string> Skills { get; set; }
        public IEnumerable<ApplicantInformation> ListOfAppInformation { get; set; }
        public string RegionCode { get; set; }
        public string RegionDesc { get; set; }
        public string ProvinceCode { get; set; }
        public string ProvinceDesc { get; set; }
        public string CityCode { get; set; }
        public string CityDesc { get; set; }
        public string BarangayCode { get; set; }
        public string BarangayDesc { get; set; }
        public DateTime? DateFrom { get; set; } = DateTime.Now.AddMonths(-1);
        public DateTime? DateTo { get; set; }
        public DateTime SelectedExportDate { get; set; }

        public bool IsExport { get; set; }


        // for virtualize
        public int StartIndex { get; set; }
        public int Count { get; set; }
        public string Gender { get; set; }
    }
}
