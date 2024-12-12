using DFC.GeoCoding.Standard.AzureMaps.Model;
using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.Sessions.Cosmos.Provider;
using NCS.DSS.Sessions.GeoCoding;
using NCS.DSS.Sessions.Helpers;
using NCS.DSS.Sessions.Models;
using NCS.DSS.Sessions.PatchSessionHttpTrigger.Service;
using NCS.DSS.Sessions.Validation;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;

namespace NCS.DSS.Sessions.PatchSessionHttpTrigger.Function
{
    public class PatchSessionHttpTrigger
    {
        private ICosmosDBProvider _cosmosDbProvider;
        private IValidate _validate;
        private IPatchSessionHttpTriggerService _sessionPatchService;
        private ILogger<PatchSessionHttpTrigger> _logger;
        private IHttpRequestHelper _httpRequestHelper;
        private IHttpResponseMessageHelper _httpResponseMessageHelper;
        private IGeoCodingService _geoCodingService;
        private IDynamicHelper _dynamicHelper;

        public PatchSessionHttpTrigger(
            ICosmosDBProvider cosmosDBProvider,
            IValidate validate,
            IPatchSessionHttpTriggerService sessionPatchService,
            ILogger<PatchSessionHttpTrigger> logger,
            IHttpRequestHelper httpRequestHelper,
            IHttpResponseMessageHelper httpResponseMessageHelper,
            IGeoCodingService geoCodingService,
            IDynamicHelper dynamicHelper)
        {
            _cosmosDbProvider = cosmosDBProvider;
            _validate = validate;
            _sessionPatchService = sessionPatchService;
            _logger = logger;
            _httpRequestHelper = httpRequestHelper;
            _httpResponseMessageHelper = httpResponseMessageHelper;
            _geoCodingService = geoCodingService;
            _dynamicHelper = dynamicHelper;
        }

