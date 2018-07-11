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
using NCS.DSS.Sessions.Cosmos.Helper;
using NCS.DSS.Sessions.GetSessionByIdHttpTrigger.Service;
using NCS.DSS.Sessions.Helpers;
using NCS.DSS.Sessions.Ioc;

namespace NCS.DSS.Sessions.GetSessionByIdHttpTrigger.Function
{
    public static class GetSessionByIdHttpTrigger
    {
        [FunctionName("GETByID")]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Sessions Retrieved", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Resource Does Not Exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Get request is malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API Key unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient Access To This Resource", ShowSchema = false)]
        [ResponseType(typeof(Models.Session))]
        [Display(Name = "GetByID", Description = "Ability to get by ID; a session object for a given customer.")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "customers/{customerId}/interactions/{interactionId}/sessions/{sessionId}")]HttpRequestMessage req, TraceWriter log, string customerId, string interactionId, string sessionId,
            [Inject]IResourceHelper resourceHelper,
            [Inject]IGetSessionByIdHttpTriggerService sessionGetService)
        {
            log.Info("C# HTTP trigger function GetSessionById processed a request.");

            if (!Guid.TryParse(customerId, out var customerGuid))
                return HttpResponseMessageHelper.BadRequest(customerGuid);

            if (!Guid.TryParse(interactionId, out var interactionGuid))
                return HttpResponseMessageHelper.BadRequest(interactionGuid);

            if (!Guid.TryParse(sessionId, out var sessionGuid))
                return HttpResponseMessageHelper.BadRequest(sessionGuid);
            
            var doesCustomerExist = resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
                return HttpResponseMessageHelper.NoContent(customerGuid);

            var doesInteractionExist = resourceHelper.DoesInteractionExist(interactionGuid);

            if (!doesInteractionExist)
                return HttpResponseMessageHelper.NoContent(interactionGuid);

            var session = await sessionGetService.GetSessionForCustomerAsync(customerGuid, sessionGuid);

            return session == null ?
                HttpResponseMessageHelper.NoContent(sessionGuid) :
                HttpResponseMessageHelper.Ok(session);
        }

    }
}
