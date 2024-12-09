using NCS.DSS.Sessions.Cosmos.Provider;
using NCS.DSS.Sessions.Models;
using NCS.DSS.Sessions.ServiceBus;
using System.Net;

namespace NCS.DSS.Sessions.PatchSessionHttpTrigger.Service
{
    public class PatchSessionHttpTriggerService : IPatchSessionHttpTriggerService
    {
        private readonly ISessionPatchService _sessionPatchService;
        private readonly ICosmosDBProvider _documentDbProvider;
        private readonly ISessionsServiceBusClient _sessionBusClient;

        public PatchSessionHttpTriggerService(ICosmosDBProvider documentDbProvider, 
            ISessionPatchService sessionPatchService,
            ISessionsServiceBusClient sessionBusClient)
        {
            _documentDbProvider = documentDbProvider;
            _sessionPatchService = sessionPatchService;
            _sessionBusClient = sessionBusClient;
        }

        public string PatchResource(string sessionJson, SessionPatch sessionPatch)
        {
            if (string.IsNullOrEmpty(sessionJson))
            {
                return null;
            }

            if (sessionPatch == null)
            {
                return null;
            }

            sessionPatch.SetDefaultValues();

            var updatedSession = _sessionPatchService.Patch(sessionJson, sessionPatch);

            return updatedSession;
        }

        public async Task<Session> UpdateCosmosAsync(string sessionJson, Guid sessionId)
        {
            if (string.IsNullOrEmpty(sessionJson))
            {
                return null;
            }

            var response = await _documentDbProvider.UpdateSessionAsync(sessionJson, sessionId);

            var responseStatusCode = response?.StatusCode;

            if (responseStatusCode == HttpStatusCode.OK)
            {
                return (dynamic)response.Resource;
            }
            else
            {
                return null;
            }
        }

        public async Task<string> GetSessionForCustomerAsync(Guid customerId, Guid sessionId)
        {
            var session = await _documentDbProvider.GetSessionForCustomerToUpdateAsync(customerId, sessionId);

            return session;
        }

        public async Task SendToServiceBusQueueAsync(Session session, Guid customerId, string reqUrl)
        {
            await _sessionBusClient.SendPatchMessageAsync(session, customerId, reqUrl);
        }
    }
}
