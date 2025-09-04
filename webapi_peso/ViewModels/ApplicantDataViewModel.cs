using webapi_peso.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace webapi_peso.ViewModels
{
    public class ApplicantDataViewModel
    {
        public ApplicantAccount ApplicantAccount { get; set; }
        public List<ApplicantEducationalBackground> ApplicantEducationalBackground { get; set; }
        public List<ApplicantEligibility> ApplicantEligibility { get; set; }
        public ApplicantExpectedSalary ApplicantExpectedSalary { get; set; }
        public ApplicantInformation ApplicantInformation { get; set; } = new();
        public List<ApplicantJobPrefOccupation> ApplicantJobPrefOccupation { get; set; } = new();
        public List<ApplicantJobPrefWorkLocation> ApplicantJobPrefWorkLocation { get; set; }
        public List<ApplicantLanguageDialectProf> ApplicantLanguageDialectProf { get; set; }
        public List<ApplicantOtherSkills> ApplicantOtherSkills { get; set; }
        public List<ApplicantProfessionalLicense> ApplicantProfessionalLicense { get; set; }
        public List<ApplicantTechnicalVocational> ApplicantTechnicalVocational { get; set; }
        public List<ApplicantWorkExperience> ApplicantWorkExperience { get; set; }

    }
}
