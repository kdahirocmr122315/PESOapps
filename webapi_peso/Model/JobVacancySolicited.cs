using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace webapi_peso.Model
{
    public class JobVacancySolicited
    {
        public int Id { get; set; }
        public string CityMunCode { get; set; }
        public string JobTitle { get; set; }
        public string Company { get; set; }
        public int NumberOfVacancy { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public string MajorOccCode { get; set; }
        public int AgeFrom { get; set; }
        public int AgeTo { get; set; }
        public string Gender { get; set; }
        public string CivilStatus { get; set; }
        public string EducationalAttainment { get; set; }
        public string WorkExperience { get; set; }
        public double Salary { get; set; }

        public bool ReasonExpansion { get; set; }
        public bool ReasonReplaceMent { get; set; }
        public bool ReasonOthers { get; set; }
        public string IndustryCode { get; set; }

        public DateTime DateCreated { get; set; }
        public JobVacancySolicited()
        {
            DateCreated = DateTime.Now;
        }
    }
}
