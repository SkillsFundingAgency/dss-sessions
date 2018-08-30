using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using NCS.DSS.Sessions.Models;

namespace NCS.DSS.Sessions.Cosmos.Provider
{
    public interface IDocumentDBProvider
    {
        bool DoesCustomerResourceExist(Guid customerId);
        Task<bool> DoesCustomerHaveATerminationDate(Guid customerId);
        bool DoesInteractionResourceExist(Guid interactionId);
        Task<List<Session>> GetSessionsForCustomerAsync(Guid customerId);
        Task<Session> GetSessionForCustomerAsync(Guid customerId, Guid sessionId);
        Task<ResourceResponse<Document>> CreateSessionAsync(Session session);
        Task<ResourceResponse<Document>> UpdateSessionAsync(Session session);
    }
}