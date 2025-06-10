using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace webapi_peso.Model
{
    public class JobApplicantsPlaced
    {
        public int Id { get; set; }
        public string CityMunCode { get; set; }
        public string ApplicantName { get; set; }
        public string JobTitle { get; set; }
        public string Gender { get; set; }
        public string Company { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public DateTime DateHired { get; set; }
        public DateTime DateCreated { get; set; }
        public JobApplicantsPlaced()
        {
            DateCreated = DateTime.Now;
        }
    }
}
