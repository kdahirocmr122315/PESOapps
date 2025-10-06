using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace webapi_peso.Model
{
    public class ApplicantLanguageDialectProf
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public string Id { get; set; }
        public string? AccountId { get; set; }
        public string LanguageName { get; set; }
        public int Read { get; set; }
        public int Write { get; set; }
        public int Speak { get; set; }
        public int Understand { get; set; }
        public long DateLastUpdate { get; set; }
        public ApplicantLanguageDialectProf()
        {
            DateLastUpdate = Helper.currentTimeMillis();
        }
    }
}
