using DFC.HTTP.Standard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NCS.DSS.Sessions.Cosmos.Provider;
using NCS.DSS.Sessions.GeoCoding;
using NCS.DSS.Sessions.Helpers;
using NCS.DSS.Sessions.Models;
using NCS.DSS.Sessions.PatchSessionHttpTrigger.Service;
using NCS.DSS.Sessions.Validation;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Net;
using System.Threading.Tasks;

namespace NCS.DSS.Sessions.Tests.FunctionTests
{
    [TestFixture]
    public class PatchSessionHttpTriggerTests
    {
        private const string ValidCustomerId = "7E467BDB-213F-407A-B86A-1954053D3C24";
        private const string ValidInteractionId = "1e1a555c-9633-4e12-ab28-09ed60d51cb3";
        private const string InValidId = "1111111-2222-3333-4444-555555555555";
        private const string ValidSessionId = "d5369b9a-6959-4bd3-92fc-1583e72b7e51";
        private Mock<ILogger<PatchSessionHttpTrigger.Function.PatchSessionHttpTrigger>> _logger;
        private HttpRequest _request;
        private Mock<ICosmosDBProvider> _cosmosDbProvider;
        private IValidate _validate;
        private Mock<IHttpRequestHelper> _httpRequestHelper;
        private IHttpResponseMessageHelper _httpResponseMessageHelper;
        private Mock<IPatchSessionHttpTriggerService> _patchSessionHttpTriggerService;
        private Mock<IGeoCodingService> _geoCodingService;
        private Session _session;
        private SessionPatch _sessionPatch;
        private string _sessionString;
        private PatchSessionHttpTrigger.Function.PatchSessionHttpTrigger _function;
        private Mock<IDynamicHelper> _dynamicHelper;

        [SetUp]
        public void Setup()
        {
            _session = new Session() { VenuePostCode = "B33 9BX" };
            _sessionPatch = new SessionPatch() { VenuePostCode = "B33 9BX" };

            _request = new DefaultHttpContext().Request;

            _logger = new Mock<ILogger<PatchSessionHttpTrigger.Function.PatchSessionHttpTrigger>>();
            _cosmosDbProvider = new Mock<ICosmosDBProvider>();
            _httpRequestHelper = new Mock<IHttpRequestHelper>();
            _httpResponseMessageHelper = new HttpResponseMessageHelper();
            _validate = new Validate();
            _patchSessionHttpTriggerService = new Mock<IPatchSessionHttpTriggerService>();
            _geoCodingService = new Mock<IGeoCodingService>();
            _sessionString = JsonConvert.SerializeObject(_session);
            _dynamicHelper = new Mock<IDynamicHelper>();
            _function = new PatchSessionHttpTrigger.Function.PatchSessionHttpTrigger(
                _cosmosDbProvider.Object,
                _validate,
                _patchSessionHttpTriggerService.Object,
                _logger.Object,
                _httpRequestHelper.Object,
                _httpResponseMessageHelper,
                _geoCodingService.Object,
                _dynamicHelper.Object);
        }

        [Test]
        public async Task PatchSessionHttpTrigger_ReturnsStatusCodeBadRequest_WhenTouchpointIdIsNotProvided()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns((string)null);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidSessionId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }


        [Test]
        public async Task PatchSessionHttpTrigger_ReturnsStatusCodeBadRequest_WhenCustomerIdIsInvalid()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");

