using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace webapi_peso.Model
{
    public class UserAccount
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public string? Id { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public ProjectConfig.USER_TYPE? UserType { get; set; }
        public int? InActive { get; set; }
        public DateTime? LastLoggedIn { get; set; }
        public string? Name { get; set; }
        public string? GivenName { get; set; }
        public string? Surname { get; set; }
        public DateTime? DateCreated { get; set; }
        public UserAccount()
        {
            DateCreated = DateTime.Now;
        }
    }
}
