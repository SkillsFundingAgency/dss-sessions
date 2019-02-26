using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;

namespace NCS.DSS.Sessions.SessionChangeFeedTrigger.Service
{
    public class SessionChangeFeedTriggerService : ISessionChangeFeedTriggerService
    {
        private readonly string _queueName = Environment.GetEnvironmentVariable("ChangeFeedQueueName");
        private readonly string _serviceBusConnectionString = Environment.GetEnvironmentVariable("ChangeFeedServiceBusConnectionString");

        public async Task SendMessageToChangeFeedQueueAsync(Document document)
        {

            if (document == null)
                return;

            var queueClient = new QueueClient(_serviceBusConnectionString, _queueName);

            var changeFeedMessageModel = new ChangeFeedMessageModel()
            {
                Document = document,
                IsSession = true
            };

            var msg = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(changeFeedMessageModel)))
            {
                ContentType = "application/json",
                MessageId = document.Id
            };

            await queueClient.SendAsync(msg);
        }

        public class ChangeFeedMessageModel
        {
            public Document Document { get; set; }
            public bool IsSession { get; set; }
        }
    }
}
