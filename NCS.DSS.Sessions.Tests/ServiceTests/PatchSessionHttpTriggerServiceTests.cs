using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using NCS.DSS.Sessions.Cosmos.Provider;
using NCS.DSS.Sessions.Models;
using NCS.DSS.Sessions.PatchSessionHttpTrigger.Service;
using Newtonsoft.Json;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using NUnit.Framework;

namespace NCS.DSS.Sessions.Tests.ServiceTests
{

    [TestFixture]
    public class PatchSessionHttpTriggerServiceTests
    {
        private IPatchSessionHttpTriggerService _sessionPatchHttpTriggerService;
        private ISessionPatchService _sessionPatchService;
        private IDocumentDBProvider _documentDbProvider;
        private Session _session;
        private SessionPatch _sessionPatch;
        private string _json;

        [SetUp]
        public void Setup()
        {
            _documentDbProvider = Substitute.For<IDocumentDBProvider>();
            _sessionPatchService = Substitute.For<ISessionPatchService>();
            _sessionPatchHttpTriggerService = Substitute.For<PatchSessionHttpTriggerService>(_documentDbProvider, _sessionPatchService);
            _session = Substitute.For<Session>();
            _sessionPatch = Substitute.For<SessionPatch>();
            _json = JsonConvert.SerializeObject(_sessionPatch);
            _sessionPatchService.Patch(_json, _sessionPatch).Returns(_session);
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
            _documentDbProvider.UpdateSessionAsync(Arg.Any<Session>()).ReturnsNull();

            // Act
            var result = await _sessionPatchHttpTriggerService.UpdateCosmosAsync(_session);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task PatchSessionHttpTriggerServiceTests_UpdateCosmosAsync_ReturnsNullWhenResourceCannotBeFound()
        {
            _documentDbProvider.CreateSessionAsync(_session).Returns(Task.FromResult(new ResourceResponse<Document>(null)).Result);

            // Act
            var result = await _sessionPatchHttpTriggerService.UpdateCosmosAsync(_session);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task PatchSessionHttpTriggerServiceTests_UpdateCosmosAsync_ReturnsResourceWhenUpdated()
        {
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

            _documentDbProvider.UpdateSessionAsync(Arg.Any<Session>()).Returns(Task.FromResult(resourceResponse).Result);

            // Act
            var result = await _sessionPatchHttpTriggerService.UpdateCosmosAsync(_session);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<Session>(result);

        }

        [Test]
        public async Task PatchSessionHttpTriggerServiceTests_GetActionPlanForCustomerAsync_ReturnsNullWhenResourceHasNotBeenFound()
        {
            _documentDbProvider.GetSessionForCustomerToUpdateAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).ReturnsNull();

            // Act
            var result = await _sessionPatchHttpTriggerService.GetSessionForCustomerAsync(Arg.Any<Guid>(), Arg.Any<Guid>());

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task PatchSessionHttpTriggerServiceTests_GetActionPlanForCustomerAsync_ReturnsResourceWhenResourceHasBeenFound()
        {
            _documentDbProvider.GetSessionForCustomerToUpdateAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(Task.FromResult(_json).Result);

            // Act
            var result = await _sessionPatchHttpTriggerService.GetSessionForCustomerAsync(Arg.Any<Guid>(), Arg.Any<Guid>());

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<string>(result);
        }
    }

}