using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net;
using System;
using System.Web.Http.Description;
using System.ComponentModel.DataAnnotations;
using NCS.DSS.Sessions.Annotations;

namespace NCS.DSS.Sessions.PutSessionHttpTrigger
{
    public static class PutSessionHttpTrigger
    {
        [FunctionName("PUT")]
        [Response(HttpStatusCode = (int)HttpStatusCode.Created, Description = "Sessions Replaced", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Unable to Replace Sessions", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient Access To This Resource", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "Unauthorised", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Resource Does Not Exist", ShowSchema = false)]
        [ResponseType(typeof(Models.Session))]
        [Display(Name = "Put", Description = "Ability to replace a session object for a given customer.")]
        [Disable]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "customers/{customerId}/sessions/{sessionId}")]HttpRequestMessage req, TraceWriter log, string customerId, string sessionId)
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
