using System.Threading.Tasks;

namespace NCS.DSS.Sessions.PostSessionHttpTrigger.Service
{
    public interface IPostSessionHttpTriggerService
    {
        Task<Models.Session> CreateAsync(Models.Session session);
    }
}