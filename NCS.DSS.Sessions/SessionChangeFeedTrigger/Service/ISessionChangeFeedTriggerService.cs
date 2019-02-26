using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;

namespace NCS.DSS.Sessions.SessionChangeFeedTrigger.Service
{
    public interface ISessionChangeFeedTriggerService
    {
        Task SendMessageToChangeFeedQueueAsync(Document document);
    }
}
