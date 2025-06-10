using NCS.DSS.Sessions.Cosmos.Provider;
using NCS.DSS.Sessions.Models;
using NCS.DSS.Sessions.ServiceBus;
using System.Net;

namespace NCS.DSS.Sessions.PostSessionHttpTrigger.Service
{
    public class PostSessionHttpTriggerService : IPostSessionHttpTriggerService
    {

        private readonly ICosmosDBProvider _cosmosDbProvider;
        private readonly ISessionsServiceBusClient _sessionBusClient;
        public PostSessionHttpTriggerService(ICosmosDBProvider cosmosDbProvider, ISessionsServiceBusClient sessionBusClient)
        {
            _cosmosDbProvider = cosmosDbProvider;
            _sessionBusClient = sessionBusClient;
        }

        public async Task<Session> CreateAsync(Session session)
        {
            if (session == null)
                return null;

            session.SetDefaultValues();

            var response = await _cosmosDbProvider.CreateSessionAsync(session);

            return response.StatusCode == HttpStatusCode.Created ? (dynamic)response.Resource : null;
        }

        public async Task SendToServiceBusQueueAsync(Session session, string reqUrl)
        {
            await _sessionBusClient.SendPostMessageAsync(session, reqUrl);
        }
    }
}
