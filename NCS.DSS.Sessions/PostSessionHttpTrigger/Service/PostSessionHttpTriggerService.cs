using System;
using System.Net;
using System.Threading.Tasks;
using NCS.DSS.Sessions.Cosmos.Provider;
using NCS.DSS.Sessions.Models;

namespace NCS.DSS.Sessions.PostSessionHttpTrigger.Function
{
    public class PostSessionHttpTriggerService : IPostSessionHttpTriggerService
    {
        public async Task<Session> CreateAsync(Session session)
        {
            if (session == null)
                return null;

            var sessionId = Guid.NewGuid();
            session.SessionId = sessionId;

            var documentDbProvider = new DocumentDBProvider();

            var response = await documentDbProvider.CreateSessionAsync(session);

            return response.StatusCode == HttpStatusCode.Created ? (dynamic)response.Resource : null;
        }
    }
}
