using System;
using System.Threading.Tasks;
using NCS.DSS.Sessions.Models;

namespace NCS.DSS.Sessions.PatchSessionHttpTrigger.Service
{
    public interface IPatchSessionHttpTriggerService
    {
        Session PatchResource(string sessionJson, SessionPatch sessionPatch);
        Task<Session> UpdateCosmosAsync(Session session);
        Task<string> GetSessionForCustomerAsync(Guid customerId, Guid sessionId);
        Task SendToServiceBusQueueAsync(Session session, Guid customerId, string reqUrl);
    }
}