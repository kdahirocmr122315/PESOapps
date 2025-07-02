namespace webapi_peso.ViewModels
{
    public class ApplicantsPerJobPostModel
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public List<AppliedApplicantViewModel> Applicants { get; set; } = new List<AppliedApplicantViewModel>();
    }
}
