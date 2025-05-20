using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace webapi_peso.ViewModels
{
    public class Pagination<T>
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalAllItems { get; set; }
        public int TotalResultItems { get; set; }
        public List<T> Results { get; set; }
    }
}
