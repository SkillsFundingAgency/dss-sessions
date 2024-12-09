using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NCS.DSS.Sessions.Cosmos.Provider;
using NCS.DSS.Sessions.GetSessionByIdHttpTrigger.Service;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Net;
using System.Threading.Tasks;
using GetSessionByIdHttpTriggerTest = NCS.DSS.Sessions.GetSessionByIdHttpTrigger.Function.GetSessionByIdHttpTrigger;
namespace NCS.DSS.Sessions.Tests.FunctionTests
{
    [TestFixture]
    public class GetSessionByIdHttpTriggerTests
    {
        private const string ValidCustomerId = "7E467BDB-213F-407A-B86A-1954053D3C24";
        private const string ValidInteractionId = "1e1a555c-9633-4e12-ab28-09ed60d51cb3";
        private const string ValidSessionId = "d5369b9a-6959-4bd3-92fc-1583e72b7e51";
        private const string InValidId = "1111111-2222-3333-4444-555555555555";

        private HttpRequest _request;
        private Mock<ICosmosDBProvider> _cosmosDbProvider;
        private Mock<ILogger<GetSessionByIdHttpTriggerTest>> _logger;
        private Mock<IHttpRequestHelper> _httpRequestHelper;
        private HttpResponseMessageHelper _httpResponseMessageHelper;
        private JsonHelper _jsonHelper;
        private Mock<IGetSessionByIdHttpTriggerService> _getSessionByIdHttpTriggerService;
        private Models.Session _session;
        private GetSessionByIdHttpTriggerTest _function;

        [SetUp]
        public void Setup()
        {
            _session = Substitute.For<Models.Session>();
            _request = new DefaultHttpContext().Request;
            _logger = new Mock<ILogger<GetSessionByIdHttpTriggerTest>>();
            _httpRequestHelper = new Mock<IHttpRequestHelper>();
            _httpResponseMessageHelper = new HttpResponseMessageHelper();
            _jsonHelper = new JsonHelper();
            _cosmosDbProvider = new Mock<ICosmosDBProvider>();
            _getSessionByIdHttpTriggerService = new Mock<IGetSessionByIdHttpTriggerService>();
            _function = new GetSessionByIdHttpTriggerTest(
                _cosmosDbProvider.Object,
                _getSessionByIdHttpTriggerService.Object,
                _logger.Object,
                _httpRequestHelper.Object,
                _httpResponseMessageHelper);

        }

        [Test]
        public async Task GetSessionByIdHttpTrigger_ReturnsStatusCodeBadRequest_WhenTouchpointIdIsNotProvided()
        {
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns((string)null);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidSessionId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task GetSessionByIdHttpTrigger_ReturnsStatusCodeBadRequest_WhenCustomerIdIsInvalid()
        {
            // Arrange 
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");

            // Act
            var result = await RunFunction(InValidId, ValidInteractionId, ValidSessionId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task GetSessionByIdHttpTrigger_ReturnsStatusCodeBadRequest_WhenInteractionIdIsInvalid()
        {
            // Arrange 
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");

            // Act
            var result = await RunFunction(ValidCustomerId, InValidId, ValidSessionId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task GetSessionByIdHttpTrigger_ReturnsStatusCodeBadRequest_WhenSessionIdIsInvalid()
        {
            // Arrange 
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, InValidId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task GetSessionByIdHttpTrigger_ReturnsStatusCodeNoContent_WhenCustomerDoesNotExist()
        {
            // Arrange 
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _cosmosDbProvider.Setup(x => x.DoesCustomerResourceExist(It.IsAny<Guid>())).Returns(Task.FromResult(false));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidSessionId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task GetSessionByIdHttpTrigger_ReturnsStatusCodeNoContent_WhenInteractionDoesNotExist()
        {
            // Arrange 
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _cosmosDbProvider.Setup(x => x.DoesInteractionResourceExistAndBelongToCustomer(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(false));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidSessionId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task GetSessionByIdHttpTrigger_ReturnsStatusCodeNocontent_WhenSessionDoesNotExist()
        {
            // Arrange 
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _getSessionByIdHttpTriggerService.Setup(x => x.GetSessionForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult<Models.Session>(null));
            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidSessionId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task GetSessionByIdHttpTrigger_ReturnsStatusCodeOk_WhenSessionExists()
        {
            // Arrange 
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _getSessionByIdHttpTriggerService.Setup(x => x.GetSessionForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(_session));
            _cosmosDbProvider.Setup(x => x.DoesCustomerResourceExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _cosmosDbProvider.Setup(x => x.DoesInteractionResourceExistAndBelongToCustomer(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(true));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidSessionId);
            var responseResult = result as JsonResult;
            //Assert
            Assert.That(result, Is.InstanceOf<JsonResult>());
            Assert.That(responseResult.StatusCode, Is.EqualTo((int)HttpStatusCode.OK));
        }

        private async Task<IActionResult> RunFunction(string customerId, string interactionId, string sessionId)
        {
            return await _function.RunAsync(
                _request,
                customerId,
                interactionId,
                sessionId).ConfigureAwait(true);
        }

    }
}