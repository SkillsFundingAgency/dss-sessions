﻿using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net;
using System;
using System.Web.Http.Description;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using NCS.DSS.Sessions.Annotations;
using NCS.DSS.Sessions.Cosmos.Helper;
using NCS.DSS.Sessions.Helpers;
using NCS.DSS.Sessions.Ioc;
using NCS.DSS.Sessions.Models;
using NCS.DSS.Sessions.Validation;

namespace NCS.DSS.Sessions.PostSessionHttpTrigger.Function
{
    public static class PostSessionHttpTrigger
    {
        [FunctionName("POST")]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Sessions Added", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Resource Does Not Exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Post request is malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API Key unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient Access To This Resource", ShowSchema = false)]
        [Response(HttpStatusCode = 422, Description = "Sessions resource validation error(s)", ShowSchema = false)]
        [ResponseType(typeof(Session))]
        [Display(Name = "Post", Description = "Ability to add a session object for a given customer.")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "customers/{customerId}/interactions/{interactionId}/sessions/")]HttpRequestMessage req, TraceWriter log, string customerId, string interactionId,
            [Inject]IResourceHelper resourceHelper,
            [Inject]IHttpRequestMessageHelper httpRequestMessageHelper,
            [Inject]IValidate validate,
            [Inject]IPostSessionHttpTriggerService sessionPostService)
        {
            log.Info("C# HTTP trigger function Post Session processed a request.");

            if (!Guid.TryParse(customerId, out var customerGuid))
                return HttpResponseMessageHelper.BadRequest(customerGuid);

            if (!Guid.TryParse(interactionId, out var interactionGuid))
                return HttpResponseMessageHelper.BadRequest(interactionGuid);
            
            Session sessionRequest;

            try
            {
                sessionRequest = await httpRequestMessageHelper.GetSessionFromRequest<Session>(req);
            }
            catch (JsonSerializationException ex)
            {
                return HttpResponseMessageHelper.UnprocessableEntity(ex);
            }

            if (sessionRequest == null)
                return HttpResponseMessageHelper.UnprocessableEntity(req);

            var errors = validate.ValidateResource(sessionRequest);

            if (errors != null && errors.Any())
                return HttpResponseMessageHelper.UnprocessableEntity(errors);

            var doesCustomerExist = resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
                return HttpResponseMessageHelper.NoContent(customerGuid);

            var doesInteractionExist = resourceHelper.DoesInteractionExist(interactionGuid);

            if (!doesInteractionExist)
                return HttpResponseMessageHelper.NoContent(interactionGuid);

            var session = await sessionPostService.CreateAsync(sessionRequest);

            return session == null
                ? HttpResponseMessageHelper.BadRequest(customerGuid)
                : HttpResponseMessageHelper.Created(session);
        }

    }
}