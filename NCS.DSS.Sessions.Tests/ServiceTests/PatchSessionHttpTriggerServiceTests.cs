using Microsoft.Azure.Cosmos;
using Moq;
using NCS.DSS.Sessions.Cosmos.Provider;
using NCS.DSS.Sessions.Models;
using NCS.DSS.Sessions.PatchSessionHttpTrigger.Service;
using NCS.DSS.Sessions.ServiceBus;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace NCS.DSS.Sessions.Tests.ServiceTests
{

    [TestFixture]
    public class PatchSessionHttpTriggerServiceTests
    {
        private readonly Guid _sessionId = Guid.Parse("7E467BDB-213F-407A-B86A-1954053D3C24");
        private IPatchSessionHttpTriggerService _sessionPatchHttpTriggerService;
        private Mock<ISessionPatchService> _sessionPatchService;
        private Mock<ICosmosDBProvider> _documentDbProvider;
        private Mock<ISessionsServiceBusClient> _serviceBusClient;
        private Session _session;
        private SessionPatch _sessionPatch;

        private string _json;

        [SetUp]
        public void Setup()
        {
            _documentDbProvider = new Mock<ICosmosDBProvider>();
            _sessionPatchService = new Mock<ISessionPatchService>();
            _serviceBusClient = new Mock<ISessionsServiceBusClient>();
            _sessionPatchHttpTriggerService = new PatchSessionHttpTriggerService(_documentDbProvider.Object, _sessionPatchService.Object,_serviceBusClient.Object);
            _session = new Session();
            _sessionPatch = new SessionPatch() { VenuePostCode = "B33 9BX" };
            _json = JsonSerializer.Serialize(_sessionPatch);
            _sessionPatchService.Setup(x => x.Patch(_json, _sessionPatch)).Returns(_session.ToString());
        }

        [Test]
        public void PatchSessionHttpTriggerServiceTests_PatchResource_ReturnsNullWhenSessionJsonIsNullOrEmpty()
        {
            // Act
            var result = _sessionPatchHttpTriggerService.PatchResource(null, Arg.Any<SessionPatch>());

            // Assert
            Assert.That(result, Is.Null);
        }


        [Test]
        public void PatchSessionHttpTriggerServiceTests_PatchResource_ReturnsNullWhenSessionPatchIsNullOrEmpty()
        {
            // Act
            var result = _sessionPatchHttpTriggerService.PatchResource(Arg.Any<string>(), null);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task PatchSessionHttpTriggerServiceTests_UpdateCosmosAsync_ReturnsNullWhenResourceCannotBeUpdated()
        {
            //Arrange
            _documentDbProvider.Setup(x => x.UpdateSessionAsync(It.IsAny<string>(), It.IsAny<Guid>())).Returns(Task.FromResult<ItemResponse<Session>>(null));

            // Act
            var result = await _sessionPatchHttpTriggerService.UpdateCosmosAsync(_json, _sessionId);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task PatchSessionHttpTriggerServiceTests_UpdateCosmosAsync_ReturnsNullWhenResourceCannotBeFound()
        {
            // Arrange
            _documentDbProvider.Setup(x => x.CreateSessionAsync(_session)).Returns(Task.FromResult<ItemResponse<Session>>(null));

            // Act
            var result = await _sessionPatchHttpTriggerService.UpdateCosmosAsync(_json, _sessionId);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task PatchSessionHttpTriggerServiceTests_UpdateCosmosAsync_ReturnsResourceWhenUpdated()
        {
            // Arrange
            var resourceResponse = new Mock<ItemResponse<Session>>();
            resourceResponse.Setup(x => x.Resource).Returns(_session);
            resourceResponse.Setup(x => x.StatusCode).Returns(HttpStatusCode.OK);
            _documentDbProvider.Setup(x => x.UpdateSessionAsync(It.IsAny<string>(), It.IsAny<Guid>())).Returns(Task.FromResult(resourceResponse.Object));

            // Act
            var result = await _sessionPatchHttpTriggerService.UpdateCosmosAsync(_json, _sessionId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<Session>());

        }

        [Test]
        public async Task PatchSessionHttpTriggerServiceTests_GetActionPlanForCustomerAsync_ReturnsNullWhenResourceHasNotBeenFound()
        {
            // Arrange
            _documentDbProvider.Setup(x => x.GetSessionForCustomerToUpdateAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult<string>(null));

            // Act
            var result = await _sessionPatchHttpTriggerService.GetSessionForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>());

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task PatchSessionHttpTriggerServiceTests_GetActionPlanForCustomerAsync_ReturnsResourceWhenResourceHasBeenFound()
        {
            // Arrange
            _documentDbProvider.Setup(x => x.GetSessionForCustomerToUpdateAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(_json));

            // Act
            var result = await _sessionPatchHttpTriggerService.GetSessionForCustomerAsync(Arg.Any<Guid>(), Arg.Any<Guid>());

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<string>());
        }
    }

}