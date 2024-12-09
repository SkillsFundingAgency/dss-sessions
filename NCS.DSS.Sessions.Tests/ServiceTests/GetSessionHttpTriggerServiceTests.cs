using Moq;
using NCS.DSS.Sessions.Cosmos.Provider;
using NCS.DSS.Sessions.GetSessionHttpTrigger.Service;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NCS.DSS.Sessions.Tests.ServiceTests
{

    [TestFixture]
    public class GetSessionHttpTriggerServiceTests
    {
        private IGetSessionHttpTriggerService _getSessionHttpTriggerService;
        private Mock<ICosmosDBProvider> _documentDbProvider;
        private readonly Guid _customerId = Guid.Parse("58b43e3f-4a50-4900-9c82-a14682ee90fa");

        [SetUp]
        public void Setup()
        {
            _documentDbProvider = new Mock<ICosmosDBProvider>();
            _getSessionHttpTriggerService = Substitute.For<GetSessionHttpTriggerService>(_documentDbProvider.Object);
        }

        [Test]
        public async Task GetSessionByIdHttpTriggerServiceTests_GetSessionForCustomerAsync_ReturnsNullWhenResourceCannotBeFound()
        {
            //Arrange
            _documentDbProvider.Setup(x => x.GetSessionsForCustomerAsync(It.IsAny<Guid>())).Returns(Task.FromResult<List<Models.Session>>(null));

            // Act
            var result = await _getSessionHttpTriggerService.GetSessionsAsync(_customerId);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetSessionByIdHttpTriggerServiceTests_GetSessionForCustomerAsync_ReturnsResource()
        {
            //Arrange
            _documentDbProvider.Setup(x => x.GetSessionsForCustomerAsync(_customerId)).Returns(Task.FromResult(new List<Models.Session>()));

            // Act
            var result = await _getSessionHttpTriggerService.GetSessionsAsync(_customerId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<List<Models.Session>>());
        }
    }
}
