using System.ComponentModel.DataAnnotations;

namespace ASC.Web.Areas.Configuration.Models
{
    public class MasterDataValueViewModel
    {
        public string RowKey { get; set; }
        [Required]
        [Display(Name = "Partition Key")]
        public string PartitionKey { get; set; }
        public bool IsActive { get; set; }

        [Required]
        public string Name { get; set; }
    }
}
