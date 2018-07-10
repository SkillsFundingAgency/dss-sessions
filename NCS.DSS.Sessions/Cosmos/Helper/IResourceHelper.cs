using System;

namespace NCS.DSS.Sessions.Cosmos.Helper
{
    public interface IResourceHelper
    {
        bool DoesCustomerExist(Guid customerId);
    }
}