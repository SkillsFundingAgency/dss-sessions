using NCS.DSS.Sessions.Models;
using System;
using System.Threading.Tasks;

namespace NCS.DSS.Sessions.GetSessionByIdHttpTrigger.Service
{
    public interface IGetSessionByIdHttpTriggerService
    {
        Task<Session> GetSessionForCustomerAsync(Guid customerId, Guid sessionId);
    }
}