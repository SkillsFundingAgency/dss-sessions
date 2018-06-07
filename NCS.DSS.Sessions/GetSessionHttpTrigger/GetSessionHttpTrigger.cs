using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;

namespace NCS.DSS.Sessions.GetSessionHttpTrigger
{
    public static class GetSessionHttpTrigger
    {
        [FunctionName("GetSession")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "customers/sessions/")]HttpRequestMessage req, TraceWriter log)
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
