using System;
using System.Threading.Tasks;
using NCS.DSS.Sessions.Cosmos.Provider;
using NCS.DSS.Sessions.GetSessionByIdHttpTrigger.Service;
using NSubstitute;
using NUnit.Framework;

namespace NCS.DSS.Sessions.Tests.ServiceTests
{

    [TestFixture]
    public class GetSessionByIdHttpTriggerServiceTests
    {
        private IGetSessionByIdHttpTriggerService _getSessionByIdHttpTriggerService;
        private IDocumentDBProvider _documentDbProvider;
        private Models.Session _session;
        private readonly Guid _customerId = Guid.Parse("58b43e3f-4a50-4900-9c82-a14682ee90fa");
        private readonly Guid _sessionId = Guid.Parse("7E467BDB-213F-407A-B86A-1954053D3C24");

        [SetUp]
        public void Setup()
        {
            _documentDbProvider = Substitute.For<IDocumentDBProvider>();
            _getSessionByIdHttpTriggerService = Substitute.For<GetSessionByIdHttpTriggerService>(_documentDbProvider);
            _session = Substitute.For<Models.Session>();
        }

        [Test]
        public async Task GetSessionByIdHttpTriggerServiceTests_GetSessionForCustomerAsyncc_ReturnsNullWhenResourceCannotBeFound()
        {
            _documentDbProvider.GetSessionForCustomerAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(Task.FromResult<Models.Session>(null).Result);

            // Act
            var result = await _getSessionByIdHttpTriggerService.GetSessionForCustomerAsync(_customerId, _sessionId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task GetSessionByIdHttpTriggerServiceTests_GetSessionForCustomerAsync_ReturnsResource()
        {
            _documentDbProvider.GetSessionForCustomerAsync(_customerId, _sessionId).Returns(Task.FromResult(_session).Result);

            // Act
            var result = await _getSessionByIdHttpTriggerService.GetSessionForCustomerAsync(_customerId, _sessionId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<Models.Session>(result);
        }
    }
}
