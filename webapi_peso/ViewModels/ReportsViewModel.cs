using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using webapi_peso.Model;

namespace webapi_peso.ViewModels
{
    public class ReportsViewModel
    {
        public List<JobVacancySolicited> VacanciesSolicited { get; set; }
        public List<JobApplicantsPlaced> PlacedApplicants { get; set; }
    }
}
