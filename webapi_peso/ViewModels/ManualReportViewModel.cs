using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using webapi_peso.Model;

namespace webapi_peso.ViewModels
{
    public class ManualReportViewModel
    {
        public PESOManualReport Report { get; set; }
        public List<RefCityMun> ListOfMunicipality { get; set; }
        public List<PESOManualReport> ListOfReports { get; set; }
    }
}
