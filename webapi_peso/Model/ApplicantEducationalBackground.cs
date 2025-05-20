using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace webapi_peso.Model
{
    public class ApplicantEducationalBackground
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public string Id { get; set; }
        public string AccountId { get; set; }
        public string? LevelName { get; set; }
        public string? School { get; set; }
        public string? Course { get; set; }
        public int? YearGraduated { get; set; }
        public string? UndergradLevel { get; set; }
        public int? YearLastAttended { get; set; }
        public string? AwardsReceived { get; set; }
        public long DateLastUpdate { get; set; }
        public ApplicantEducationalBackground()
        {
            DateLastUpdate = Helper.currentTimeMillis();
        }
    }
}
