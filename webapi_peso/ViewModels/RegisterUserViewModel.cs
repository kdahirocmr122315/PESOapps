using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace webapi_peso.ViewModels
{
    public class RegisterUserViewModel
    {
        public string Password { get; set; }
        public string RoleName { get; set; }
    }
}
