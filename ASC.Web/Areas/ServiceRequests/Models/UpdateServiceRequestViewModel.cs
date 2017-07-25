using System;
using System.ComponentModel.DataAnnotations;

namespace ASC.Web.Areas.ServiceRequests.Models
{
    public class UpdateServiceRequestViewModel : NewServiceRequestViewModel
    {
        public string RowKey { get; set; }
        public string PartitionKey { get; set; }
        [Required]
        [Display(Name = "Service Engineer")]
        public string ServiceEngineer { get; set; }
        [Required]
        [Display(Name = "Status")]
        public string Status { get; set; }
    }
}
