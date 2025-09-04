using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace webapi_peso.ViewModels
{
    public class VerificationCodeViewModel
    {
        public string UserAccountId { get; set; }
        public bool IsEmailNotFound { get; set; }
        public string Code { get; set; }
        public string Email { get; set; }
    }
}
