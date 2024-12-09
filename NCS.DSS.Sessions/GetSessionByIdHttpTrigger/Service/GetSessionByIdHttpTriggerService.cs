using NCS.DSS.Sessions.Cosmos.Provider;
using NCS.DSS.Sessions.Models;

namespace NCS.DSS.Sessions.GetSessionByIdHttpTrigger.Service
{
    public class GetSessionByIdHttpTriggerService : IGetSessionByIdHttpTriggerService
    {

        private readonly ICosmosDBProvider _cosmosDbProvider;

        public GetSessionByIdHttpTriggerService(ICosmosDBProvider cosmosDbProvider)
        {
            _cosmosDbProvider = cosmosDbProvider;
        }

        public async Task<Session> GetSessionForCustomerAsync(Guid customerId, Guid sessionId)
        {
            var session = await _cosmosDbProvider.GetSessionForCustomerAsync(customerId, sessionId);

            return session;
        }
    }
}
