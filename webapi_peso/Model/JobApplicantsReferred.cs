using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace webapi_peso.Model
{
    public class JobApplicantsReferred
    {
        public int Id { get; set; }
        public string ApplicantAccountId { get; set; }
        public string EmployerId { get; set; }
        public string ReferredBy { get; set; }
        public string ApplicantCityMunCode { get; set; }
        public string Gender { get; set; }
        public string JobTitle { get; set; }
        public DateTime DateReferred { get; set; }
        public JobApplicantsReferred()
        {
            DateReferred = DateTime.Now;
        }
    }
}
