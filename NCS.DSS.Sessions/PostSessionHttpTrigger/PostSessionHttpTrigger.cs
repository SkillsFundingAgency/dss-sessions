using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using System;
using System.Web.Http.Description;

namespace NCS.DSS.Sessions.PostSessionHttpTrigger
{
    public static class PostSessionHttpTrigger
    {
        [FunctionName("POST")]
        [ResponseType(typeof(Models.Session))]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "Post", Route = "customers/{customerId}/sessions/{sessionid}")]HttpRequestMessage req, TraceWriter log, string sessionId)
        {
            log.Info("C# HTTP trigger function Post Session processed a request.");

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
                Content = new StringContent(JsonConvert.SerializeObject("Post Session successful for id: " + sessionId),
                    System.Text.Encoding.UTF8, "application/json")
            };
        }

    }
}
