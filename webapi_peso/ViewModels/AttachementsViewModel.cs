using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace webapi_peso.ViewModels
{
    public class AttachementsViewModel
    {
        public string Id { get; set; }
        public string FileName { get; set; }
        public string FileSize { get; set; }
        public string FolderName { get; set; }
        public int IsAlreadyUploaded { get; set; }
    }
}
