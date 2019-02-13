using System;
using System.Net;
using System.Threading.Tasks;
using NCS.DSS.Sessions.Cosmos.Provider;
using NCS.DSS.Sessions.Models;
using NCS.DSS.Sessions.ServiceBus;

namespace NCS.DSS.Sessions.PatchSessionHttpTrigger.Service
{
    public class PatchSessionHttpTriggerService : IPatchSessionHttpTriggerService
    {
        private readonly ISessionPatchService _sessionPatchService;
        private readonly IDocumentDBProvider _documentDbProvider;

        public PatchSessionHttpTriggerService(IDocumentDBProvider documentDbProvider, ISessionPatchService sessionPatchService)
        {
            _documentDbProvider = documentDbProvider;
            _sessionPatchService = sessionPatchService;
        }

        public Session PatchResource(string sessionJson, SessionPatch sessionPatch)
        {
            if (string.IsNullOrEmpty(sessionJson))
                return null;

            if (sessionPatch == null)
                return null;

            sessionPatch.SetDefaultValues();

            var updatedSession = _sessionPatchService.Patch(sessionJson, sessionPatch);

            return updatedSession;
        }

        public async Task<Session> UpdateCosmosAsync(Session session)
        {
            if (session == null)
                return null;

            var response = await _documentDbProvider.UpdateSessionAsync(session);

            var responseStatusCode = response?.StatusCode;

            return responseStatusCode == HttpStatusCode.OK ? session : null;
        }

        public async Task<string> GetSessionForCustomerAsync(Guid customerId, Guid sessionId)
        {
            var session = await _documentDbProvider.GetSessionForCustomerToUpdateAsync(customerId, sessionId);

            return session;
        }

        public async Task SendToServiceBusQueueAsync(Session session, Guid customerId, string reqUrl)
        {
            await ServiceBusClient.SendPatchMessageAsync(session, customerId, reqUrl);
        }
    }
}
