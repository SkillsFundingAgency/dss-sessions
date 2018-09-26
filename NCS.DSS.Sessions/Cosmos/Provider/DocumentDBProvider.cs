using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using NCS.DSS.Sessions.Cosmos.Client;
using NCS.DSS.Sessions.Cosmos.Helper;
using NCS.DSS.Sessions.Models;

namespace NCS.DSS.Sessions.Cosmos.Provider
{
    public class DocumentDBProvider : IDocumentDBProvider
    {

        public async Task<bool> DoesCustomerResourceExist(Guid customerId)
        {
            var documentUri = DocumentDBHelper.CreateCustomerDocumentUri(customerId);

            var client = DocumentDBClient.CreateDocumentClient();

            if (client == null)
                return false;

            try
            {
                var response = await client.ReadDocumentAsync(documentUri);
                if (response.Resource != null)
                    return true;
            }
            catch (DocumentClientException)
            {
                return false;
            }

            return false;
        }

        public bool DoesInteractionResourceExistAndBelongToCustomer(Guid interactionId, Guid customerId)
        {
            var collectionUri = DocumentDBHelper.CreateInteractionDocumentCollectionUri();

            var client = DocumentDBClient.CreateDocumentClient();

            if (client == null)
                return false;

            try
            {
                var query = client.CreateDocumentQuery<long>(collectionUri, new SqlQuerySpec()
                {
                    QueryText = "SELECT VALUE COUNT(1) FROM interactions i " +
                                "WHERE i.id = @interactionId " +
                                "AND i.CustomerId = @customerId",

                    Parameters = new SqlParameterCollection()
                    {
                        new SqlParameter("@interactionId", interactionId),
                        new SqlParameter("@customerId", customerId)
                    }
                }).AsEnumerable().FirstOrDefault();

                return Convert.ToBoolean(Convert.ToInt16(query));
            }
            catch (DocumentQueryException)
            {
                return false;
            }

        }

        public async Task<bool> DoesCustomerHaveATerminationDate(Guid customerId)
        {
            var documentUri = DocumentDBHelper.CreateCustomerDocumentUri(customerId);

            var client = DocumentDBClient.CreateDocumentClient();

            if (client == null)
                return false;

            try
            {
                var response = await client.ReadDocumentAsync(documentUri);

                var dateOfTermination = response.Resource?.GetPropertyValue<DateTime?>("DateOfTermination");

                return dateOfTermination.HasValue;
            }
            catch (DocumentClientException)
            {
                return false;
            }
        }

        public async Task<List<Session>> GetSessionsForCustomerAsync(Guid customerId)
        {
            var collectionUri = DocumentDBHelper.CreateDocumentCollectionUri();

            var client = DocumentDBClient.CreateDocumentClient();

            if (client == null)
                return null;

            var sessionsQuery = client.CreateDocumentQuery<Session>(collectionUri)
                .Where(so => so.CustomerId == customerId).AsDocumentQuery();

            var sessions = new List<Session>();

            while (sessionsQuery.HasMoreResults)
            {
                var response = await sessionsQuery.ExecuteNextAsync<Session>();
                sessions.AddRange(response);
            }

            return sessions.Any() ? sessions : null;
        }

        public async Task<Session> GetSessionForCustomerAsync(Guid customerId, Guid sessionId)
        {
            var collectionUri = DocumentDBHelper.CreateDocumentCollectionUri();

            var client = DocumentDBClient.CreateDocumentClient();

            var sessionForCustomerQuery = client
                ?.CreateDocumentQuery<Session>(collectionUri, new FeedOptions { MaxItemCount = 1 })
                .Where(x => x.CustomerId == customerId && x.SessionId == sessionId)
                .AsDocumentQuery();

            if (sessionForCustomerQuery == null)
                return null;

            var sessions = await sessionForCustomerQuery.ExecuteNextAsync<Session>();

            return sessions?.FirstOrDefault();
        }


        public async Task<ResourceResponse<Document>> CreateSessionAsync(Session session)
        {

            var collectionUri = DocumentDBHelper.CreateDocumentCollectionUri();

            var client = DocumentDBClient.CreateDocumentClient();

            if (client == null)
                return null;

            var response = await client.CreateDocumentAsync(collectionUri, session);

            return response;

        }

        public async Task<ResourceResponse<Document>> UpdateSessionAsync(Session session)
        {
            var documentUri = DocumentDBHelper.CreateDocumentUri(session.SessionId.GetValueOrDefault());

            var client = DocumentDBClient.CreateDocumentClient();

            if (client == null)
                return null;

            var response = await client.ReplaceDocumentAsync(documentUri, session);

            return response;
        }
    }
}