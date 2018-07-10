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
        Task<ResourceResponse<Document>> CreateSessionAsync(Session session);
    }
}