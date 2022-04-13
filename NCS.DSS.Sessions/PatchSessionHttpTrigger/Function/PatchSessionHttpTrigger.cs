using DFC.Common.Standard.Logging;
using DFC.GeoCoding.Standard.AzureMaps.Model;
using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using NCS.DSS.Sessions.Cosmos.Helper;
using NCS.DSS.Sessions.GeoCoding;
using NCS.DSS.Sessions.Models;
using NCS.DSS.Sessions.PatchSessionHttpTrigger.Service;
using NCS.DSS.Sessions.Validation;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace NCS.DSS.Sessions.PatchSessionHttpTrigger.Function
{
    public class PatchSessionHttpTrigger
    {
        private IResourceHelper _resourceHelper;
        private IValidate _validate;
        private IPatchSessionHttpTriggerService _sessionPatchService;
        private ILoggerHelper _loggerHelper;
        private IHttpRequestHelper _httpRequestHelper;
        private IHttpResponseMessageHelper _httpResponseMessageHelper;
        private IJsonHelper _jsonHelper;
        private IGeoCodingService _geoCodingService;

        public PatchSessionHttpTrigger(
            IResourceHelper resourceHelper,
            IValidate validate,
            IPatchSessionHttpTriggerService sessionPatchService,
            ILoggerHelper loggerHelper,
            IHttpRequestHelper httpRequestHelper,
            IHttpResponseMessageHelper httpResponseMessageHelper,
            IJsonHelper jsonHelper,
            IGeoCodingService geoCodingService)
        {
            _resourceHelper = resourceHelper;
            _validate = validate;
            _sessionPatchService = sessionPatchService;
            _loggerHelper = loggerHelper;
            _httpRequestHelper = httpRequestHelper;
            _httpResponseMessageHelper = httpResponseMessageHelper;
            _jsonHelper = jsonHelper;
            _geoCodingService = geoCodingService;
        }

        [FunctionName("PATCH")]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Sessions Patched", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Resource Does Not Exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Patch request is malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API Key unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient Access To This Resource", ShowSchema = false)]
        [Response(HttpStatusCode = (int)422, Description = "Sessions resource validation error(s)", ShowSchema = false)]
        [ProducesResponseType(typeof(Models.Session), 200)]
        [Display(Name = "Patch", Description = "Ability to update a session object for a given customer.")]
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "customers/{customerId}/interactions/{interactionId}/sessions/{sessionId}")]HttpRequest req, ILogger log, string customerId, string interactionId, string sessionId)
        {
            _loggerHelper.LogMethodEnter(log);

            var correlationId = _httpRequestHelper.GetDssCorrelationId(req);
            if (string.IsNullOrEmpty(correlationId))
                log.LogInformation("Unable to locate 'DssCorrelationId' in request header");

            if (!Guid.TryParse(correlationId, out var correlationGuid))
            {
                log.LogInformation("Unable to parse 'DssCorrelationId' to a Guid");
                correlationGuid = Guid.NewGuid();
            }

            var touchpointId = _httpRequestHelper.GetDssTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, "Unable to locate 'TouchpointId' in request header");
                return _httpResponseMessageHelper.BadRequest();
            }

            var ApimURL = _httpRequestHelper.GetDssApimUrl(req);
            if (string.IsNullOrEmpty(ApimURL))
            {
                log.LogInformation("Unable to locate 'apimurl' in request header");
                return _httpResponseMessageHelper.BadRequest();
            }

            var subcontractorId = _httpRequestHelper.GetDssSubcontractorId(req);
            if (string.IsNullOrEmpty(subcontractorId))
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, "Unable to locate 'subcontractorId' in request header");
                return _httpResponseMessageHelper.BadRequest();
            }

            _loggerHelper.LogInformationMessage(log, correlationGuid,
                string.Format("Patch Session C# HTTP trigger function  processed a request. By Touchpoint: {0}",
                    touchpointId));

            if (!Guid.TryParse(customerId, out var customerGuid))
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Unable to parse 'customerId' to a Guid: {0}", customerId));
                return _httpResponseMessageHelper.BadRequest(customerGuid);
            }

            if (!Guid.TryParse(interactionId, out var interactionGuid))
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Unable to parse 'interactionId' to a Guid: {0}", interactionId));
                return _httpResponseMessageHelper.BadRequest(interactionGuid);
            }

            if (!Guid.TryParse(sessionId, out var sessionGuid))
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Unable to parse 'sessionId' to a Guid: {0}", sessionGuid));
                return _httpResponseMessageHelper.BadRequest(sessionGuid);
            }

            SessionPatch sessionPatchRequest;

            try
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, "Attempt to get resource from body of the request");
                sessionPatchRequest = await _httpRequestHelper.GetResourceFromRequest<SessionPatch>(req);
            }
            catch (JsonException ex)
            {
                _loggerHelper.LogError(log, correlationGuid, "Unable to retrieve body from req", ex);
                return _httpResponseMessageHelper.UnprocessableEntity(ex);
            }

            if (sessionPatchRequest == null)
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, "session patch request is null");
                return _httpResponseMessageHelper.UnprocessableEntity(req);
            }

            _loggerHelper.LogInformationMessage(log, correlationGuid, "Attempt to set id's for session patch");
            sessionPatchRequest.SetIds(touchpointId, subcontractorId);

            _loggerHelper.LogInformationMessage(log, correlationGuid, "Attempt to validate resource");
            var errors = _validate.ValidateResource(sessionPatchRequest);

            if (errors != null && errors.Any())
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, "validation errors with resource");
                return _httpResponseMessageHelper.UnprocessableEntity(errors);
            }

            _loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Attempting to see if customer exists {0}", customerGuid));
            var doesCustomerExist = await _resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Customer does not exist {0}", customerGuid));
                return _httpResponseMessageHelper.NoContent(customerGuid);
            }

            _loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Attempting to see if this is a read only customer {0}", customerGuid));
            var isCustomerReadOnly = await _resourceHelper.IsCustomerReadOnly(customerGuid);

            if (isCustomerReadOnly)
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Customer is read only {0}", customerGuid));
                return _httpResponseMessageHelper.Forbidden(customerGuid);
            }

            _loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Attempting to see if interaction exists {0}", interactionGuid));
            var doesInteractionExist = _resourceHelper.DoesInteractionResourceExistAndBelongToCustomer(interactionGuid, customerGuid);

            if (!doesInteractionExist)
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Interaction does not exist {0}", interactionGuid));
                return _httpResponseMessageHelper.NoContent(interactionGuid);
            }

            _loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Attempting to get sessions for customer {0}", customerGuid));
            var sessionForCustomer = await _sessionPatchService.GetSessionForCustomerAsync(customerGuid, sessionGuid);

            if (sessionForCustomer == null)
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Session does not exist {0}", sessionGuid));
                return _httpResponseMessageHelper.NoContent(sessionGuid);
            }
             
            if (!string.IsNullOrEmpty(sessionPatchRequest.VenuePostCode))
            {
                Position position;

                try
                {
                    var postcode = sessionPatchRequest.VenuePostCode.Replace(" ", string.Empty);
                    position = await _geoCodingService.GetPositionForPostcodeAsync(postcode);
                }
                catch (Exception e)
                {
                    _loggerHelper.LogException(log, correlationGuid, string.Format("Unable to get long and lat for postcode: {0}", sessionPatchRequest.VenuePostCode), e);
                    throw;
                }

                sessionPatchRequest.SetLongitudeAndLatitude(position);
            }

            _loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Attempting to Patch Session {0}", sessionGuid));
            var patchedSession = _sessionPatchService.PatchResource(sessionForCustomer, sessionPatchRequest);

            if (patchedSession == null)
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Unable to Patch Session {0}", sessionGuid));
                return _httpResponseMessageHelper.NoContent(sessionGuid);
            }

            _loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Attempting to update Session {0}", sessionGuid));
            var updatedSession = await _sessionPatchService.UpdateCosmosAsync(patchedSession, sessionGuid);

            if (updatedSession != null)
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("attempting to send to service bus {0}", sessionGuid));
                await _sessionPatchService.SendToServiceBusQueueAsync(updatedSession, customerGuid, ApimURL);
            }

            return updatedSession == null ?
                _httpResponseMessageHelper.BadRequest(sessionGuid) :
                _httpResponseMessageHelper.Ok(_jsonHelper.SerializeObjectAndRenameIdProperty(updatedSession, "id", "SessionId"));
        }
    }
}