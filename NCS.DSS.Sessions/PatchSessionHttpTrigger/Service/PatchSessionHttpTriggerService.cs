using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NCS.DSS.Sessions.Cosmos.Provider;
using NCS.DSS.Sessions.Models;
using NCS.DSS.Sessions.ServiceBus;

namespace NCS.DSS.Sessions.PatchSessionHttpTrigger.Service
{
    public class PatchSessionHttpTriggerService : IPatchSessionHttpTriggerService
    {
        private readonly ISessionPatchService _sessionPatchService;
        private readonly IDocumentDBProvider _documentDbProvider;
        private ILogger _logger;

        public PatchSessionHttpTriggerService(IDocumentDBProvider documentDbProvider, ISessionPatchService sessionPatchService,ILogger logger)
        {
            _documentDbProvider = documentDbProvider;
            _sessionPatchService = sessionPatchService;
            _logger = logger;
        }

        public string PatchResource(string sessionJson, SessionPatch sessionPatch)
        {
            if (string.IsNullOrEmpty(sessionJson))
            {
                _logger.LogInformation("PatchSessionHttpTriggerService sessionJson is null");
                return null;
            }

            if (sessionPatch == null)
            {
                _logger.LogInformation("PatchSessionHttpTriggerService sessionPatch is null");
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
                _logger.LogInformation("UpdateCosmosAsync sessionPatch is null");
                return null;
            }

            var response = await _documentDbProvider.UpdateSessionAsync(sessionJson, sessionId);

            var responseStatusCode = response?.StatusCode;

            if (responseStatusCode == HttpStatusCode.OK)
            {
                _logger.LogInformation($"PatchSessionHttpTriggerService UpdateCosmosAsync HttpStatusCode.OK");
                return (dynamic)response.Resource;
            }
            else
            {
                _logger.LogInformation($"PatchSessionHttpTriggerService UpdateCosmosAsync returning null");

                return null;
            }
        }

        public async Task<string> GetSessionForCustomerAsync(Guid customerId, Guid sessionId)
        {
            var session = await _documentDbProvider.GetSessionForCustomerToUpdateAsync(customerId, sessionId);
            _logger.LogInformation($"PatchSessionHttpTriggerService GetSessionForCustomerAsync customerid {customerId} sessionId {sessionId}");

            return session;
        }

        public async Task SendToServiceBusQueueAsync(Session session, Guid customerId, string reqUrl)
        {
            await ServiceBusClient.SendPatchMessageAsync(session, customerId, reqUrl);
        }
    }
}
