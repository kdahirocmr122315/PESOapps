using webapi_peso.Model;

namespace webapi_peso.ViewModels
{
    public class AppliedJobsViewModel
    {
        //public JobApplicantion Applicant {  get; set; }
        public string Id { get; set; }
        public string JobPostId { get; set; }
        public string ApplicantId { get; set; }
        public DateTime DateCreated { get; set; }
        public EmployerJobPost Post { get; set; }

    }
}
