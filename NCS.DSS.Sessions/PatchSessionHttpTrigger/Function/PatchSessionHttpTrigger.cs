using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using System;
using System.Web.Http.Description;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.Extensions.Logging;
using NCS.DSS.Sessions.Annotations;
using NCS.DSS.Sessions.Cosmos.Helper;
using NCS.DSS.Sessions.Helpers;
using NCS.DSS.Sessions.Ioc;
using NCS.DSS.Sessions.Models;
using NCS.DSS.Sessions.PatchSessionHttpTrigger.Service;
using NCS.DSS.Sessions.Validation;

namespace NCS.DSS.Sessions.PatchSessionHttpTrigger.Function
{
    public static class PatchSessionHttpTrigger
    {
        [FunctionName("PATCH")]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Sessions Patched", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Resource Does Not Exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Patch request is malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API Key unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient Access To This Resource", ShowSchema = false)]
        [Response(HttpStatusCode = (int)422, Description = "Sessions resource validation error(s)", ShowSchema = false)]
        [ResponseType(typeof(Session))]
        [Display(Name = "Patch", Description = "Ability to update a session object for a given customer.")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "customers/{customerId}/interactions/{interactionId}/sessions/{sessionId}")]HttpRequestMessage req, ILogger log, string customerId, string interactionId, string sessionId,
            [Inject]IResourceHelper resourceHelper,
            [Inject]IHttpRequestMessageHelper httpRequestMessageHelper,
            [Inject]IValidate validate,
            [Inject]IPatchSessionHttpTriggerService sessionPatchService)
        {
            var touchpointId = httpRequestMessageHelper.GetTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                log.LogInformation("Unable to locate 'TouchpointId' in request header.");
                return HttpResponseMessageHelper.BadRequest();
            }

            var ApimURL = httpRequestMessageHelper.GetApimURL(req);
            if (string.IsNullOrEmpty(ApimURL))
            {
                log.LogInformation("Unable to locate 'apimurl' in request header");
                return HttpResponseMessageHelper.BadRequest();
            }

            log.LogInformation("C# HTTP trigger function Patch Session processed a request. " + touchpointId);

            if (!Guid.TryParse(customerId, out var customerGuid))
                return HttpResponseMessageHelper.BadRequest(customerGuid);

            if (!Guid.TryParse(interactionId, out var interactionGuid))
                return HttpResponseMessageHelper.BadRequest(interactionGuid);

            if (!Guid.TryParse(sessionId, out var sessionGuid))
                return HttpResponseMessageHelper.BadRequest(sessionGuid);

            SessionPatch sessionPatchRequest;

            try
            {
                sessionPatchRequest = await httpRequestMessageHelper.GetSessionFromRequest<SessionPatch>(req);
            }
            catch (JsonException ex)
            {
                return HttpResponseMessageHelper.UnprocessableEntity(ex);
            }

            if (sessionPatchRequest == null)
                return HttpResponseMessageHelper.UnprocessableEntity(req);

            sessionPatchRequest.LastModifiedTouchpointId = touchpointId;

            var errors = validate.ValidateResource(sessionPatchRequest);

            if (errors != null && errors.Any())
                return HttpResponseMessageHelper.UnprocessableEntity(errors);

            var doesCustomerExist = await resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
                return HttpResponseMessageHelper.NoContent(customerGuid);

            var isCustomerReadOnly = await resourceHelper.IsCustomerReadOnly(customerGuid);

            if (isCustomerReadOnly)
                return HttpResponseMessageHelper.Forbidden(customerGuid);

            var doesInteractionExist = await resourceHelper.DoesInteractionExist(interactionGuid);

            if (!doesInteractionExist)
                return HttpResponseMessageHelper.NoContent(interactionGuid);

            var session = await sessionPatchService.GetSessionForCustomerAsync(customerGuid, sessionGuid);

            if (session == null)
                return HttpResponseMessageHelper.NoContent(sessionGuid);

            var updatedSession = await sessionPatchService.UpdateAsync(session, sessionPatchRequest);

            if (updatedSession != null)
                await sessionPatchService.SendToServiceBusQueueAsync(updatedSession, customerGuid, ApimURL);

            return updatedSession == null ?
                HttpResponseMessageHelper.BadRequest(sessionGuid) :
                HttpResponseMessageHelper.Ok(JsonHelper.SerializeObject(updatedSession));
        }

    }
}