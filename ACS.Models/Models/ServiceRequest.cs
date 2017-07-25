using ASC.Models.BaseTypes;
using System;

namespace ASC.Models.Models
{
    public class ServiceRequest : BaseEntity, IAuditTracker
    {
        public ServiceRequest() { }

        public ServiceRequest(string email)
        {
            this.RowKey = Guid.NewGuid().ToString();
            this.PartitionKey = email;
        }

        public string VehicleName { get; set; }
        public string VehicleType { get; set; }
        public string Status { get; set; }
        public string RequestedServices { get; set; }
        public DateTime? RequestedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string ServiceEngineer { get; set; }
    }
}
