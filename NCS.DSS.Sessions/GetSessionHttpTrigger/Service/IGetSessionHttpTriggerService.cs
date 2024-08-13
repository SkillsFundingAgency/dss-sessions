using NCS.DSS.Sessions.Models;

namespace NCS.DSS.Sessions.GetSessionHttpTrigger.Service
{
    public interface IGetSessionHttpTriggerService
    {
        Task<List<Session>> GetSessionsAsync(Guid customerId);
    }
}