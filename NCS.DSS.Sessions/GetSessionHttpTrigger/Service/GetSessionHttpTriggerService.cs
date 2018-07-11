using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NCS.DSS.Sessions.Cosmos.Provider;
using NCS.DSS.Sessions.Models;

namespace NCS.DSS.Sessions.GetSessionHttpTrigger.Service
{
    public class GetSessionHttpTriggerService : IGetSessionHttpTriggerService
    {
        public async Task<List<Session>> GetSessionsAsync(Guid customerId)
        {
            var documentDbProvider = new DocumentDBProvider();
            var sessions = await documentDbProvider.GetSessionsForCustomerAsync(customerId);

            return sessions;
        }
    }
}
