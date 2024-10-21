using DFC.JSON.Standard;
using NCS.DSS.Sessions.Models;
using NCS.DSS.Sessions.PatchSessionHttpTrigger.Service;
using NCS.DSS.Sessions.ReferenceData;
using Newtonsoft.Json;
using NSubstitute;
using NUnit.Framework;
using System;

namespace NCS.DSS.Sessions.Tests.ServiceTests
{

    [TestFixture]
    public class SessionPatchServiceTests
    {
        private IJsonHelper _jsonHelper;
        private SessionPatchService _sessionPatchService;
        private SessionPatch _sessionPatch;
        private string _json;

        [SetUp]
        public void Setup()
        {
            _jsonHelper = new JsonHelper();
            _sessionPatchService = new SessionPatchService(_jsonHelper);
            _sessionPatch = Substitute.For<SessionPatch>();

            _json = JsonConvert.SerializeObject(_sessionPatch);

        }

        [Test]
        public void SessionPatchServiceTests_ReturnsNull_WhenSessionPatchIsNull()
        {
            var result = _sessionPatchService.Patch(string.Empty, Arg.Any<SessionPatch>());

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void SessionPatchServiceTests_CheckDateandTimeOfSessionIsUpdated_WhenPatchIsCalled()
        {
            var sessionPatch = new SessionPatch { DateandTimeOfSession = DateTime.MaxValue };

            var patchedSession = _sessionPatchService.Patch(_json, sessionPatch);

            var session = JsonConvert.DeserializeObject<Session>(patchedSession);

            // Assert
            Assert.That(session.DateandTimeOfSession, Is.EqualTo(DateTime.MaxValue));
        }

        [Test]
        public void SessionPatchServiceTests_CheckVenuePostCodeIsUpdated_WhenPatchIsCalled()
        {
            var sessionPatch = new SessionPatch { VenuePostCode = "CV1 1VC" };

            var patchedSession = _sessionPatchService.Patch(_json, sessionPatch);

            var session = JsonConvert.DeserializeObject<Session>(patchedSession);

            // Assert
            Assert.That(session.VenuePostCode, Is.EqualTo("CV1 1VC"));
        }

        [Test]
        public void SessionPatchServiceTests_CheckSessionAttendedIsUpdated_WhenPatchIsCalled()
        {
            var sessionPatch = new SessionPatch { SessionAttended = true };

            var patchedSession = _sessionPatchService.Patch(_json, sessionPatch);

            var session = JsonConvert.DeserializeObject<Session>(patchedSession);

            // Assert
            Assert.That(session.SessionAttended, Is.EqualTo(true));
        }

        [Test]
        public void SessionPatchServiceTests_CheckReasonForNonAttendanceIsUpdated_WhenPatchIsCalled()
        {
            var sessionPatch = new SessionPatch { ReasonForNonAttendance = ReasonForNonAttendance.Forgot };

            var patchedSession = _sessionPatchService.Patch(_json, sessionPatch);

            var session = JsonConvert.DeserializeObject<Session>(patchedSession);

            // Assert
            Assert.That(session.ReasonForNonAttendance, Is.EqualTo(ReasonForNonAttendance.Forgot));
        }

        [Test]
        public void SessionPatchServiceTests_CheckLastModifiedDateIsUpdated_WhenPatchIsCalled()
        {
            var sessionPatch = new SessionPatch { LastModifiedDate = DateTime.MaxValue };

            var patchedSession = _sessionPatchService.Patch(_json, sessionPatch);

            var session = JsonConvert.DeserializeObject<Session>(patchedSession);

            // Assert
            Assert.That(session.LastModifiedDate, Is.EqualTo(DateTime.MaxValue));
        }

        [Test]
        public void SessionPatchServiceTests_CheckLastModifiedByUpdated_WhenPatchIsCalled()
        {
            var sessionPatch = new SessionPatch { LastModifiedTouchpointId = "0000000111" };

            var patchedSession = _sessionPatchService.Patch(_json, sessionPatch);

            var session = JsonConvert.DeserializeObject<Session>(patchedSession);

            // Assert
            Assert.That(session.LastModifiedTouchpointId, Is.EqualTo("0000000111"));
        }

        [Test]
        public void SessionPatchServiceTests_CheckSubcontractorIdIsUpdated_WhenPatchIsCalled()
        {
            var sessionPatch = new SessionPatch { SubcontractorId = "0000000111" };

            var patchedSession = _sessionPatchService.Patch(_json, sessionPatch);

            var session = JsonConvert.DeserializeObject<Session>(patchedSession);

            // Assert
            Assert.That(session.SubcontractorId, Is.EqualTo("0000000111"));
        }
    }
}
