using System;
using System.Threading.Tasks;
using NCS.DSS.Sessions.Cosmos.Provider;

namespace NCS.DSS.Sessions.Cosmos.Helper
{
    public class ResourceHelper : IResourceHelper
    {
        public async Task<bool> DoesCustomerExist(Guid customerId)
        {
            var documentDbProvider = new DocumentDBProvider();
            var doesCustomerExist = await documentDbProvider.DoesCustomerResourceExist(customerId);

            return doesCustomerExist;
        }

        public async Task<bool> IsCustomerReadOnly(Guid customerId)
        {
            var documentDbProvider = new DocumentDBProvider();
            var isCustomerReadOnly = await documentDbProvider.DoesCustomerHaveATerminationDate(customerId);

            return isCustomerReadOnly;
        }

        public bool DoesInteractionResourceExistAndBelongToCustomer(Guid interactionId, Guid customerId)
        {
            var documentDbProvider = new DocumentDBProvider();
            var doesInteractionExist = documentDbProvider.DoesInteractionResourceExistAndBelongToCustomer(interactionId, customerId);

            return doesInteractionExist;
        }
    }
}
