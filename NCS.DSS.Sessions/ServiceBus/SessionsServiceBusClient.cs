using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using NCS.DSS.Sessions.Models;
using Newtonsoft.Json;
using System.Text;

namespace NCS.DSS.Sessions.ServiceBus
{
    public class SessionsServiceBusClient : ISessionsServiceBusClient
    {
        private readonly ILogger<SessionsServiceBusClient> _logger;
        public readonly string QueueName = Environment.GetEnvironmentVariable("QueueName");
        private readonly ServiceBusClient _serviceBusClient;
        public SessionsServiceBusClient(ServiceBusClient serviceBusClient, ILogger<SessionsServiceBusClient> logger)
        {
            _serviceBusClient = serviceBusClient;
            _logger = logger;
        }
        public async Task SendPostMessageAsync(Session session, string reqUrl)
        {
            try
            {
                _logger.LogInformation("Attempting to Create Sender for Service Bus Client");
                var serviceBusSender = _serviceBusClient.CreateSender(QueueName);
                _logger.LogInformation("Preparing Message for Service Bus");
                var messageModel = new MessageModel()
                {
                    TitleMessage = "New Session record {" + session.SessionId + "} added at " + DateTime.UtcNow,
                    CustomerGuid = session.CustomerId,
                    LastModifiedDate = session.LastModifiedDate,
                    URL = reqUrl + "/" + session.SessionId,
                    IsNewCustomer = false,
                    TouchpointId = session.LastModifiedTouchpointId
                };

                var msg = new ServiceBusMessage(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(messageModel)))
                {
                    ContentType = "application/json",
                    MessageId = session.CustomerId + " " + DateTime.UtcNow
                };
                _logger.LogInformation("Attempting to Send Service Bus Message for Employment Progression with ID {SessionId}", session.SessionId);
                await serviceBusSender.SendMessageAsync(msg);
                _logger.LogInformation("POST Service Bus Message for Employment Progression with ID {SessionId} has been sent successfully", session.SessionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to Send POST Service Bus Message for Employment Progression with ID {SessionId}. Exception Raised with {Message}.", session.SessionId, ex.Message);
                throw;
            }
        }

        public async Task SendPatchMessageAsync(Session session, Guid customerId, string reqUrl)
        {
            try
            {
                _logger.LogInformation("Attempting to Create Sender for Service Bus Client");
                var serviceBusSender = _serviceBusClient.CreateSender(QueueName);
                _logger.LogInformation("Preparing Message for Service Bus");
                var messageModel = new MessageModel
                {
                    TitleMessage = "Session record modification for {" + customerId + "} at " + DateTime.UtcNow,
                    CustomerGuid = customerId,
                    LastModifiedDate = session.LastModifiedDate,
                    URL = reqUrl,
                    IsNewCustomer = false,
                    TouchpointId = session.LastModifiedTouchpointId
                };

                var msg = new ServiceBusMessage(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(messageModel)))
                {
                    ContentType = "application/json",
                    MessageId = customerId + " " + DateTime.UtcNow
                }; 
                _logger.LogInformation("Attempting to Send Service Bus Message for Employment Progression with ID {SessionId}", session.SessionId);
                await serviceBusSender.SendMessageAsync(msg);
                _logger.LogInformation("PATCH Service Bus Message for Employment Progression with ID {SessionId} has been sent successfully", session.SessionId);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to Send PATCH Service Bus Message for Employment Progression with ID {SessionId}. Exception Raised with {Message}.", session.SessionId, ex.Message);
                throw;
            }
        }
    }
}