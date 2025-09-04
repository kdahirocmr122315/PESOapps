using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace webapi_peso.Model
{
    public class VerificationCode
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Code { get; set; }
        public DateTime DateExpiry { get; set; }
        public VerificationCode()
        {
            DateExpiry = DateTime.Now.AddMinutes(5);
        }
    }
}
