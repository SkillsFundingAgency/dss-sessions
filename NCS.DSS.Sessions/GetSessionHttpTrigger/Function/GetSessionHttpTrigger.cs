using System;
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
using NCS.DSS.Sessions.Cosmos.Helper;
using NCS.DSS.Sessions.GetSessionHttpTrigger.Service;
using NCS.DSS.Sessions.Helpers;
using NCS.DSS.Sessions.Ioc;

namespace NCS.DSS.Sessions.GetSessionHttpTrigger.Function
{
    public static class GetSessionHttpTrigger
    {
        [FunctionName("GET")]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Sessions Retrieved", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Resource Does Not Exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Get request is malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API Key unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient Access To This Resource", ShowSchema = false)]
        [ResponseType(typeof(Models.Session))]
        [Display(Name = "Get", Description = "Ability to get a session object for a given customer.")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "customers/{customerId}/interactions/{interactionId}/sessions")]HttpRequestMessage req, TraceWriter log, string customerId, string interactionId,
            [Inject]IResourceHelper resourceHelper,
            [Inject]IGetSessionHttpTriggerService sessionGetService)
        {
            log.Info("C# HTTP trigger function GetSession processed a request.");

            if (!Guid.TryParse(customerId, out var customerGuid))
                return HttpResponseMessageHelper.BadRequest(customerGuid);

            if (!Guid.TryParse(interactionId, out var interactionGuid))
                return HttpResponseMessageHelper.BadRequest(interactionGuid);

            var doesCustomerExist = resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
                return HttpResponseMessageHelper.NoContent(customerGuid);

            var doesInteractionExist = resourceHelper.DoesInteractionExist(interactionGuid);

            if (!doesInteractionExist)
                return HttpResponseMessageHelper.NoContent(interactionGuid);

            var sessions = await sessionGetService.GetSessionsAsync(customerGuid);

            return sessions == null ?
                HttpResponseMessageHelper.NoContent(interactionGuid) :
                HttpResponseMessageHelper.Ok(sessions);
        }

    }
}
