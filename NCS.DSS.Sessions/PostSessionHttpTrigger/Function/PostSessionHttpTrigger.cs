using DFC.GeoCoding.Standard.AzureMaps.Model;
using DFC.HTTP.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.Sessions.Cosmos.Provider;
using NCS.DSS.Sessions.GeoCoding;
using NCS.DSS.Sessions.Helpers;
using NCS.DSS.Sessions.Models;
using NCS.DSS.Sessions.PostSessionHttpTrigger.Service;
using NCS.DSS.Sessions.Validation;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;

namespace NCS.DSS.Sessions.PostSessionHttpTrigger.Function
{
    public class PostSessionHttpTrigger
    {
        private ICosmosDBProvider _cosmosDbProvider;
        private IValidate _validate;
        private IPostSessionHttpTriggerService _sessionPostService;
        private ILogger<PostSessionHttpTrigger> _logger;
        private IHttpRequestHelper _httpRequestHelper;
        private IHttpResponseMessageHelper _httpResponseMessageHelper;
        private IGeoCodingService _geoCodingService;
        private IDynamicHelper _dynamicHelper;

        public PostSessionHttpTrigger(ICosmosDBProvider cosomsDbProvider,
            IValidate validate,
            IPostSessionHttpTriggerService sessionPostService,
            ILogger<PostSessionHttpTrigger> logger,
            IHttpRequestHelper httpRequestHelper,
            IHttpResponseMessageHelper httpResponseMessageHelper,
            IGeoCodingService geoCodingService,
            IDynamicHelper dynamicHelper)
        {
            _cosmosDbProvider = cosomsDbProvider;
            _validate = validate;
            _sessionPostService = sessionPostService;
            _logger = logger;
            _httpRequestHelper = httpRequestHelper;
            _httpResponseMessageHelper = httpResponseMessageHelper;
            _geoCodingService = geoCodingService;
            _dynamicHelper = dynamicHelper;
        }

