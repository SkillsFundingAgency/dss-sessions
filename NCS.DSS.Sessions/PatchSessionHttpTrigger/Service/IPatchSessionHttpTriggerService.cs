using System;
using System.Threading.Tasks;
using NCS.DSS.Sessions.Models;

namespace NCS.DSS.Sessions.PatchSessionHttpTrigger.Service
{
    public interface IPatchSessionHttpTriggerService
    {
        Task<Session> UpdateAsync(Session session, SessionPatch sessionPatch);
        Task<Session> GetSessionForCustomerAsync(Guid customerId, Guid sessionId);
        Task SendToServiceBusQueueAsync(Session session, Guid customerId, string reqUrl);
    }
}