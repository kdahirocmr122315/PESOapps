using webapi_peso.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using webapi_peso.ViewModels;

namespace webapi_peso.ViewModels
{
    public class ReferralViewModel
    {
        public bool IsAlreadySubmittedToday { get; set; }
        public DateTime LastDateSubmitted { get; set; }
        public string Skill { get; set; }
        public ApplicantInformation ApplicantInformation { get; set; }
        public EmployerDetails ReferredTo { get; set; }

        public List<EmployerDetails> Employers { get; set; }
        public List<ApplicantDataViewModel> Applicants { get; set; } = new();
        public string Message { get; set; }
    }
}
