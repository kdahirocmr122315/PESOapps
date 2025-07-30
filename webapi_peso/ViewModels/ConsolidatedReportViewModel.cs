using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace webapi_peso.ViewModels
{
    public class ConsolidatedReportViewModel
    {
        public string ReportName { get; set; }
        public int RowNumber { get; set; }
        public string MunicipalityName { get; set; }
        public string MonthName { get; set; }
        public int Month { get; set; }
        public int NumberOfApplicants { get; set; }

        public int? Solicited { get; set; }
        public int? SolicitedFemale { get; set; }
        public int Registered { get; set; }
        public int RegisteredFemale { get; set; }
        public int Referred { get; set; }
        public int ReferredFemale { get; set; }
        public int Placed { get; set; }
        public int PlacedFemale { get; set; }
    }
}
