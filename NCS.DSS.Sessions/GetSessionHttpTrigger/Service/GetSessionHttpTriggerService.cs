using NCS.DSS.Sessions.Cosmos.Provider;
using NCS.DSS.Sessions.Models;

namespace NCS.DSS.Sessions.GetSessionHttpTrigger.Service
{
    public class GetSessionHttpTriggerService : IGetSessionHttpTriggerService
    {

        private readonly IDocumentDBProvider _documentDbProvider;

        public GetSessionHttpTriggerService(IDocumentDBProvider documentDbProvider)
        {
            _documentDbProvider = documentDbProvider;
        }

        public async Task<List<Session>> GetSessionsAsync(Guid customerId)
        {
            var sessions = await _documentDbProvider.GetSessionsForCustomerAsync(customerId);

            return sessions;
        }
    }
}
