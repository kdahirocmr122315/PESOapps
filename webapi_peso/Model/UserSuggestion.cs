using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace webapi_peso.Model
{
    public class UserSuggestion
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public string Id { get; set; }
        public string UserEmail { get; set; }
        public string SuggestionMessage { get; set; }
        public DateTime DateCreated { get; set; }
        public int IsOk { get; set; }
        public UserSuggestion()
        {
            DateCreated = DateTime.Now;
        }
    }
}
