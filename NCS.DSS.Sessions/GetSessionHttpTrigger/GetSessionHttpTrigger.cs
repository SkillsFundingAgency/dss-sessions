using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http.Description;
using System.ComponentModel.DataAnnotations;
using NCS.DSS.Sessions.Annotations;

namespace NCS.DSS.Sessions.GetSessionHttpTrigger
{
    public static class GetSessionHttpTrigger
    {
        [FunctionName("GET")]
        [SessionsResponse(HttpStatusCode = (int)HttpStatusCode.Created, Description = "Sessions Retrieved", ShowSchema = true)]
        [SessionsResponse(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Unable to Retrieve Sessions", ShowSchema = false)]
        [SessionsResponse(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Forbidden", ShowSchema = false)]
        [ResponseType(typeof(Models.Session))]
        [Display(Name = "Get", Description = "Ability to get a session object for a given customer.")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Customers/sessions/")]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function GetSession processed a request.");

            var service = new GetSessionHttpTriggerService();
            var values = await service.GetSessions();

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(values),
                    System.Text.Encoding.UTF8, "application/json")
            };
        }

    }
}
