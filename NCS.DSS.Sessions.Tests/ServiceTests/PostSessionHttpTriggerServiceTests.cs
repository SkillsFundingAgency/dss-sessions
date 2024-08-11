using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Moq;
using NCS.DSS.Sessions.Cosmos.Provider;
using NCS.DSS.Sessions.PostSessionHttpTrigger.Service;
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
        private Mock<IDocumentDBProvider> _documentDbProvider;
        private Models.Session _session;

        [SetUp]
        public void Setup()
        {
            _documentDbProvider = new Mock<IDocumentDBProvider>();
            _postSessionHttpTriggerService = new PostSessionHttpTriggerService(_documentDbProvider.Object);
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
            const string documentServiceResponseClass = "Microsoft.Azure.Documents.DocumentServiceResponse, Microsoft.Azure.DocumentDB.Core, Version=2.2.1.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";
            const string dictionaryNameValueCollectionClass = "Microsoft.Azure.Documents.Collections.DictionaryNameValueCollection, Microsoft.Azure.DocumentDB.Core, Version=2.2.1.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";

            var resourceResponse = new ResourceResponse<Document>(new Document());
            var documentServiceResponseType = Type.GetType(documentServiceResponseClass);

            const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;

            var headers = new NameValueCollection { { "x-ms-request-charge", "0" } };

            var headersDictionaryType = Type.GetType(dictionaryNameValueCollectionClass);

            var headersDictionaryInstance = Activator.CreateInstance(headersDictionaryType, headers);

            var arguments = new[] { Stream.Null, headersDictionaryInstance, HttpStatusCode.Created, null };

            var documentServiceResponse = documentServiceResponseType.GetTypeInfo().GetConstructors(flags)[0].Invoke(arguments);

            var responseField = typeof(ResourceResponse<Document>).GetTypeInfo().GetField("response", flags);

            responseField?.SetValue(resourceResponse, documentServiceResponse);

            _documentDbProvider.Setup(x => x.CreateSessionAsync(It.IsAny<Models.Session>())).Returns(Task.FromResult(resourceResponse));

            // Act
            var result = await _postSessionHttpTriggerService.CreateAsync(_session);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<Models.Session>());

        }
    }
}
