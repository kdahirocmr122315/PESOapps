using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace webapi_peso.Model
{
    public class JobApplicantion
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public string Id { get; set; }
        public string Email { get; set; }
        public string Message { get; set; }
        public string JobPostId { get; set; }
        public string ApplicantId { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
