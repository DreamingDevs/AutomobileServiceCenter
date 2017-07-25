using ASC.Utilities.ValidationAttributes;
using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;

namespace ASC.Web.Areas.ServiceRequests.Models
{
    public class NewServiceRequestViewModel
    {
        [Required(ErrorMessage = "Error_VehicleName_Message")]
        [Display(Name ="Vehicle Name")]
        public string VehicleName { get; set; }
        [Required]
        [Display(Name = "Vehicle Type")]
        public string VehicleType { get; set; }
        [Required]
        [Display(Name = "Requested Services")]
        public string RequestedServices { get; set; }

        [Required]
        [FutureDate(90)]
        [Remote(action: "CheckDenialService", controller: "ServiceRequest", areaName: "ServiceRequests")]
        [Display(Name = "Requested Date")]
        public DateTime? RequestedDate { get; set; }
    }
}
