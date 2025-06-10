using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NCS.DSS.Sessions.Models;
using System.Net;
using System.Text.Json;

namespace NCS.DSS.Sessions.Cosmos.Provider
{
    public class CosmosDBProvider : ICosmosDBProvider
    {
        private readonly Container _container;
        private readonly Container _customerContainer;
        private readonly Container _interactionContainer;
        private readonly ILogger<CosmosDBProvider> _logger;
        public CosmosDBProvider(CosmosClient cosmosClient,
            IOptions<SessionsConfigurationSettings> configOptions,
            ILogger<CosmosDBProvider> logger)
        {
            _container = GetContainer(cosmosClient, configOptions.Value.DatabaseId, configOptions.Value.CollectionId);
            _customerContainer = GetContainer(cosmosClient, configOptions.Value.CustomerDatabaseId, configOptions.Value.CustomerCollectionId);
            _interactionContainer = GetContainer(cosmosClient, configOptions.Value.InteractionDatabaseId, configOptions.Value.InteractionCollectionId);
            _logger = logger;
        }
        private static Container GetContainer(CosmosClient cosmosClient, string databaseId, string collectionId)
          => cosmosClient.GetContainer(databaseId, collectionId);
        public async Task<bool> DoesCustomerResourceExist(Guid customerId)
        {
            try
            {
                var queryCust = _customerContainer.GetItemLinqQueryable<Customer>().Where(x => x.id == customerId).ToFeedIterator();

                while (queryCust.HasMoreResults)
                {
                    var response = await queryCust.ReadNextAsync();
                    if (response != null)
                    {
                        _logger.LogInformation("Customer Record found in Cosmos DB for {CustomerID}", customerId);
                        return true;
                    }
                }
                _logger.LogWarning("No Customer Record found with {CustomerID} in Cosmos DB", customerId);
                return false;
            }
            catch (CosmosException ce)
            {
                _logger.LogError(ce, "Failed to find the Customer Record in Cosmos DB {CustomerID}. Exception {Exception}.", customerId, ce.Message);
                throw;
            }
        }

        public async Task<bool> DoesInteractionResourceExistAndBelongToCustomer(Guid interactionId, Guid customerId)
        {
            try
            {
                var queryInt = _interactionContainer.GetItemLinqQueryable<Interaction>()
                                    .Where(x => x.CustomerId == customerId && x.id == interactionId).ToFeedIterator();

                while (queryInt.HasMoreResults)
                {
                    var response = await queryInt.ReadNextAsync();
                    if (response != null && response.Resource.Any())
                    {
                        _logger.LogInformation("Interaction Record found with ID {InteractionId} in Cosmos DB for Customer with ID {CustomerID}", interactionId, customerId);
                        return true;
                    }
                }
                _logger.LogWarning("No Interaction found with ID {InteractionId} and Customer ID {CustomerID} in Cosmos DB", interactionId, customerId);
                return false;
            }
            catch (CosmosException ce)
            {
                _logger.LogError(ce, "Failed to find the Interaction Record in Cosmos DB {CustomerID}. Exception {Exception}", customerId, ce.Message);
                throw;
            }

        }

        public async Task<bool> DoesCustomerHaveATerminationDate(Guid customerId)
        {
            try
            {
                var queryCust = _customerContainer.GetItemLinqQueryable<Customer>().Where(x => x.id == customerId).ToFeedIterator();

                while (queryCust.HasMoreResults)
                {
                    var response = await queryCust.ReadNextAsync();
                    var tDate = response.Resource.FirstOrDefault().DateOfTermination;
                    _logger.LogInformation("Customer with {CustomerID} Have a termination date of {tDate} ", customerId, tDate);
                    return tDate.HasValue;
                }
                _logger.LogWarning("No Customer Record found with {CustomerID} in Cosmos DB", customerId);
                return false;
            }
            catch (CosmosException ce)
            {
                _logger.LogError(ce, "Failed to get DateOfTermination for {CustomerID}. Exception {Exception}.", customerId, ce.Message);
                throw;
            }
        }

