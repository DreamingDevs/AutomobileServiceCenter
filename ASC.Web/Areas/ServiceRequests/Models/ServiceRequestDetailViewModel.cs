using ASC.Models.Models;
using System.Collections.Generic;

namespace ASC.Web.Areas.ServiceRequests.Models
{
    public class ServiceRequestDetailViewModel
    {
        public UpdateServiceRequestViewModel ServiceRequest { get; set; }
        public List<ServiceRequest> ServiceRequestAudit { get; set; }
    }
}