            // Act
            var result = await RunFunction(InValidId, ValidInteractionId, ValidSessionId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task PatchSessionHttpTrigger_ReturnsStatusCodeBadRequest_WhenInteractionIdIsInvalid()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("someurl");

            // Act
            var result = await RunFunction(ValidCustomerId, InValidId, ValidSessionId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task PatchSessionHttpTrigger_ReturnsStatusCodeBadRequest_WhenSessionIdIsInvalid()
        {

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, InValidId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task PatchSessionHttpTrigger_ReturnsStatusCodeNoContent_WhenSessionPatchCantBePatched()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("someurl");
            _cosmosDbProvider.Setup(x => x.DoesCustomerResourceExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<SessionPatch>(_request)).Returns(Task.FromResult(_sessionPatch));
            _cosmosDbProvider.Setup(x => x.DoesInteractionResourceExistAndBelongToCustomer(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _patchSessionHttpTriggerService.Setup(x => x.GetSessionForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult("SOME SESSION"));
            _patchSessionHttpTriggerService.Setup(x => x.PatchResource(It.IsAny<string>(), It.IsAny<SessionPatch>())).Returns<string>(null);
            _patchSessionHttpTriggerService.Setup(x => x.UpdateCosmosAsync(It.IsAny<string>(), It.IsAny<Guid>())).Returns(Task.FromResult<Session>(null));


            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidSessionId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task PatchSessionHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenSessionRequestIsInvalid()
        {
            //Arrange 
            _session.VenuePostCode = "sdfsdfsdfds";
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("someurl");
            _cosmosDbProvider.Setup(x => x.DoesCustomerResourceExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<SessionPatch>(_request)).Throws(new JsonException());
            _cosmosDbProvider.Setup(x => x.DoesInteractionResourceExistAndBelongToCustomer(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _patchSessionHttpTriggerService.Setup(x => x.GetSessionForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult("SOME SESSION"));
            _patchSessionHttpTriggerService.Setup(x => x.PatchResource(It.IsAny<string>(), It.IsAny<SessionPatch>())).Returns("SOME STRING");
            _patchSessionHttpTriggerService.Setup(x => x.UpdateCosmosAsync(It.IsAny<string>(), It.IsAny<Guid>())).Returns(Task.FromResult<Session>(null));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidSessionId);

            // Assert
            Assert.That(result, Is.InstanceOf<UnprocessableEntityObjectResult>());
        }

        [Test]
        public async Task PatchSessionHttpTrigger_ReturnsStatusCodeNoContent_WhenCustomerDoesNotExist()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("someurl");
            _cosmosDbProvider.Setup(x => x.DoesCustomerResourceExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<SessionPatch>(_request)).Returns(Task.FromResult(_sessionPatch));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidSessionId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task PatchSessionHttpTrigger_ReturnsStatusCodeNoContent_WhenSessionDoesNotExist()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("someurl");
            _cosmosDbProvider.Setup(x => x.DoesCustomerResourceExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<SessionPatch>(_request)).Returns(Task.FromResult(_sessionPatch));
            _cosmosDbProvider.Setup(x => x.DoesInteractionResourceExistAndBelongToCustomer(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(true));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidSessionId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task PatchSessionHttpTrigger_ReturnsStatusCodeNoContent_WhenInteractionDoesNotExist()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("someurl");
            _cosmosDbProvider.Setup(x => x.DoesCustomerResourceExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<SessionPatch>(_request)).Returns(Task.FromResult(_sessionPatch));
            _cosmosDbProvider.Setup(x => x.DoesInteractionResourceExistAndBelongToCustomer(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(false));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidSessionId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task PatchSessionHttpTrigger_ReturnsStatusCodeBadRequest_WhenUnableToUpdateSessionRecord()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("someurl");
            _cosmosDbProvider.Setup(x => x.DoesCustomerResourceExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<SessionPatch>(_request)).Returns(Task.FromResult(_sessionPatch));
            _cosmosDbProvider.Setup(x => x.DoesInteractionResourceExistAndBelongToCustomer(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _patchSessionHttpTriggerService.Setup(x => x.GetSessionForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult("SOME SESSION"));
            _patchSessionHttpTriggerService.Setup(x => x.PatchResource(It.IsAny<string>(), It.IsAny<SessionPatch>())).Returns("SOME STRING");
            _patchSessionHttpTriggerService.Setup(x => x.UpdateCosmosAsync(It.IsAny<string>(), It.IsAny<Guid>())).Returns(Task.FromResult<Session>(null));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidSessionId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task PatchSessionHttpTrigger_ReturnsStatusCodeBadRequest_WhenRequestIsNotValid()
        {
            //_patchSessionHttpTriggerService.GetSessionForCustomerAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(Task.FromResult(_sessionString).Result);

            //_patchSessionHttpTriggerService.UpdateCosmosAsync(Arg.Any<string>(), Arg.Any<Guid>()).Returns(Task.FromResult<Session>(null).Result);


            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidSessionId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task PatchSessionHttpTrigger_ReturnsStatusCodeOK_WhenRequestIsValid()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("someurl");
            _cosmosDbProvider.Setup(x => x.DoesCustomerResourceExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<SessionPatch>(_request)).Returns(Task.FromResult(_sessionPatch));
            _cosmosDbProvider.Setup(x => x.DoesInteractionResourceExistAndBelongToCustomer(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _patchSessionHttpTriggerService.Setup(x => x.GetSessionForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult("SOME SESSION"));
            _patchSessionHttpTriggerService.Setup(x => x.PatchResource(It.IsAny<string>(), It.IsAny<SessionPatch>())).Returns("SOME STRING");
            _patchSessionHttpTriggerService.Setup(x => x.UpdateCosmosAsync(It.IsAny<string>(), It.IsAny<Guid>())).Returns(Task.FromResult<Session>(_session));

            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidSessionId);
            var responseResult = result as JsonResult;
            //Assert
            Assert.That(result, Is.InstanceOf<JsonResult>());
            Assert.That(responseResult.StatusCode, Is.EqualTo((int)HttpStatusCode.OK));
        }

        private async Task<IActionResult> RunFunction(string customerId, string interactionId, string sessionId)
        {
            return await _function.Run(
                _request,
                customerId,
                interactionId,
                sessionId).ConfigureAwait(true);
        }

    }
}