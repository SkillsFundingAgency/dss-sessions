using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;

namespace NCS.DSS.Sessions.ServiceBus
{
    public static class ServiceBusClient
    {
        public static readonly string QueueName = Environment.GetEnvironmentVariable("QueueName");
        public static readonly string ServiceBusConnectionString = Environment.GetEnvironmentVariable("ServiceBusConnectionString");

        public static async Task SendPostMessageAsync(Models.Session session, string reqUrl)
        {
            var queueClient = new QueueClient(ServiceBusConnectionString, QueueName);

            var messageModel = new MessageModel()
            {
                TitleMessage = "New Session record {" + session.SessionId + "} added at " + DateTime.UtcNow,
                CustomerGuid = session.CustomerId,
                LastModifiedDate = session.LastModifiedDate,
                URL = reqUrl + "/" + session.SessionId,
                IsNewCustomer = false,
                TouchpointId = session.LastModifiedTouchpointId
            };

            var msg = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(messageModel)))
            {
                ContentType = "application/json",
                MessageId = session.CustomerId + " " + DateTime.UtcNow
            };

            await queueClient.SendAsync(msg);
        }

        public static async Task SendPatchMessageAsync(Models.Session session, Guid customerId, string reqUrl)
        {
            var queueClient = new QueueClient(ServiceBusConnectionString, QueueName);

            var messageModel = new MessageModel
            {
                TitleMessage = "Session record modification for {" + customerId + "} at " + DateTime.UtcNow,
                CustomerGuid = customerId,
                LastModifiedDate = session.LastModifiedDate,
                URL = reqUrl,
                IsNewCustomer = false,
                TouchpointId = session.LastModifiedTouchpointId
            };

            var msg = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(messageModel)))
            {
                ContentType = "application/json",
                MessageId = customerId + " " + DateTime.UtcNow
            };

            await queueClient.SendAsync(msg);
        }
    }

    public class MessageModel
    {
        public string TitleMessage { get; set; }
        public Guid? CustomerGuid { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public string URL { get; set; }
        public bool IsNewCustomer { get; set; }
        public string TouchpointId { get; set; }
    }

}