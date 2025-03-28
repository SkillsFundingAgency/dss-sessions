﻿using DFC.HTTP.Standard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NCS.DSS.Sessions.Cosmos.Provider;
using NCS.DSS.Sessions.GeoCoding;
using NCS.DSS.Sessions.Helpers;
using NCS.DSS.Sessions.Models;
using NCS.DSS.Sessions.PostSessionHttpTrigger.Service;
using NCS.DSS.Sessions.Validation;
using Newtonsoft.Json;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace NCS.DSS.Sessions.Tests.FunctionTests
{
    [TestFixture]
    public class PostSessionHttpTriggerTests
    {
        private const string ValidCustomerId = "7E467BDB-213F-407A-B86A-1954053D3C24";
        private const string ValidInteractionId = "1e1a555c-9633-4e12-ab28-09ed60d51cb3";
        private const string InValidId = "1111111-2222-3333-4444-555555555555";
        private Mock<ILogger<PostSessionHttpTrigger.Function.PostSessionHttpTrigger>> _logger;
        private HttpRequest _request;
        private ICosmosDBProvider _cosmosDbProvider;
        private IValidate _validate;
        private IHttpRequestHelper _httpRequestHelper;
        private IHttpResponseMessageHelper _httpResponseMessageHelper;
        private IPostSessionHttpTriggerService _postSessionHttpTriggerService;
        private IGeoCodingService _geoCodingService;
        private Models.Session _session;
        private PostSessionHttpTrigger.Function.PostSessionHttpTrigger _function;
        private Mock<IDynamicHelper> _dynamicHelper;

        [SetUp]
        public void Setup()
        {
            _session = Substitute.For<Models.Session>();
            _session.VenuePostCode = string.Empty;

            _request = new DefaultHttpContext().Request;

            _logger = new Mock<ILogger<PostSessionHttpTrigger.Function.PostSessionHttpTrigger>>();
            _cosmosDbProvider = Substitute.For<ICosmosDBProvider>();
            _validate = Substitute.For<IValidate>();
            _httpRequestHelper = Substitute.For<IHttpRequestHelper>();
            _httpResponseMessageHelper = Substitute.For<IHttpResponseMessageHelper>();
            _geoCodingService = Substitute.For<IGeoCodingService>();
            _postSessionHttpTriggerService = Substitute.For<IPostSessionHttpTriggerService>();
            _dynamicHelper = new Mock<IDynamicHelper>();

            _httpRequestHelper.GetDssTouchpointId(_request).Returns("0000000001");
            _httpRequestHelper.GetDssApimUrl(_request).Returns("http://localhost:7071/");
            _httpRequestHelper.GetResourceFromRequest<Session>(_request).Returns(Task.FromResult(_session).Result);

            _cosmosDbProvider.DoesCustomerResourceExist(Arg.Any<Guid>()).ReturnsForAnyArgs(true);
            _cosmosDbProvider.DoesInteractionResourceExistAndBelongToCustomer(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(true);
            _function = new PostSessionHttpTrigger.Function.PostSessionHttpTrigger(_cosmosDbProvider, _validate, _postSessionHttpTriggerService, _logger.Object, _httpRequestHelper, _httpResponseMessageHelper, _geoCodingService, _dynamicHelper.Object);
        }

        [Test]
        public async Task PostSessionHttpTrigger_ReturnsStatusCodeBadRequest_WhenTouchpointIdIsNotProvided()
        {
            _httpRequestHelper.GetDssTouchpointId(_request).Returns((string)null);

            _httpResponseMessageHelper
                .BadRequest().Returns(x => new HttpResponseMessage(HttpStatusCode.BadRequest));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task PostSessionHttpTrigger_ReturnsStatusCodeBadRequest_WhenCustomerIdIsInvalid()
        {
            _httpResponseMessageHelper
                .BadRequest(Arg.Any<Guid>()).Returns(x => new HttpResponseMessage(HttpStatusCode.BadRequest));

            // Act
            var result = await RunFunction(InValidId, ValidInteractionId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task PostSessionHttpTrigger_ReturnsStatusCodeBadRequest_WhenInteractionIdIsInvalid()
        {
            _httpResponseMessageHelper
                .BadRequest(Arg.Any<Guid>()).Returns(x => new HttpResponseMessage(HttpStatusCode.BadRequest));

            // Act
            var result = await RunFunction(ValidCustomerId, InValidId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task PostSessionHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenSessionHasFailedValidation()
        {
            var validationResults = new List<ValidationResult> { new ValidationResult("interaction Id is Required") };
            _validate.ValidateResource(Arg.Any<Models.Session>()).Returns(validationResults);

            _httpResponseMessageHelper.UnprocessableEntity(Arg.Any<List<ValidationResult>>())
                .Returns(x => new HttpResponseMessage((HttpStatusCode)422));

            var result = await RunFunction(ValidCustomerId, ValidInteractionId);

            // Assert
            Assert.That(result, Is.InstanceOf<UnprocessableEntityObjectResult>());
        }

        [Test]
        public async Task PostSessionHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenSessionRequestIsInvalid()
        {
            _httpRequestHelper.GetResourceFromRequest<Session>(_request).ThrowsForAnyArgs(new JsonException());

            _httpResponseMessageHelper
                .UnprocessableEntity(Arg.Any<JsonException>()).Returns(x => new HttpResponseMessage((HttpStatusCode)422));

            var result = await RunFunction(ValidCustomerId, ValidInteractionId);

            // Assert
            Assert.That(result, Is.InstanceOf<UnprocessableEntityObjectResult>());
        }

        [Test]
        public async Task PostSessionHttpTrigger_ReturnsStatusCodeNoContent_WhenCustomerDoesNotExist()
        {
            _cosmosDbProvider.DoesCustomerResourceExist(Arg.Any<Guid>()).Returns(false);

            _httpResponseMessageHelper
                .NoContent(Arg.Any<Guid>()).Returns(x => new HttpResponseMessage(HttpStatusCode.NoContent));

            var result = await RunFunction(ValidCustomerId, ValidInteractionId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task PostSessionHttpTrigger_ReturnsStatusCodeNoContent_WhenInteractionDoesNotExist()
        {
            _cosmosDbProvider.DoesInteractionResourceExistAndBelongToCustomer(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(false);

            _httpResponseMessageHelper
                .NoContent(Arg.Any<Guid>()).Returns(x => new HttpResponseMessage(HttpStatusCode.NoContent));

            var result = await RunFunction(ValidCustomerId, ValidInteractionId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task PostSessionHttpTrigger_ReturnsStatusCodeBadRequest_WhenUnableToCreateSessionRecord()
        {
            _postSessionHttpTriggerService.CreateAsync(Arg.Any<Models.Session>()).Returns(Task.FromResult<Models.Session>(null).Result);

            _httpResponseMessageHelper
                .BadRequest(Arg.Any<Guid>()).Returns(x => new HttpResponseMessage(HttpStatusCode.BadRequest));

            var result = await RunFunction(ValidCustomerId, ValidInteractionId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task PostSessionHttpTrigger_ReturnsStatusCodeBadRequest_WhenRequestIsInValid()
        {
            _postSessionHttpTriggerService.CreateAsync(Arg.Any<Models.Session>()).Returns(Task.FromResult<Models.Session>(null).Result);

            _httpResponseMessageHelper
                .BadRequest(Arg.Any<Guid>()).Returns(x => new HttpResponseMessage(HttpStatusCode.BadRequest));

            var result = await RunFunction(ValidCustomerId, ValidInteractionId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task PostSessionHttpTrigger_ReturnsStatusCodeCreated_WhenRequestIsValid()
        {
            _postSessionHttpTriggerService.CreateAsync(Arg.Any<Models.Session>()).Returns(Task.FromResult<Models.Session>(_session).Result);

            _httpResponseMessageHelper
                .Created(Arg.Any<string>()).Returns(x => new HttpResponseMessage(HttpStatusCode.Created));

            var result = await RunFunction(ValidCustomerId, ValidInteractionId);
            var responseResult = result as JsonResult;
            //Assert
            Assert.That(result, Is.InstanceOf<JsonResult>());
            Assert.That(responseResult.StatusCode, Is.EqualTo((int)HttpStatusCode.Created));
        }

        private async Task<IActionResult> RunFunction(string customerId, string interactionId)
        {
            return await _function.Run(
                _request,
                customerId,
                interactionId).ConfigureAwait(false);
        }
    }
}