        [Function("POST")]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Sessions Added", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Resource Does Not Exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Post request is malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API Key unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient Access To This Resource", ShowSchema = false)]
        [Response(HttpStatusCode = 422, Description = "Sessions resource validation error(s)", ShowSchema = false)]
        [ProducesResponseType(typeof(Models.Session), 201)]
        [Display(Name = "Post", Description = "Ability to add a session object for a given customer.")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "customers/{customerId}/interactions/{interactionId}/sessions/")] HttpRequest req, string customerId, string interactionId)

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
                _logger.LogInformation("{CorrelationId} Response Status Code: {StatusCode}. Unable to locate 'TouchpointId' in request header", correlationId, response.StatusCode);
                return response;
            }

            var ApimURL = _httpRequestHelper.GetDssApimUrl(req);
            if (string.IsNullOrEmpty(ApimURL))
            {
                var response = new BadRequestObjectResult(HttpStatusCode.BadRequest);
                _logger.LogWarning("{CorrelationId} Response Status Code: {StatusCode}. Unable to locate 'apimurl' in request header",correlationId,response.StatusCode);
                return response;
            }

            var subcontractorId = _httpRequestHelper.GetDssSubcontractorId(req);
            if (string.IsNullOrEmpty(subcontractorId))
                _logger.LogWarning("{CorrelationId} Unable to locate 'SubcontractorId' in request header",correlationId);

            _logger.LogInformation("Header validation has succeeded. Touchpoint ID: {TouchpointId}", touchpointId);

            if (!Guid.TryParse(customerId, out var customerGuid))
            {
                var response = new BadRequestObjectResult(customerGuid);
                _logger.LogWarning("{CorrelationId} Response Status Code: {StatusCode}. Unable to parse 'customerId' to a Guid: {customerId}", correlationId, response.StatusCode,customerGuid);
                return response;
            }

            if (!Guid.TryParse(interactionId, out var interactionGuid))
            {
                var response = new BadRequestObjectResult(interactionGuid);
                _logger.LogWarning("{CorrelationId} Response Status Code: {StatusCode}. Unable to parse 'interactionId' to a Guid: {interactionId}", correlationId, response.StatusCode,interactionId);
                return response;
            }

            Session sessionRequest;

            try
            {
                _logger.LogInformation("{CorrelationId} Attempt to get resource from body of the request",correlationId);
                sessionRequest = await _httpRequestHelper.GetResourceFromRequest<Session>(req);
            }
            catch (Exception ex)
            {
                var response = new UnprocessableEntityObjectResult(_dynamicHelper.ExcludeProperty(ex, ["TargetSite"]));
                _logger.LogError(ex,"{CorrelationId} Response Status Code: {StatusCode}. Unable to retrieve body from req {Exception}", correlationId, response.StatusCode, ex.Message);
                return response;
            }

            if (sessionRequest == null)
            {
                var response = new UnprocessableEntityObjectResult(req);
                _logger.LogWarning("{CorrelationId} Response Status Code: {StatusCode}. session request is null", correlationId, response.StatusCode);
                return response;
            }

            _logger.LogInformation("{CorrelationId} Attempt to set id's for session patch",correlationId);
            sessionRequest.SetIds(customerGuid, interactionGuid, touchpointId, subcontractorId);

            _logger.LogInformation("{CorrelationId} Attempt to validate resource",correlationId);
            var errors = _validate.ValidateResource(sessionRequest);

            if (errors != null && errors.Any())
            {
                var response = new UnprocessableEntityObjectResult(errors);
                _logger.LogWarning("{CorrelationId} Response Status Code: {StatusCode}. validation errors with resource", correlationId, response.StatusCode, errors);
                return response;
            }

            _logger.LogInformation("{CorrelationId} Attempting to see if customer exists {customerGuid}", correlationId, customerGuid);
            var doesCustomerExist = await _cosmosDbProvider.DoesCustomerResourceExist(customerGuid);

            if (!doesCustomerExist)
            {
                var response = new NoContentResult();
                _logger.LogWarning("{CorrelationId} Response Status Code: {StatusCode}. Customer does not exist {customerGuid}", correlationId, customerGuid);
                return response;
            }
            else
            {
                _logger.LogInformation("{CorrelationId} Customer record found in Cosmos DB {customerGuid}", correlationId, customerGuid);                
            }

            _logger.LogInformation("{CorrelationId} Attempting to see if this is a read only customer {customerGuid}", correlationId, customerGuid);
            var isCustomerReadOnly = await _cosmosDbProvider.DoesCustomerHaveATerminationDate(customerGuid);

            if (isCustomerReadOnly)
            {
                var response = new ObjectResult(customerGuid.ToString())
                {
                    StatusCode = (int)HttpStatusCode.Forbidden
                };
                _logger.LogWarning("{CorrelationId} Response Status Code: {StatusCode}. Customer is read only {customerGuid}", correlationId, response.StatusCode,interactionGuid);
                return response;
            }

            _logger.LogInformation("{CorrelationId} Attempting to see if interaction exists {interactionGuid}", correlationId, interactionGuid);
            var doesInteractionExist = await _cosmosDbProvider.DoesInteractionResourceExistAndBelongToCustomer(interactionGuid, customerGuid);

            if (!doesInteractionExist)
            {
                var response = new NoContentResult();
                _logger.LogWarning("{CorrelationId} Response Status Code: {StatusCode}. Interaction does not exist {interactionGuid}", correlationId, response.StatusCode,interactionGuid);
                return response;
            }
            else
            {
                _logger.LogInformation("{CorrelationId} Interaction record with {interactionGuid} found in Cosmos DB for Customer {customerGuid}", correlationId, interactionGuid, customerGuid);
            }
            _logger.LogInformation("{CorrelationId} Attempting to get long and lat for postcode",correlationId);
            if (!string.IsNullOrEmpty(sessionRequest.VenuePostCode))
            {
                Position position;
                try
                {
                    var postcode = sessionRequest.VenuePostCode.Replace(" ", string.Empty);
                    position = await _geoCodingService.GetPositionForPostcodeAsync(postcode);
                }
                catch (Exception e)
                {
                    _logger.LogError(e,"{CorrelationId} Unable to get long and lat for postcode: {VenuePostCode}", correlationId, sessionRequest.VenuePostCode);
                    throw;
                }

                sessionRequest.SetLongitudeAndLatitude(position);
            }
            else
            {
                _logger.LogInformation("{CorrelationId} Postcode is Null or Empty. Unable to get long and lat.", correlationId);
            }
            _logger.LogInformation("{CorrelationId} Attempting to Create session for customer {customerGuid}", correlationId, customerGuid);
            var session = await _sessionPostService.CreateAsync(sessionRequest);

            if (session == null)
            {
                var response = new BadRequestObjectResult(customerGuid);
                _logger.LogWarning("{CorrelationId} Response Status Code: {StatusCode}. Failed to post a session for customer {customerGuid}", correlationId, response.StatusCode);
                _logger.LogInformation("Function {FunctionName} has finished invoking", functionName);
                return response;
            }
            else
            {
                _logger.LogInformation("{CorrelationId} Attempting to send to service bus {SessionId}", correlationId, session.SessionId);
                await _sessionPostService.SendToServiceBusQueueAsync(session, ApimURL);
                var response = new JsonResult(session, new JsonSerializerOptions())
                {
                    StatusCode = (int)HttpStatusCode.Created
                };
                _logger.LogInformation("{CorrelationId} Response Status Code: {StatusCode}. Successfully posted a session {SessionId} for customer {customerGuid}",correlationId,response.StatusCode,session.SessionId,customerGuid);
                _logger.LogInformation("Function {FunctionName} has finished invoking", functionName);
                return response;
            }
        }

    }
}