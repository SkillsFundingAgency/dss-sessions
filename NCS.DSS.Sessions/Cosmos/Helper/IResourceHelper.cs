using System;
using System.Threading.Tasks;

namespace NCS.DSS.Sessions.Cosmos.Helper
{
    public interface IResourceHelper
    {
        Task<bool> DoesCustomerExist(Guid customerId);
        Task<bool> IsCustomerReadOnly(Guid customerId);
        Task<bool> DoesInteractionExist(Guid interactionId);
    }
}