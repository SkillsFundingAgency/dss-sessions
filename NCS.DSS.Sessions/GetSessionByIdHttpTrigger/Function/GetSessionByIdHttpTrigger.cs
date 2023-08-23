using DFC.Common.Standard.Logging;
using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using NCS.DSS.Sessions.Cosmos.Helper;
using NCS.DSS.Sessions.GetSessionByIdHttpTrigger.Service;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace NCS.DSS.Sessions.GetSessionByIdHttpTrigger.Function
{
    public class GetSessionByIdHttpTrigger
    {
        private IResourceHelper _resourceHelper;
        private IGetSessionByIdHttpTriggerService _sessionGetService;
        private ILoggerHelper _loggerHelper;
        private IHttpRequestHelper _httpRequestHelper;
        private IHttpResponseMessageHelper _httpResponseMessageHelper;
        private IJsonHelper _jsonHelper;

        public GetSessionByIdHttpTrigger(
            IResourceHelper resourceHelper,
            IGetSessionByIdHttpTriggerService sessionGetService,
            ILoggerHelper loggerHelper,
            IHttpRequestHelper httpRequestHelper,
            IHttpResponseMessageHelper httpResponseMessageHelper,
            IJsonHelper jsonHelper)
        {
            _resourceHelper = resourceHelper;
            _sessionGetService = sessionGetService;
            _loggerHelper = loggerHelper;
            _httpRequestHelper = httpRequestHelper;
            _httpResponseMessageHelper = httpResponseMessageHelper;
            _jsonHelper = jsonHelper;
        }

        [FunctionName("GETByID")]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Sessions Retrieved", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Resource Does Not Exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Get request is malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API Key unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient Access To This Resource", ShowSchema = false)]
        [ProducesResponseType(typeof(Models.Session), 200)]
        [Display(Name = "GetByID", Description = "Ability to get by ID; a session object for a given customer.")]
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "customers/{customerId}/interactions/{interactionId}/sessions/{sessionId}")] HttpRequest req, ILogger log, string customerId, string interactionId, string sessionId)
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

            log.LogInformation($"DssCorrelationId: [{correlationGuid}]");


            var touchpointId = _httpRequestHelper.GetDssTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                var response =  _httpResponseMessageHelper.BadRequest();
                log.LogWarning($"Response Status Code: [{response.StatusCode}]. Unable to locate 'TouchpointId' in request header");
                return response;
            }

            log.LogInformation($"Get Session By Id C# HTTP trigger function  processed a request. By Touchpoint:[{touchpointId}]");

            if (!Guid.TryParse(customerId, out var customerGuid))
            {
                var response =  _httpResponseMessageHelper.BadRequest(customerGuid);
                log.LogWarning($"Response Status Code: [{response.StatusCode}]. Unable to parse 'customerId' to a Guid: [{customerId}]");
                return response;
            }

            if (!Guid.TryParse(interactionId, out var interactionGuid))
            {
                var response =  _httpResponseMessageHelper.BadRequest(interactionGuid);
                log.LogWarning($"Response Status Code: [{response.StatusCode}]. Unable to parse 'interactionId' to a Guid: [{interactionId}]");
                return response;
            }

            if (!Guid.TryParse(sessionId, out var sessionGuid))
            {
                var response =  _httpResponseMessageHelper.BadRequest(sessionGuid);
                log.LogWarning($"Response Status Code: [{response.StatusCode}]. Unable to parse 'sessionId' to a Guid: [{sessionGuid}]");
                return response;
            }

            log.LogInformation($"Attempting to see if customer exists [{customerGuid}]");
            var doesCustomerExist = await _resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
            {
                var response =  _httpResponseMessageHelper.NoContent(customerGuid);
                log.LogWarning($"Response Status Code: [{response.StatusCode}]. Customer does not exist [{customerGuid}]");
                return response;
            }

            log.LogInformation($"Attempting to see if interaction exists [{interactionGuid}]");
            var doesInteractionExist = _resourceHelper.DoesInteractionResourceExistAndBelongToCustomer(interactionGuid, customerGuid);

            if (!doesInteractionExist)
            {
                var response =  _httpResponseMessageHelper.NoContent(interactionGuid);
                log.LogWarning($"Response Status Code: [{response.StatusCode}]. Interaction does not exist [{interactionGuid}]");
                return response;
            }

            log.LogInformation($"Attempting to get sessions for customer [{customerGuid}]");
            var session = await _sessionGetService.GetSessionForCustomerAsync(customerGuid, sessionGuid);

            if (session == null)
            {
                var response =  _httpResponseMessageHelper.NoContent(sessionGuid);
                log.LogWarning($"Response Status Code: [{response.StatusCode}]. Session does not exist [{sessionGuid}]");
                return response;
            }
            else
            {
                var response =  _httpResponseMessageHelper.Ok(_jsonHelper.SerializeObjectAndRenameIdProperty(session, "id", "SessionId"));
                log.LogInformation($"Response Status Code: [{response.StatusCode}]. Get session succeeded [{sessionGuid}]");
                return response;
            }
        }

    }
}
