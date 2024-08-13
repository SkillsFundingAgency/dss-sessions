using NCS.DSS.Sessions.Models;

namespace NCS.DSS.Sessions.GetSessionByIdHttpTrigger.Service
{
    public interface IGetSessionByIdHttpTriggerService
    {
        Task<Session> GetSessionForCustomerAsync(Guid customerId, Guid sessionId);
    }
}