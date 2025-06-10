using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace webapi_peso.Model
{
    public class ApplicantInformation
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public string? Id { get; set; }
        public string? JobFairReferenceCode { get; set; }
        public string? AccountId { get; set; }
        [Required(ErrorMessage = "* Email is required.")]
        [EmailAddress(ErrorMessage = "* Invalid email format.")]
        public string? Email { get; set; }
        [Required]
        public string? SurName { get; set; }
        [Required]
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? Suffix { get; set; }
        [Required(ErrorMessage = "* Date of birth is required.")]
        public long DateOfBirth { get; set; }
        public string? PlaceOfBirth { get; set; }
        [Required(ErrorMessage = "* Gender is required.")]
        public string? Gender { get; set; }
        public string? PresentHouseNoOrStreetVillage { get; set; }
        [Required(ErrorMessage = "* Please specify your barangay.")]
        public string? PresentBarangay { get; set; }
        [Required(ErrorMessage = "* Please specify your municipality or city address.")]
        public string? PresentMunicipalityCity { get; set; }
        [Required(ErrorMessage = "* Please specify your province.")]
        public string? PresentProvince { get; set; }
        public string? PresentRegion { get; set; }
        public string? ProvincialHouseNoOrStreetVillage { get; set; }
        [Required(ErrorMessage = "* Please specify your barangay.")]
        public string? ProvincialBarangay { get; set; }
        [Required(ErrorMessage = "* Please specify your municipality or city address.")]
        public string? ProvincialMunicipalityCity { get; set; }
        [Required(ErrorMessage = "* Please specify your province.")]
        public string? ProvincialProvince { get; set; }
        public string? ProvincialRegion { get; set; }
        public string? Religion { get; set; }
        public string? CivilStatus { get; set; }
        public string? TIN { get; set; }
        public string? GSISORSSS { get; set; }
        public string? PAGIBIG { get; set; }
        public string? PHILHEALTH { get; set; }
        public string? Height { get; set; }
        public string? LandlineNumber { get; set; }
        [Required(ErrorMessage = "* Cellphone number is required.")]
        public string? CellphoneNumber { get; set; }
        public string? Disability { get; set; }
        public string? EmpStatus { get; set; }
        public string? EmpStatusChild { get; set; }
        public int ActivelyLookingForWork { get; set; }
        public string? WillingToWorkNow { get; set; }
        public string? HowLongLookingForWork { get; set; }
        public string? PassportNumber { get; set; }
        public long? PassportExpiryDate { get; set; }
        public long DateLastUpdate { get; set; }
        public ApplicantInformation()
        {
            DateLastUpdate = Helper.currentTimeMillis();
        }
    }
}
