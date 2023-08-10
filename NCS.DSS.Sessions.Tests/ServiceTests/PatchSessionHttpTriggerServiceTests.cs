using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Moq;
using NCS.DSS.Sessions.Cosmos.Provider;
using NCS.DSS.Sessions.Models;
using NCS.DSS.Sessions.PatchSessionHttpTrigger.Service;
using Newtonsoft.Json;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace NCS.DSS.Sessions.Tests.ServiceTests
{

    [TestFixture]
    public class PatchSessionHttpTriggerServiceTests
    {
        private readonly Guid _sessionId = Guid.Parse("7E467BDB-213F-407A-B86A-1954053D3C24");
        private IPatchSessionHttpTriggerService _sessionPatchHttpTriggerService;
        private Mock<ISessionPatchService> _sessionPatchService;
        private Mock<IDocumentDBProvider> _documentDbProvider;
        private Session _session;
        private SessionPatch _sessionPatch;
       
        private string _json;

        [SetUp]
        public void Setup()
        {
            _documentDbProvider = new Mock<IDocumentDBProvider>();
            _sessionPatchService = new Mock<ISessionPatchService>();
            _sessionPatchHttpTriggerService = new PatchSessionHttpTriggerService(_documentDbProvider.Object, _sessionPatchService.Object);
            _session = new Session();
            _sessionPatch = new SessionPatch() { VenuePostCode="B33 9BX" };
            _json = JsonConvert.SerializeObject(_sessionPatch);
            _sessionPatchService.Setup(x=>x.Patch(_json, _sessionPatch)).Returns(_session.ToString());
        }

        [Test]
        public void PatchSessionHttpTriggerServiceTests_PatchResource_ReturnsNullWhenSessionJsonIsNullOrEmpty()
        {
            // Act
            var result = _sessionPatchHttpTriggerService.PatchResource(null, Arg.Any<SessionPatch>());

            // Assert
            Assert.IsNull(result);
        }


        [Test]
        public void PatchSessionHttpTriggerServiceTests_PatchResource_ReturnsNullWhenSessionPatchIsNullOrEmpty()
        {
            // Act
            var result = _sessionPatchHttpTriggerService.PatchResource(Arg.Any<string>(), null);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task PatchSessionHttpTriggerServiceTests_UpdateCosmosAsync_ReturnsNullWhenResourceCannotBeUpdated()
        {
            //Arrange
            _documentDbProvider.Setup(x=>x.UpdateSessionAsync(It.IsAny<string>(), It.IsAny<Guid>())).Returns(Task.FromResult<ResourceResponse<Document>>(null));

            // Act
            var result = await _sessionPatchHttpTriggerService.UpdateCosmosAsync(_json, _sessionId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task PatchSessionHttpTriggerServiceTests_UpdateCosmosAsync_ReturnsNullWhenResourceCannotBeFound()
        {
            // Arrange
            _documentDbProvider.Setup(x=>x.CreateSessionAsync(_session)).Returns(Task.FromResult(new ResourceResponse<Document>(null)));

            // Act
            var result = await _sessionPatchHttpTriggerService.UpdateCosmosAsync(_json, _sessionId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task PatchSessionHttpTriggerServiceTests_UpdateCosmosAsync_ReturnsResourceWhenUpdated()
        {
            // Arrange
            const string documentServiceResponseClass = "Microsoft.Azure.Documents.DocumentServiceResponse, Microsoft.Azure.DocumentDB.Core, Version=2.2.1.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";
            const string dictionaryNameValueCollectionClass = "Microsoft.Azure.Documents.Collections.DictionaryNameValueCollection, Microsoft.Azure.DocumentDB.Core, Version=2.2.1.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";

            var resourceResponse = new ResourceResponse<Document>(new Document());
            var documentServiceResponseType = Type.GetType(documentServiceResponseClass);

            const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;

            var headers = new NameValueCollection { { "x-ms-request-charge", "0" } };

            var headersDictionaryType = Type.GetType(dictionaryNameValueCollectionClass);

            var headersDictionaryInstance = Activator.CreateInstance(headersDictionaryType, headers);

            var arguments = new[] { Stream.Null, headersDictionaryInstance, HttpStatusCode.OK, null };

            var documentServiceResponse = documentServiceResponseType.GetTypeInfo().GetConstructors(flags)[0].Invoke(arguments);

            var responseField = typeof(ResourceResponse<Document>).GetTypeInfo().GetField("response", flags);

            responseField?.SetValue(resourceResponse, documentServiceResponse);

            _documentDbProvider.Setup(x=>x.UpdateSessionAsync(It.IsAny<string>(), It.IsAny<Guid>())).Returns(Task.FromResult(resourceResponse));

            // Act
            var result = await _sessionPatchHttpTriggerService.UpdateCosmosAsync(_json, _sessionId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<Session>(result);

        }

        [Test]
        public async Task PatchSessionHttpTriggerServiceTests_GetActionPlanForCustomerAsync_ReturnsNullWhenResourceHasNotBeenFound()
        {
            // Arrange
            _documentDbProvider.Setup(x=>x.GetSessionForCustomerToUpdateAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult<string>(null));

            // Act
            var result = await _sessionPatchHttpTriggerService.GetSessionForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>());

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task PatchSessionHttpTriggerServiceTests_GetActionPlanForCustomerAsync_ReturnsResourceWhenResourceHasBeenFound()
        {
            // Arrange
            _documentDbProvider.Setup(x=>x.GetSessionForCustomerToUpdateAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(_json));

            // Act
            var result = await _sessionPatchHttpTriggerService.GetSessionForCustomerAsync(Arg.Any<Guid>(), Arg.Any<Guid>());

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<string>(result);
        }
    }

}