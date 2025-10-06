using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace webapi_peso.Model
{
    public class ApplicantTechnicalVocational
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public string Id { get; set; }
        public string? AccountId { get; set; }
        public string CourseName { get; set; }
        public DateTime? DurationFrom { get; set; }
        public DateTime? DurationTo { get; set; }
        public string? TrainingInstitution { get; set; }
        public string? CertifcateReceived { get; set; }
        public long? DateLastUpdate { get; set; }
        public ApplicantTechnicalVocational()
        {
            DateLastUpdate = Helper.currentTimeMillis();
        }
    }
}
