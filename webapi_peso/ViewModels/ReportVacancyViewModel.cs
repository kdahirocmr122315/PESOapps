using webapi_peso.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace webapi_peso.ViewModels
{
    public class ReportVacancyViewModel
    {
        public ReportVacancyViewModel()
        {
            ListOfJobs = new List<JobVacancySolicited>();
        }
        public int Count { get; set; }
        public string Company { get; set; }

        public int Month { get; set; }
        public List<JobVacancySolicited> ListOfJobs { get; set; }
    }
}
