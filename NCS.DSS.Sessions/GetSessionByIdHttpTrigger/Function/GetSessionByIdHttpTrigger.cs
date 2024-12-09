using DFC.HTTP.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.Sessions.Cosmos.Provider;
using NCS.DSS.Sessions.GetSessionByIdHttpTrigger.Service;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;

namespace NCS.DSS.Sessions.GetSessionByIdHttpTrigger.Function
{
    public class GetSessionByIdHttpTrigger
    {
        private IGetSessionByIdHttpTriggerService _sessionGetService;
        private ICosmosDBProvider _cosmosDbProvider;
        private ILogger<GetSessionByIdHttpTrigger> _logger;
        private IHttpRequestHelper _httpRequestHelper;
        private IHttpResponseMessageHelper _httpResponseMessageHelper;

        public GetSessionByIdHttpTrigger(
            ICosmosDBProvider cosmosDbProvider,
            IGetSessionByIdHttpTriggerService sessionGetService,
            ILogger<GetSessionByIdHttpTrigger> logger,
            IHttpRequestHelper httpRequestHelper,
            IHttpResponseMessageHelper httpResponseMessageHelper)
        {
            _cosmosDbProvider = cosmosDbProvider;
            _sessionGetService = sessionGetService;
            _logger = logger;
            _httpRequestHelper = httpRequestHelper;
            _httpResponseMessageHelper = httpResponseMessageHelper;
        }

        [Function("GETByID")]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Sessions Retrieved", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Resource Does Not Exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Get request is malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API Key unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient Access To This Resource", ShowSchema = false)]
        [ProducesResponseType(typeof(Models.Session), 200)]
        [Display(Name = "GetByID", Description = "Ability to get by ID; a session object for a given customer.")]
        public async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "customers/{customerId}/interactions/{interactionId}/sessions/{sessionId}")] HttpRequest req, string customerId, string interactionId, string sessionId)
        {
            var functionName = nameof(GetSessionByIdHttpTrigger);

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

            _logger.LogInformation("{CorrelationId} Get Session By Id C# HTTP trigger function  processed a request. By Touchpoint:[{touchpointId}]",correlationId,touchpointId);

            if (!Guid.TryParse(customerId, out var customerGuid))
            {
                var response = new BadRequestObjectResult(customerGuid);
                _logger.LogWarning("{CorrelationId} Response Status Code: {StatusCode}. Unable to parse 'customerId' to a Guid: [{customerId}]", correlationId, customerId);
                return response;
            }

            if (!Guid.TryParse(interactionId, out var interactionGuid))
            {
                var response = new BadRequestObjectResult(interactionGuid);
                _logger.LogWarning("{CorrelationId} Response Status Code: {StatusCode}. Unable to parse 'interactionId' to a Guid: [{interactionId}]", correlationId, response.StatusCode, interactionId);
                return response;
            }

            if (!Guid.TryParse(sessionId, out var sessionGuid))
            {
                var response = new BadRequestObjectResult(sessionGuid);
                _logger.LogWarning("{CorrelationId} Response Status Code: {StatusCode}. Unable to parse 'sessionId' to a Guid: [{sessionGuid}]", correlationId, response.StatusCode, sessionGuid);
                return response;
            }

            _logger.LogInformation("{CorrelationId} Attempting to see if customer exists [{customerGuid}]", correlationId, customerGuid);
            var doesCustomerExist = await _cosmosDbProvider.DoesCustomerResourceExist(customerGuid);

            if (!doesCustomerExist)
            {
                var response = new NoContentResult();
                _logger.LogWarning("{CorrelationId} Response Status Code: {StatusCode}. Customer does not exist [{customerGuid}]", correlationId, response.StatusCode, customerGuid);
                return response;
            }

            _logger.LogInformation("{CorrelationId} Attempting to see if interaction exists [{interactionGuid}]", correlationId, interactionGuid);
            var doesInteractionExist = await _cosmosDbProvider.DoesInteractionResourceExistAndBelongToCustomer(interactionGuid, customerGuid);

            if (!doesInteractionExist)
            {
                var response = new NoContentResult();
                _logger.LogWarning("{CorrelationId} Response Status Code: {StatusCode}. Interaction does not exist [{interactionGuid}]", correlationId, response.StatusCode, interactionGuid);
                return response;
            }

            _logger.LogInformation("{CorrelationId} Attempting to get sessions for customer [{customerGuid}]", correlationId, customerGuid);
            var session = await _sessionGetService.GetSessionForCustomerAsync(customerGuid, sessionGuid);

            if (session == null)
            {
                var response = new NoContentResult();
                _logger.LogWarning("{CorrelationId} Response Status Code: {StatusCode}. Session does not exist [{sessionGuid}]", correlationId, response.StatusCode, sessionGuid);
                _logger.LogInformation("Function {FunctionName} has finished invoking", functionName);
                return response;
            }
            else
            {
                var response = new JsonResult(session, new JsonSerializerOptions())
                {
                    StatusCode = (int)HttpStatusCode.OK
                };
                _logger.LogInformation("{CorrelationId} Response Status Code: {StatusCode}. Get session succeeded [{sessionGuid}]",correlationId,response.StatusCode,sessionGuid);
                _logger.LogInformation("Function {FunctionName} has finished invoking", functionName);
                return response;
            }
        }

    }
}
