using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace webapi_peso.ViewModels
{
    public class JobRegisteredReferredViewModel
    {

        public string ApplicantName { get; set; }
        public string Skills { get; set; }
        public string Gender { get; set; }
        public string Age { get; set; }
        public string CivilStatus { get; set; }
        public string Education { get; set; }
        public int YearsOfWorkExperience { get; set; }
        public string EmploymentStatus { get; set; }
        public string ReferredAs { get; set; }
        public string ReferredTo { get; set; }
    }
}
