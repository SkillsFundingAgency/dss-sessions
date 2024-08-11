using NCS.DSS.Sessions.Models;
using System.Threading.Tasks;

namespace NCS.DSS.Sessions.PostSessionHttpTrigger.Service
{
    public interface IPostSessionHttpTriggerService
    {
        Task<Models.Session> CreateAsync(Session session);
        Task SendToServiceBusQueueAsync(Session session, string reqUrl);
    }
}