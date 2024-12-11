using NCS.DSS.Sessions.Cosmos.Provider;
using NCS.DSS.Sessions.Models;

namespace NCS.DSS.Sessions.GetSessionHttpTrigger.Service
{
    public class GetSessionHttpTriggerService : IGetSessionHttpTriggerService
    {

        private readonly ICosmosDBProvider _cosmosDbProvider;

        public GetSessionHttpTriggerService(ICosmosDBProvider documentDbProvider)
        {
            _cosmosDbProvider = documentDbProvider;
        }

        public async Task<List<Session>> GetSessionsAsync(Guid customerId)
        {
            var sessions = await _cosmosDbProvider.GetSessionsForCustomerAsync(customerId);

            return sessions;
        }
    }
}
