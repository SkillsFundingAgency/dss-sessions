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

namespace NCS.DSS.Sessions.PostSessionHttpTrigger
{
    public static class PostSessionHttpTrigger
    {
        [FunctionName("POST")]
        [SessionsResponse(HttpStatusCode = (int)HttpStatusCode.Created, Description = "Session Added", ShowSchema = true)]
        [SessionsResponse(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Unable to Add Session", ShowSchema = false)]
        [SessionsResponse(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Forbidden", ShowSchema = false)]
        [ResponseType(typeof(Models.Session))]
        [Display(Name = "Post", Description = "Ability to add a session object for a given customer.")]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "customers/{customerId}/sessions/{sessionId}")]HttpRequestMessage req, TraceWriter log, string customerId, string sessionId)
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