        public async Task<List<Session>> GetSessionsForCustomerAsync(Guid customerId)
        {
            try
            {
                var queryCdb = _container.GetItemLinqQueryable<Models.Session>().Where(x => x.CustomerId == customerId).ToFeedIterator();

                while (queryCdb.HasMoreResults)
                {
                    var response = await queryCdb.ReadNextAsync();
                    if (response != null && response.Resource.Any())
                    {
                        _logger.LogInformation("Interaction Records found in Cosmos DB for Customer with ID {CustomerID}", customerId);
                        return response.Resource.ToList();
                    }
                }
                _logger.LogWarning("No Interaction found with {CustomerID} in Cosmos DB", customerId);
                return null;
            }
            catch (CosmosException ce)
            {
                _logger.LogError(ce, "Failed to find the Interaction Record in Cosmos DB {CustomerID}. Exception {Exception}", customerId, ce.Message);
                throw;
            }
        }

        public async Task<Session> GetSessionForCustomerAsync(Guid customerId, Guid sessionId)
        {
            try
            {
                var queryCdb = _container.GetItemLinqQueryable<Models.Session>().Where(x => x.SessionId == sessionId && x.CustomerId == customerId).ToFeedIterator();

                while (queryCdb.HasMoreResults)
                {
                    var response = await queryCdb.ReadNextAsync();
                    if (response != null && response.Resource.Any())
                    {
                        _logger.LogInformation("Session Record found with ID {SessionId} in Cosmos DB for Customer with ID {CustomerID}",sessionId, customerId);
                        return response.Resource.FirstOrDefault();
                    }
                }
                _logger.LogWarning("No Session found with ID {SessionId} for Customer with {CustomerID} in Cosmos DB", sessionId, customerId);
                return null;
            }
            catch (CosmosException ce)
            {
                _logger.LogError(ce, "Failed to find the Session Record with ID {SessionId} in Cosmos DB {CustomerID}. Exception {Exception}", sessionId, customerId, ce.Message);
                throw;
            }
        }

        public async Task<string> GetSessionForCustomerToUpdateAsync(Guid customerId, Guid sessionId)
        {
            try
            {
                var queryCdb = _container.GetItemLinqQueryable<Models.Session>().Where(x => x.SessionId == sessionId && x.CustomerId == customerId).ToFeedIterator();

                while (queryCdb.HasMoreResults)
                {
                    var response = await queryCdb.ReadNextAsync();
                    if (response != null && response.Resource.Any())
                    {
                        var jsonString = JsonSerializer.Serialize(response.Resource.FirstOrDefault());
                        _logger.LogInformation("Session Record found with ID {SessionId} in Cosmos DB for Customer with ID {CustomerID}", sessionId, customerId);
                        return jsonString;
                    }
                }
                _logger.LogWarning("No Session found with ID {SessionId} for Customer with {CustomerID} in Cosmos DB", sessionId, customerId);
                return null;
            }
            catch (CosmosException ce)
            {
                _logger.LogError(ce, "Failed to find the Session Record with ID {SessionId} in Cosmos DB {CustomerID}. Exception {Exception}", sessionId, customerId, ce.Message);
                throw;
            }
        }

        public async Task<ItemResponse<Session>> CreateSessionAsync(Session session)
        {
            try
            {
                var response = await _container.CreateItemAsync(session, null);
                if (response.StatusCode == HttpStatusCode.Created)
                {
                    _logger.LogInformation("Session Record Created in Cosmos DB for {SessionId}", session.SessionId);
                }
                else
                {
                    _logger.LogError("Failed and returned {StatusCode} to CreateSession Record in Cosmos DB for {SessionId}", response.StatusCode, session.SessionId);
                }
                return response;
            }
            catch (CosmosException ce)
            {
                _logger.LogError(ce, "Failed to CreateSession Record in Cosmos DB {SessionId}. Exception {Exception}.", session.SessionId, ce.Message);
                throw;
            }

        }

        public async Task<ItemResponse<Session>> UpdateSessionAsync(string sessionJson, Guid sessionId)
        {
            try
            {
                var session = JsonSerializer.Deserialize<Models.Session>(sessionJson);
                var response = await _container.ReplaceItemAsync(session, sessionId.ToString());
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    _logger.LogInformation("Session Record Updated in Cosmos DB for {SessionId}", session.SessionId);
                }
                else
                {
                    _logger.LogError("Failed and returned {StatusCode} to Update Session Record in Cosmos DB for {SessionId}", response.StatusCode, session.SessionId);
                }
                return response;
            }
            catch (CosmosException ce)
            {
                _logger.LogError(ce, "Failed to Update Session Record in Cosmos DB {SessionId}. Exception {Exception}.", sessionId, ce.Message);
                throw;
            }
        }

    }
}