using webapi_peso.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace webapi_peso.ViewModels
{
    public class 
        AppliedApplicantViewModel
    {
        public ApplicantInformation Applicant { get; set; }
        public string FilePath { get; set; }
        public DateTime DateApplied { get; set; }
        public bool IsInterviewed { get; set; }
        public bool IsHired { get; set; }
        //public List<string> Attachments { get; set; }
        public List<AttachementsViewModel> Attachments { get; set; } = new();
    }
}
