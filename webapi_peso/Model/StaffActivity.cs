using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace webapi_peso.Model
{
    public class StaffActivity
    {
        public int Id { get; set; }
        public string LargeContent { get; set; }
        public DateTime DateOfActivity { get; set; }
        public DateTime DateCreated { get; set; }
        public StaffActivity()
        {
            DateCreated = DateTime.Now;
        }
    }
}
