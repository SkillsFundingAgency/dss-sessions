using DFC.HTTP.Standard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NCS.DSS.Sessions.Cosmos.Provider;
using NCS.DSS.Sessions.GetSessionHttpTrigger.Service;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace NCS.DSS.Sessions.Tests.FunctionTests
{
    [TestFixture]
    public class GetSessionHttpTriggerTest
    {
        private const string ValidCustomerId = "7E467BDB-213F-407A-B86A-1954053D3C24";
        private const string ValidInteractionId = "1e1a555c-9633-4e12-ab28-09ed60d51cb3";
        private const string InValidId = "1111111-2222-3333-4444-555555555555";

        private HttpRequest _request;
        private Mock<ICosmosDBProvider> _cosmosDbProvider;
        private Mock<IHttpRequestHelper> _httpRequestHelper;
        private IHttpResponseMessageHelper _httpResponseMessageHelper;
        private Mock<IGetSessionHttpTriggerService> _getSessionHttpTriggerService;
        private NCS.DSS.Sessions.GetSessionHttpTrigger.Function.GetSessionHttpTrigger _function;
        private Mock<ILogger<GetSessionHttpTrigger.Function.GetSessionHttpTrigger>> _logger;

        [SetUp]
        public void Setup()
        {
            _request = new DefaultHttpContext().Request;
            _logger = new Mock<ILogger<GetSessionHttpTrigger.Function.GetSessionHttpTrigger>>();
            _cosmosDbProvider = new Mock<ICosmosDBProvider>();
            _httpRequestHelper = new Mock<IHttpRequestHelper>();
            _httpResponseMessageHelper = new HttpResponseMessageHelper();
            _getSessionHttpTriggerService = new Mock<IGetSessionHttpTriggerService>();
            _function = new GetSessionHttpTrigger.Function.GetSessionHttpTrigger(_cosmosDbProvider.Object, _getSessionHttpTriggerService.Object, _httpRequestHelper.Object, _httpResponseMessageHelper, _logger.Object);

        }

        [Test]
        public async Task GetSessionHttpTrigger_ReturnsStatusCodeBadRequest_WhenTouchpointIdIsNotProvided()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns((string)null);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task GetSessionHttpTrigger_ReturnsStatusCodeBadRequest_WhenCustomerIdIsInvalid()
        {

            // Act
            var result = await RunFunction(InValidId, ValidInteractionId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task GetSessionHttpTrigger_ReturnsStatusCodeBadRequest_WhenInteractionIdIsInvalid()
        {
            // Act
            var result = await RunFunction(ValidCustomerId, InValidId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task GetSessionHttpTrigger_ReturnsStatusCodeNoContent_WhenCustomerDoesNotExist()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _cosmosDbProvider.Setup(x => x.DoesCustomerResourceExist(It.IsAny<Guid>())).Returns(Task.FromResult(false));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task GetSessionHttpTrigger_ReturnsStatusCodeNoContent_WhenInteractionDoesNotExist()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _cosmosDbProvider.Setup(x => x.DoesInteractionResourceExistAndBelongToCustomer(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(false));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task GetSessionHttpTrigger_ReturnsStatusCodeNoContent_WhenSessionDoesNotExist()
        {
            // Arrange
            _getSessionHttpTriggerService.Setup(x => x.GetSessionsAsync(It.IsAny<Guid>())).Returns(Task.FromResult<List<Models.Session>>(null));
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task GetSessionHttpTrigger_ReturnsStatusCodeOk_WhenSessionExists()
        {
            // Arrange
            _cosmosDbProvider.Setup(x => x.DoesCustomerResourceExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _getSessionHttpTriggerService.Setup(x => x.GetSessionsAsync(It.IsAny<Guid>())).Returns(Task.FromResult<List<Models.Session>>(new List<Models.Session>()));
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _cosmosDbProvider.Setup(x => x.DoesInteractionResourceExistAndBelongToCustomer(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(true));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId);
            var responseResult = result as JsonResult;
            //Assert
            Assert.That(result, Is.InstanceOf<JsonResult>());
            Assert.That(responseResult.StatusCode, Is.EqualTo((int)HttpStatusCode.OK));
        }

        private async Task<IActionResult> RunFunction(string customerId, string interactionId)
        {
            return await _function.RunAsync(
                _request,
                customerId,
                interactionId
                ).ConfigureAwait(true);
        }
    }
}