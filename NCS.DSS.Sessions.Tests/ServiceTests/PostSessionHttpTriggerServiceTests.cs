using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Moq;
using NCS.DSS.Sessions.Cosmos.Provider;
using NCS.DSS.Sessions.Models;
using NCS.DSS.Sessions.PostSessionHttpTrigger.Service;
using NCS.DSS.Sessions.ServiceBus;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace NCS.DSS.Sessions.Tests.ServiceTests
{

    [TestFixture]
    public class PostSessionHttpTriggerServiceTests
    {
        private IPostSessionHttpTriggerService _postSessionHttpTriggerService;
        private Mock<ICosmosDBProvider> _cosmosDbProvider;
        private Mock<ISessionsServiceBusClient> _serviceBusClient;
        private Models.Session _session;

        [SetUp]
        public void Setup()
        {
            _cosmosDbProvider = new Mock<ICosmosDBProvider>();
            _serviceBusClient = new Mock<ISessionsServiceBusClient>();
            _postSessionHttpTriggerService = new PostSessionHttpTriggerService(_cosmosDbProvider.Object,_serviceBusClient.Object);
            _session = Substitute.For<Models.Session>();
        }

        [Test]
        public async Task PostSessionsHttpTriggerServiceTests_CreateAsync_ReturnsNullWhenSessionJsonIsNullOrEmpty()
        {
            // Act
            var result = await _postSessionHttpTriggerService.CreateAsync(null);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task PostSessionsHttpTriggerServiceTests_CreateAsync_ReturnsResourceWhenUpdated()
        {
            //Arrange
            var resourceResponse = new Mock<ItemResponse<Session>>();
            resourceResponse.Setup(x => x.Resource).Returns(_session);
            resourceResponse.Setup(x => x.StatusCode).Returns(HttpStatusCode.Created);

            _cosmosDbProvider.Setup(x => x.CreateSessionAsync(It.IsAny<Models.Session>())).Returns(Task.FromResult(resourceResponse.Object));

            // Act
            var result = await _postSessionHttpTriggerService.CreateAsync(_session);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<Models.Session>());

        }
    }
}
