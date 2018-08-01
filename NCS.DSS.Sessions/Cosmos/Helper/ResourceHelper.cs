using System;
using NCS.DSS.Sessions.Cosmos.Provider;

namespace NCS.DSS.Sessions.Cosmos.Helper
{
    public class ResourceHelper : IResourceHelper
    {
        public bool DoesCustomerExist(Guid customerId)
        {
            var documentDbProvider = new DocumentDBProvider();
            var doesCustomerExist = documentDbProvider.DoesCustomerResourceExist(customerId);

            return doesCustomerExist;
        }

        public bool DoesInteractionExist(Guid interactionId)
        {
            var documentDbProvider = new DocumentDBProvider();
            var doesInteractionExist = documentDbProvider.DoesInteractionResourceExist(interactionId);

            return doesInteractionExist;
        }
    }
}
