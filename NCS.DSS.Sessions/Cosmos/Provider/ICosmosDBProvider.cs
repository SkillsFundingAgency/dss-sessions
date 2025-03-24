using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using NCS.DSS.Sessions.Models;

namespace NCS.DSS.Sessions.Cosmos.Provider
{
    public interface ICosmosDBProvider
    {
        Task<bool> DoesCustomerResourceExist(Guid customerId);
        Task<bool> DoesInteractionResourceExistAndBelongToCustomer(Guid interactionId, Guid customerId);
        Task<bool> DoesCustomerHaveATerminationDate(Guid customerId);
        Task<List<Session>> GetSessionsForCustomerAsync(Guid customerId);
        Task<Session> GetSessionForCustomerAsync(Guid customerId, Guid sessionId);
        Task<string> GetSessionForCustomerToUpdateAsync(Guid customerId, Guid sessionId);
        Task<ItemResponse<Session>> CreateSessionAsync(Session session);
        Task<ItemResponse<Session>> UpdateSessionAsync(string sessionJson, Guid sessionId);
    }
}