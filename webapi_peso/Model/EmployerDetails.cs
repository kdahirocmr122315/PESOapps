using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace webapi_peso.Model
{
    public class EmployerDetails
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public string? Id { get; set; }
        [Required(ErrorMessage = "*** Establishment name is required")]
        public string? EstablishmentName { get; set; }
        public string AcronymAbbreviation { get; set; }
        [Required(ErrorMessage = "*** Tax identification number is required")]
        public string? TIN { get; set; }
        [Required(ErrorMessage = "*** Employer type is required")]
        public string? EmployerType { get; set; }
        [Required(ErrorMessage = "*** Total workforce is required")]
        public string? WorkForce { get; set; }
        [Required(ErrorMessage = "*** Line of business is required")]
        public string? LineOfBusiness { get; set; }
        public string Address { get; set; }
        [Required(ErrorMessage = "*** Barangay is required")]
        public string Barangay { get; set; }
        [Required(ErrorMessage = "*** City/municipality is required")]
        public string CityMunicipality { get; set; }
        [Required(ErrorMessage = "*** Province is required")]
        public string Province { get; set; }
        [Required(ErrorMessage = "*** Region is required")]
        public string Region { get; set; }
        public string ContactPrependName { get; set; }
        [Required(ErrorMessage = "*** Contact person is required")]
        public string? ContactFullName { get; set; }
        [Required(ErrorMessage = "*** Position is required")]
        public string ContactPosition { get; set; }
        public string? ContactTelephoneNo { get; set; }
        [Required(ErrorMessage = "*** Mobile number is required")]
        public string? ContactMobileNo { get; set; }
        public string? ContactFaxNo { get; set; }
        [Required(ErrorMessage = "*** Email address is required")]
        public string ContactEmailAddress { get; set; }
        public int? NumberOfHiredApplicants { get; set; }
        public List<EmployerJobPost>? JobPosts { get; set; }
        public DateTime? DateCreated { get; set; }
        public ProjectConfig.ACCOUNT_STATUS? Status { get; set; }
        public string? PESORemarks { get; set; }
        public DateTime? LastUpdate { get; set; }
        public EmployerDetails()
        {
            DateCreated = DateTime.Now;
            JobPosts = new List<EmployerJobPost>();
        }
    }
}
