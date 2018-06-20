using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using System;
using System.Web.Http.Description;
using System.ComponentModel.DataAnnotations;
using NCS.DSS.Sessions.Annotations;

namespace NCS.DSS.Sessions.PutSessionHttpTrigger
{
    public static class PutSessionHttpTrigger
    {
        [FunctionName("PUT")]
        [SessionsResponse(HttpStatusCode = (int)HttpStatusCode.Created, Description = "Session Replaced", ShowSchema = true)]
        [SessionsResponse(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Unable to Replace Session", ShowSchema = false)]
        [SessionsResponse(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Forbidden", ShowSchema = false)]
        [ResponseType(typeof(Models.Session))]
        [Display(Name = "Put", Description = "Ability to replace a session object for a given customer.")]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "Customers/{customerId}/sessions/{sessionId}")]HttpRequestMessage req, TraceWriter log, string customerId, string sessionId)
        {
            log.Info("C# HTTP trigger function Put Session processed a request.");

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
                Content = new StringContent(JsonConvert.SerializeObject("Put Session successful for id: " + sessionId),
                    System.Text.Encoding.UTF8, "application/json")
            };
        }

    }
}
