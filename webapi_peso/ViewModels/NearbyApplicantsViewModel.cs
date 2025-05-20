using webapi_peso.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace webapi_peso.ViewModels
{
    public class NearbyApplicantsViewModel
    {
        public IEnumerable<ApplicantInformation> ApplicantInformationList { get; set; }
        public int MoreCount { get; set; }
        public int InterviewedCount { get; set; }
        public int HiredCount { get; set; }
        public int ScheduledCount { get; set; }
        public int TotalCount { get; set; } 
    }
}
