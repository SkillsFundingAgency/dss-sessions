using NCS.DSS.Sessions.Cosmos.Provider;
using NCS.DSS.Sessions.Models;

namespace NCS.DSS.Sessions.GetSessionByIdHttpTrigger.Service
{
    public class GetSessionByIdHttpTriggerService : IGetSessionByIdHttpTriggerService
    {

        private readonly IDocumentDBProvider _documentDbProvider;

        public GetSessionByIdHttpTriggerService(IDocumentDBProvider documentDbProvider)
        {
            _documentDbProvider = documentDbProvider;
        }

        public async Task<Session> GetSessionForCustomerAsync(Guid customerId, Guid sessionId)
        {
            var session = await _documentDbProvider.GetSessionForCustomerAsync(customerId, sessionId);

            return session;
        }
    }
}