        [Function("PATCH")]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Sessions Patched", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Resource Does Not Exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Patch request is malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API Key unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient Access To This Resource", ShowSchema = false)]
        [Response(HttpStatusCode = (int)422, Description = "Sessions resource validation error(s)", ShowSchema = false)]
        [ProducesResponseType(typeof(Models.Session), 200)]
        [Display(Name = "Patch", Description = "Ability to update a session object for a given customer.")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "customers/{customerId}/interactions/{interactionId}/sessions/{sessionId}")] HttpRequest req, string customerId, string interactionId, string sessionId)
        {
            var functionName = nameof(PatchSessionHttpTrigger);

            _logger.LogInformation("Function {FunctionName} has been invoked", functionName);

            var correlationId = _httpRequestHelper.GetDssCorrelationId(req);
            if (string.IsNullOrEmpty(correlationId))
                _logger.LogInformation("Unable to locate 'DssCorrelationId' in request header");

            if (!Guid.TryParse(correlationId, out var correlationGuid))
            {
                _logger.LogInformation("Unable to parse 'DssCorrelationId' to a Guid");
                correlationGuid = Guid.NewGuid();
            }
            var touchpointId = _httpRequestHelper.GetDssTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                var response = new BadRequestObjectResult(HttpStatusCode.BadRequest);
                _logger.LogWarning("{CorrelationId} Response Status Code: {StatusCode}. Unable to locate 'TouchpointId' in request header", correlationId, response.StatusCode);
                return response;
            }

            var ApimURL = _httpRequestHelper.GetDssApimUrl(req);
            if (string.IsNullOrEmpty(ApimURL))
            {
                var response = new BadRequestObjectResult(HttpStatusCode.BadRequest);
                _logger.LogWarning("{CorrelationId} Response Status Code: {StatusCode}. Unable to locate 'apimurl' in request header", correlationId, response.StatusCode);
                return response;
            }

            var subcontractorId = _httpRequestHelper.GetDssSubcontractorId(req);
            if (string.IsNullOrEmpty(subcontractorId))
                _logger.LogInformation("{CorrelationId} Unable to locate 'SubcontractorId' in request header", correlationId);

            if (!Guid.TryParse(customerId, out var customerGuid))
            {
                var response = new BadRequestObjectResult(customerGuid);
                _logger.LogWarning("{CorrelationId} Response Status Code: {StatusCode}. Unable to parse 'customerId' to a Guid: {customerId}",correlationId,response.StatusCode,customerId);
                return response;
            }

            if (!Guid.TryParse(interactionId, out var interactionGuid))
            {
                var response = new BadRequestObjectResult(interactionGuid);
                _logger.LogWarning("{CorrelationId} Response Status Code: {StatusCode}. Unable to parse 'interactionId' to a Guid: {interactionId}", correlationId, response.StatusCode,interactionId);
                return response;
            }

            if (!Guid.TryParse(sessionId, out var sessionGuid))
            {
                var response = new BadRequestObjectResult(sessionGuid);
                _logger.LogWarning("{CorrelationId} Response Status Code: {StatusCode}. Unable to parse 'sessionId' to a Guid: {SessionId}", correlationId, response.StatusCode,sessionGuid);
                return response;
            }

            _logger.LogInformation("{CorrelationId} Input validation has succeeded.", correlationId);

            SessionPatch sessionPatchRequest;

            try
            {
                _logger.LogInformation("{CorrelationId} Attempt to get resource from body of the request",correlationId);
                sessionPatchRequest = await _httpRequestHelper.GetResourceFromRequest<SessionPatch>(req);
            }
            catch (Exception ex)
            {
                var response = new UnprocessableEntityObjectResult(_dynamicHelper.ExcludeProperty(ex, ["TargetSite"]));
                _logger.LogError(ex,"{CorrelationId} Response Status Code: {StatusCode}. Unable to retrieve body from req", correlationId, response.StatusCode);
                return response;
            }

            if (sessionPatchRequest == null)
            {
                var response = new UnprocessableEntityObjectResult(req);
                _logger.LogWarning("{CorrelationId} Response Status Code: {StatusCode}. session patch request is null", correlationId, response.StatusCode);
                return response;
            }

            _logger.LogInformation("{CorrelationId} Attempt to set id's for session patch",correlationId);
            sessionPatchRequest.SetIds(touchpointId, subcontractorId);

            _logger.LogInformation("{CorrelationId} Attempt to validate resource",correlationId);
            var errors = _validate.ValidateResource(sessionPatchRequest);

            if (errors != null && errors.Any())
            {
                var response = new UnprocessableEntityObjectResult(errors);
                _logger.LogWarning("{CorrelationId} Response Status Code: {StatusCode}. validation errors with resource {Errors}", correlationId, response.StatusCode, errors);
                return response;
            }

            _logger.LogInformation("{CorrelationId} Attempting to see if customer exists {CustomerId}",correlationId,customerGuid);
            var doesCustomerExist = await _cosmosDbProvider.DoesCustomerResourceExist(customerGuid);

            if (!doesCustomerExist)
            {
                var response = new NoContentResult();
                _logger.LogWarning("{CorrelationId} Response Status Code: {StatusCode}. Customer does not exist {CustomerId}",correlationId,customerGuid);
                return response;
            }
            else
            {
                _logger.LogInformation("{CorrelationId} Customer record found in Cosmos DB {customerGuid}", correlationId, customerGuid);
            }

            _logger.LogInformation("{CorrelationId} Attempting to see if this is a read only customer {CustomerId}",correlationId,customerGuid);
            var isCustomerReadOnly = await _cosmosDbProvider.DoesCustomerHaveATerminationDate(customerGuid);

            if (isCustomerReadOnly)
            {
                var response = new ObjectResult(customerGuid.ToString())
                {
                    StatusCode = (int)HttpStatusCode.Forbidden
                };
                _logger.LogWarning("{CorrelationId} Response Status Code: {StatusCode}. Customer is read only {CustomerId}", correlationId, response.StatusCode,customerGuid);
                return response;
            }

            _logger.LogInformation("{CorrelationId} Attempting to see if interaction exists {InteractionId}",correlationId,interactionGuid);
            var doesInteractionExist = await _cosmosDbProvider.DoesInteractionResourceExistAndBelongToCustomer(interactionGuid, customerGuid);

            if (!doesInteractionExist)
            {
                var response = new NoContentResult();
                _logger.LogWarning("{CorrelationId} Response Status Code: {StatusCode}. Interaction does not exist {InteractionId}",correlationId, response.StatusCode,interactionGuid);
                return response;
            }
            else
            {
                _logger.LogInformation("{CorrelationId} Interaction record with {interactionGuid} found in Cosmos DB for Customer {customerGuid}", correlationId, interactionGuid, customerGuid);
            }

            _logger.LogInformation("{CorrelationId} Attempting to get sessions for customer {CustomerId}",correlationId,customerGuid);
            var sessionForCustomer = await _sessionPatchService.GetSessionForCustomerAsync(customerGuid, sessionGuid);

            if (sessionForCustomer == null)
            {
                var response = new NoContentResult();
                _logger.LogWarning("{CorrelationId} Response Status Code: {StatusCode}. Session does not exist {SessionId}", correlationId, response.StatusCode,sessionGuid);
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
                    _logger.LogError(e,"{CorrelationId} Unable to get long and lat for postcode: {VenuePostCode}",correlationId,sessionPatchRequest.VenuePostCode );
                    throw;
                }

                sessionPatchRequest.SetLongitudeAndLatitude(position);
            }
            else
            {
                _logger.LogInformation("{CorrelationId} Postcode is Null or Empty. Unable to get long and lat.", correlationId);
            }
            _logger.LogInformation("{CorrelationId} Attempting to Patch Session {SessionId}",correlationId,sessionGuid);
            var patchedSession = _sessionPatchService.PatchResource(sessionForCustomer, sessionPatchRequest);

            if (patchedSession == null)
            {
                var response = new NoContentResult();
                _logger.LogWarning("{CorrelationId} Response Status Code: {StatusCode}. Unable to Patch Session {SessionId}", correlationId, response.StatusCode,sessionGuid);
                return response;
            }

            _logger.LogInformation("{CorrelationId} Attempting to update Session {SessionId}",correlationId,sessionGuid);
            var updatedSession = await _sessionPatchService.UpdateCosmosAsync(patchedSession, sessionGuid);

            if (updatedSession == null)
            {
                var response = new BadRequestObjectResult(sessionGuid);
                _logger.LogWarning("{CorrelationId} Response Status Code: {StatusCode}. Failed to patch the session {SessionId}", correlationId, response.StatusCode,sessionGuid);
                _logger.LogInformation("Function {FunctionName} has finished invoking", functionName);
                return response;
            }
            else
            { 
                _logger.LogInformation("{CorrelationId} Attempting to send to service bus {SessionId}",correlationId,sessionGuid);
                await _sessionPatchService.SendToServiceBusQueueAsync(updatedSession, customerGuid, ApimURL);
                var response = new JsonResult(updatedSession, new JsonSerializerOptions())
                {
                    StatusCode = (int)HttpStatusCode.OK
                };
                _logger.LogInformation("{CorrelationId} Response Status Code: {StatusCode}. Successfully patched the session {SessionId}", correlationId, response.StatusCode,sessionGuid);
                _logger.LogInformation("Function {FunctionName} has finished invoking", functionName);
                return response;
            }
        }
    }
}