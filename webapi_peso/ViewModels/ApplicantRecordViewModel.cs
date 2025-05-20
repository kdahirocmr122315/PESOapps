using webapi_peso.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace webapi_peso.ViewModels
{
    public class ApplicantRecordViewModel
    {
        public ApplicantAccount ApplicantAccount { get; set; }
        public List<ApplicantOtherSkills> ApplicantOtherSkills { get; set; }
    }
}
