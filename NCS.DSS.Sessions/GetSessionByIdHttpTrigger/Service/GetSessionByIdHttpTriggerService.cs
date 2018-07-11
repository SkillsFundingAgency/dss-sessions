using System;
using System.Threading.Tasks;
using NCS.DSS.Sessions.Cosmos.Provider;
using NCS.DSS.Sessions.Models;

namespace NCS.DSS.Sessions.GetSessionByIdHttpTrigger.Service
{
    public class GetSessionByIdHttpTriggerService : IGetSessionByIdHttpTriggerService
    {
        public async Task<Session> GetSessionForCustomerAsync(Guid customerId, Guid sessionId)
        {
            var documentDbProvider = new DocumentDBProvider();
            var session = await documentDbProvider.GetSessionForCustomerAsync(customerId, sessionId);

            return session;
        }
    }
}
