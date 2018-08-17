using System.Threading.Tasks;
using NCS.DSS.Sessions.Models;

namespace NCS.DSS.Sessions.PostSessionHttpTrigger.Service
{
    public interface IPostSessionHttpTriggerService
    {
        Task<Models.Session> CreateAsync(Session session);
        Task SendToServiceBusQueueAsync(Session session, string reqUrl);
    }
}