using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace webapi_peso.Model
{
    public class UpdateDetailsSession
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public string Id { get; set; }
        public string AccountId { get; set; }
        public DateTime DateExpiry { get; set; }
        public DateTime DateCreated { get; set; }
        public UpdateDetailsSession()
        {
            DateCreated = DateTime.Now;
        }
    }
}
