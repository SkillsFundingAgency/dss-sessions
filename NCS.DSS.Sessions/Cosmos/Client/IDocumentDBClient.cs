using Microsoft.Azure.Documents.Client;

namespace NCS.DSS.Sessions.Cosmos.Client
{
    public interface IDocumentDBClient
    {
        DocumentClient CreateDocumentClient();
    }
}