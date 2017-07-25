using ASC.Models.BaseTypes;
using System;

namespace ASC.Models.Models
{
    public class OnlineUser : BaseEntity
    {
        public OnlineUser() { }
        public OnlineUser(string name)
        {
            this.RowKey = Guid.NewGuid().ToString();
            this.PartitionKey = name;
        }
    }
}
