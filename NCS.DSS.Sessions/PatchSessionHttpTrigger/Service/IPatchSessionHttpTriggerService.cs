using NCS.DSS.Sessions.Models;

namespace NCS.DSS.Sessions.PatchSessionHttpTrigger.Service
{
    public interface IPatchSessionHttpTriggerService
    {
        string PatchResource(string sessionJson, SessionPatch sessionPatch);
        Task<Session> UpdateCosmosAsync(string sessionJson, Guid sessionId);
        Task<string> GetSessionForCustomerAsync(Guid customerId, Guid sessionId);
        Task SendToServiceBusQueueAsync(Session session, Guid customerId, string reqUrl);
    }
}