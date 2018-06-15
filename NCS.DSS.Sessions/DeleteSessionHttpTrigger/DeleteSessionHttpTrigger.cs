using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using System;
using System.Web.Http.Description;

namespace NCS.DSS.Sessions.DeleteSessionHttpTrigger
{
    public static class DeleteSessionHttpTrigger
    {
        [FunctionName("DELETE")]
        [ResponseType(typeof(Models.Session))]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "customers/{customerId}/sessions/{sessionid}")]HttpRequestMessage req, TraceWriter log, string sessionId)
        {
            log.Info("C# HTTP trigger function Delete Session processed a request.");

            if (!Guid.TryParse(sessionId, out var sessionGuid))
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(sessionGuid),
                        System.Text.Encoding.UTF8, "application/json")
                };
            }
            
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject("Delete Session successful for id: " + sessionId),
                    System.Text.Encoding.UTF8, "application/json")
            };
        }

    }
}
