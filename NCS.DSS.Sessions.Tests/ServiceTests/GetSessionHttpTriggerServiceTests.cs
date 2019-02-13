using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NCS.DSS.Sessions.Cosmos.Provider;
using NCS.DSS.Sessions.GetSessionHttpTrigger.Service;
using NSubstitute;
using NUnit.Framework;

namespace NCS.DSS.Sessions.Tests.ServiceTests
{

    [TestFixture]
    public class GetSessionHttpTriggerServiceTests
    {
        private IGetSessionHttpTriggerService _getSessionHttpTriggerService;
        private IDocumentDBProvider _documentDbProvider;
        private readonly Guid _customerId = Guid.Parse("58b43e3f-4a50-4900-9c82-a14682ee90fa");

        [SetUp]
        public void Setup()
        {
            _documentDbProvider = Substitute.For<IDocumentDBProvider>();
            _getSessionHttpTriggerService = Substitute.For<GetSessionHttpTriggerService>(_documentDbProvider);
        }

        [Test]
        public async Task GetSessionByIdHttpTriggerServiceTests_GetSessionForCustomerAsync_ReturnsNullWhenResourceCannotBeFound()
        {
            _documentDbProvider.GetSessionsForCustomerAsync(Arg.Any<Guid>()).Returns(Task.FromResult<List<Models.Session>>(null).Result);

            // Act
            var result = await _getSessionHttpTriggerService.GetSessionsAsync(_customerId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task GetSessionByIdHttpTriggerServiceTests_GetSessionForCustomerAsync_ReturnsResource()
        {
            _documentDbProvider.GetSessionsForCustomerAsync(_customerId).Returns(Task.FromResult(new List<Models.Session>()).Result);

            // Act
            var result = await _getSessionHttpTriggerService.GetSessionsAsync(_customerId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<List<Models.Session>>(result);
        }
    }
}
