using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace webapi_peso.Model
{
    public class ApplicantOtherSkills
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public string Id { get; set; }
        public string AccountId { get; set; }
        public string SkillName { get; set; }
        public long DateLastUpdate { get; set; }
        public ApplicantOtherSkills()
        {
            DateLastUpdate = Helper.currentTimeMillis();
        }
    }
}
