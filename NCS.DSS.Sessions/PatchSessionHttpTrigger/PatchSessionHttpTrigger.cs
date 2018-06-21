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

namespace NCS.DSS.Sessions.PatchSessionHttpTrigger
{
    public static class PatchSessionHttpTrigger
    {
        [FunctionName("PATCH")]
        [SessionsResponse(HttpStatusCode = (int)HttpStatusCode.Created, Description = "Session Updated", ShowSchema = true)]
        [SessionsResponse(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Unable to Update Session", ShowSchema = false)]
        [SessionsResponse(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Forbidden", ShowSchema = false)]
        [ResponseType(typeof(Models.Session))]
        [Display(Name = "Patch", Description = "Ability to update a session object for a given customer.")]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "customers/{customerId}/sessions/{sessionId}")]HttpRequestMessage req, TraceWriter log, string customerId, string sessionId)
        {
            log.Info("C# HTTP trigger function Patch Session processed a request.");

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
                Content = new StringContent(JsonConvert.SerializeObject("Patch Session successful for id: " + sessionId),
                    System.Text.Encoding.UTF8, "application/json")
            };
        }

    }
}
