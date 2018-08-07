using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace NCS.DSS.Sessions.Helpers
{
    public interface IHttpRequestMessageHelper
    {
        Task<T> GetSessionFromRequest<T>(HttpRequestMessage req);
        string GetTouchpointId(HttpRequestMessage req);
    }
}