using webapi_peso.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace webapi_peso.ViewModels
{
    public class ScheduledApplicantsViewModel
    {
        public ApplicantInformation AppInformation { get; set; }
        public DateTime DateScheduled { get; set; }
    }
}
