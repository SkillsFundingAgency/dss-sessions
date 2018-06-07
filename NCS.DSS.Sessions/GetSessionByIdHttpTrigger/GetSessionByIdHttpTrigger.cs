using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using System;

namespace NCS.DSS.Sessions.GetSessionByIdHttpTrigger
{
    public static class GetSessionByIdHttpTrigger
    {
        [FunctionName("GetSessionById")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "customers/{customerId:guid}/sessions/{sessionid:guid}")]HttpRequestMessage req, TraceWriter log, string sessionId)
        {
            log.Info("C# HTTP trigger function GetSessionById processed a request.");

            if (!Guid.TryParse(sessionId, out var sessionGuid))
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(sessionGuid),
                        System.Text.Encoding.UTF8, "application/json")
                };
            }
            
            var service = new GetSessionByIdHttpTriggerService();
            var values = await service.GetSessions(sessionGuid);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(values),
                    System.Text.Encoding.UTF8, "application/json")
            };
        }

    }
}
