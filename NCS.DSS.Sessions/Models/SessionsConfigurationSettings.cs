using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NCS.DSS.Sessions.Models
{
    public class SessionsConfigurationSettings
    {
        public string SessionConnectionString { get; set; }
        public string CollectionId { get; set; }
        public string CustomerCollectionId { get; set; }
        public string InteractionCollectionId { get; set; }
        public string DatabaseId { get; set; }
        public string CustomerDatabaseId { get; set; }
        public string InteractionDatabaseId { get; set; }
        public string QueueName { get; set; }
        public string ServiceBusConnectionString { get; set; }
    }
}
