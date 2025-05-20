using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace webapi_peso.Model
{
    public class AdminEmployerEmails
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public string Id { get; set; }
        public string Emails { get; set; }
        public DateTime? LastDateSent { get; set; }
        public DateTime DateAdded { get; set; }
        public int InActive { get; set; }
        public AdminEmployerEmails()
        {
            DateAdded = DateTime.Now;
        }
    }
}
