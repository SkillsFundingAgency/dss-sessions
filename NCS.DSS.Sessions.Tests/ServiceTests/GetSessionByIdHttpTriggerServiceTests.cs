using Moq;
using NCS.DSS.Sessions.Cosmos.Provider;
using NCS.DSS.Sessions.GetSessionByIdHttpTrigger.Service;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace NCS.DSS.Sessions.Tests.ServiceTests
{

    [TestFixture]
    public class GetSessionByIdHttpTriggerServiceTests
    {
        private IGetSessionByIdHttpTriggerService _getSessionByIdHttpTriggerService;
        private Mock<ICosmosDBProvider> _cosmosDbProvider;
        private Models.Session _session;
        private readonly Guid _customerId = Guid.Parse("58b43e3f-4a50-4900-9c82-a14682ee90fa");
        private readonly Guid _sessionId = Guid.Parse("7E467BDB-213F-407A-B86A-1954053D3C24");

        [SetUp]
        public void Setup()
        {
            _cosmosDbProvider = new Mock<ICosmosDBProvider>();
            _getSessionByIdHttpTriggerService = new GetSessionByIdHttpTriggerService(_cosmosDbProvider.Object);
            _session = Substitute.For<Models.Session>();
        }

        [Test]
        public async Task GetSessionByIdHttpTriggerServiceTests_GetSessionForCustomerAsyncc_ReturnsNullWhenResourceCannotBeFound()
        {
            // Arrange
            _cosmosDbProvider.Setup(x => x.GetSessionForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult<Models.Session>(null));

            // Act
            var result = await _getSessionByIdHttpTriggerService.GetSessionForCustomerAsync(_customerId, _sessionId);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetSessionByIdHttpTriggerServiceTests_GetSessionForCustomerAsync_ReturnsResource()
        {
            // Arrange
            _cosmosDbProvider.Setup(x => x.GetSessionForCustomerAsync(_customerId, _sessionId)).Returns(Task.FromResult(_session));

            // Act
            var result = await _getSessionByIdHttpTriggerService.GetSessionForCustomerAsync(_customerId, _sessionId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<Models.Session>());
        }
    }
}
