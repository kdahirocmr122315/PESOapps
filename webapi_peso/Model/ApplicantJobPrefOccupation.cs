using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace webapi_peso.Model
{
    public class ApplicantJobPrefOccupation
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public string Id { get; set; }
        public string? AccountId { get; set; }
        public string OccupationName { get; set; }
        public int IsRemoved { get; set; }
        public long DateLastUpdate { get; set; }
        public ApplicantJobPrefOccupation()
        {
            DateLastUpdate = Helper.currentTimeMillis();
        }
    }
}
