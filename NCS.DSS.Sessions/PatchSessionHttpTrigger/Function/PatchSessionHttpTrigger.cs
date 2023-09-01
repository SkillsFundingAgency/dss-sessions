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

            var correlationId = _httpRequestHelper.GetDssCorrelationId(req);
            if (string.IsNullOrEmpty(correlationId))
                log.LogInformation("Unable to locate 'DssCorrelationId' in request header");

            if (!Guid.TryParse(correlationId, out var correlationGuid))
            {
                log.LogInformation("Unable to parse 'DssCorrelationId' to a Guid");
                correlationGuid = Guid.NewGuid();
            }

            log.LogInformation($"DssCorrelationId: [{correlationGuid}]");

            var touchpointId = _httpRequestHelper.GetDssTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                var response = _httpResponseMessageHelper.BadRequest();
                log.LogWarning($"Response Status Code: [{response.StatusCode}]. Unable to locate 'TouchpointId' in request header");
                return response;
            }

            var ApimURL = _httpRequestHelper.GetDssApimUrl(req);
            if (string.IsNullOrEmpty(ApimURL))
            {
                var response = _httpResponseMessageHelper.BadRequest();
                log.LogWarning($"Response Status Code: [{response.StatusCode}]. Unable to locate 'apimurl' in request header");
                return response;
            }

            var subcontractorId = _httpRequestHelper.GetDssSubcontractorId(req);
            if (string.IsNullOrEmpty(subcontractorId))
                log.LogInformation($"Unable to locate 'SubcontractorId' in request header");

            log.LogInformation($"Patch Session C# HTTP trigger function  processed a request. By Touchpoint: [{touchpointId}]");

            if (!Guid.TryParse(customerId, out var customerGuid))
            {
                var response = _httpResponseMessageHelper.BadRequest(customerGuid);
                log.LogWarning($"Response Status Code: [{response.StatusCode}]. Unable to parse 'customerId' to a Guid: [{customerId}]");
                return response;
            }

            if (!Guid.TryParse(interactionId, out var interactionGuid))
            {
                var response = _httpResponseMessageHelper.BadRequest(interactionGuid);
                log.LogWarning($"Response Status Code: [{response.StatusCode}]. Unable to parse 'interactionId' to a Guid: [{interactionId}]");
                return response;
            }

            if (!Guid.TryParse(sessionId, out var sessionGuid))
            {
                var response = _httpResponseMessageHelper.BadRequest(sessionGuid);
                log.LogWarning($"Response Status Code: [{response.StatusCode}]. Unable to parse 'sessionId' to a Guid: [{sessionGuid}]");
                return response;
            }

            SessionPatch sessionPatchRequest;

            try
            {
                log.LogInformation($"Attempt to get resource from body of the request");
                sessionPatchRequest = await _httpRequestHelper.GetResourceFromRequest<SessionPatch>(req);
            }
            catch (JsonException ex)
            {
                var response = _httpResponseMessageHelper.UnprocessableEntity(ex);
                log.LogError($"Response Status Code: [{response.StatusCode}]. Unable to retrieve body from req", ex);
                return response;
            }

            if (sessionPatchRequest == null)
            {
                var response = _httpResponseMessageHelper.UnprocessableEntity(req);
                log.LogWarning($"Response Status Code: [{response.StatusCode}]. session patch request is null");
                return response;
            }

            log.LogInformation($"Attempt to set id's for session patch");
            sessionPatchRequest.SetIds(touchpointId, subcontractorId);

            log.LogInformation($"Attempt to validate resource");
            var errors = _validate.ValidateResource(sessionPatchRequest);

            if (errors != null && errors.Any())
            {
                var response = _httpResponseMessageHelper.UnprocessableEntity(errors);
                log.LogWarning($"Response Status Code: [{response.StatusCode}]. validation errors with resource", errors);
                return response;
            }

            log.LogInformation($"Attempting to see if customer exists [{customerGuid}]");
            var doesCustomerExist = await _resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
            {
                var response = _httpResponseMessageHelper.NoContent(customerGuid);
                log.LogWarning($"Response Status Code: [{response.StatusCode}]. Customer does not exist [{customerGuid}]");
                return response;
            }

            log.LogInformation($"Attempting to see if this is a read only customer [{customerGuid}]");
            var isCustomerReadOnly = await _resourceHelper.IsCustomerReadOnly(customerGuid);

            if (isCustomerReadOnly)
            {
                var response = _httpResponseMessageHelper.Forbidden(customerGuid);
                log.LogWarning($"Response Status Code: [{response.StatusCode}]. Customer is read only [{customerGuid}]");
                return response;
            }

            log.LogInformation($"Attempting to see if interaction exists [{interactionGuid}]");
            var doesInteractionExist = _resourceHelper.DoesInteractionResourceExistAndBelongToCustomer(interactionGuid, customerGuid);

            if (!doesInteractionExist)
            {
                var response = _httpResponseMessageHelper.NoContent(interactionGuid);
                log.LogWarning($"Response Status Code: [{response.StatusCode}]. Interaction does not exist [{interactionGuid}]");
                return response;
            }

            log.LogInformation($"Attempting to get sessions for customer [{customerGuid}]");
            var sessionForCustomer = await _sessionPatchService.GetSessionForCustomerAsync(customerGuid, sessionGuid);

            if (sessionForCustomer == null)
            {
                var response = _httpResponseMessageHelper.NoContent(sessionGuid);
                log.LogWarning($"Response Status Code: [{response.StatusCode}]. Session does not exist [{sessionGuid}]");
                return response;
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
                    log.LogError($"Unable to get long and lat for postcode: [{sessionPatchRequest.VenuePostCode}]", e);
                    throw;
                }

                sessionPatchRequest.SetLongitudeAndLatitude(position);
            }

            log.LogInformation($"Attempting to Patch Session [{sessionGuid}]");
            var patchedSession = _sessionPatchService.PatchResource(sessionForCustomer, sessionPatchRequest);

            if (patchedSession == null)
            {
                var response = _httpResponseMessageHelper.NoContent(sessionGuid);
                log.LogWarning($"Response Status Code: [{response.StatusCode}]. Unable to Patch Session [{sessionGuid}]");
                return response;
            }

            log.LogInformation($"Attempting to update Session [{sessionGuid}]");
            var updatedSession = await _sessionPatchService.UpdateCosmosAsync(patchedSession, sessionGuid);

            if (updatedSession != null)
            {
                log.LogInformation($"Attempting to send to service bus [{sessionGuid}]");
                await _sessionPatchService.SendToServiceBusQueueAsync(updatedSession, customerGuid, ApimURL);
            }

            if (updatedSession == null)
            {
                var response = _httpResponseMessageHelper.BadRequest(sessionGuid);
                log.LogWarning($"Response Status Code: [{response.StatusCode}]. Failed to patch the session [{sessionGuid}]");
                return response;
            }
            else
            {
                var response = _httpResponseMessageHelper.Ok(_jsonHelper.SerializeObjectAndRenameIdProperty(updatedSession, "id", "SessionId"));
                log.LogInformation($"Response Status Code: [{response.StatusCode}]. Successfully patched the session [{sessionGuid}]");
                return response;
            }
        }
    }
}