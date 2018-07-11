using System;
using System.Net;
using System.Threading.Tasks;
using NCS.DSS.Sessions.Cosmos.Provider;
using NCS.DSS.Sessions.Models;

namespace NCS.DSS.Sessions.PatchSessionHttpTrigger.Service
{
    public class PatchSessionHttpTriggerService : IPatchSessionHttpTriggerService
    {
        public async Task<Session> UpdateAsync(Session session, SessionPatch sessionPatch)
        {
            if (session == null)
                return null;

            session.Patch(sessionPatch);

            var documentDbProvider = new DocumentDBProvider();
            var response = await documentDbProvider.UpdateSessionAsync(session);

            var responseStatusCode = response.StatusCode;

            return responseStatusCode == HttpStatusCode.OK ? session : null;
        }

        public async Task<Session> GetSessionForCustomerAsync(Guid customerId, Guid sessionId)
        {
            var documentDbProvider = new DocumentDBProvider();
            var session = await documentDbProvider.GetSessionForCustomerAsync(customerId, sessionId);

            return session;
        }
    }
}
