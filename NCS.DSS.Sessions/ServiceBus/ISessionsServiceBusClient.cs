using NCS.DSS.Sessions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NCS.DSS.Sessions.ServiceBus
{
    public interface ISessionsServiceBusClient
    {
        Task SendPostMessageAsync(Session session, string reqUrl);
        Task SendPatchMessageAsync(Session session, Guid customerId, string reqUrl);
    }
}
