using System.Threading.Tasks;

namespace NCS.DSS.Sessions.PostSessionHttpTrigger.Function
{
    public interface IPostSessionHttpTriggerService
    {
        Task<Models.Session> CreateAsync(Models.Session session);
    }
}