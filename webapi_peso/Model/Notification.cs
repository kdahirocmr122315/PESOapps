using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace webapi_peso.Model
{
    public class Notification
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int IsRead { get; set; }
        public long DateNotified { get; set; }
        public string ActionTable { get; set; }
        public string ActionTableId { get; set; }
    }
}
