using System.ComponentModel.DataAnnotations;

namespace ASC.Web.Areas.Promotions.Models
{
    public class PromotionViewModel
    {
        public string RowKey { get; set; }
        public bool IsDeleted { get; set; }

        [Required]
        [Display(Name = "Type")]
        public string PartitionKey { get; set; }

        [Required]
        [Display(Name = "Header")]
        public string Header { get; set; }

        [Required]
        [Display(Name = "Content")]
        public string Content { get; set; }
    }
}
