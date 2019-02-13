using System;
using DFC.JSON.Standard;
using NCS.DSS.Sessions.Models;
using NCS.DSS.Sessions.PatchSessionHttpTrigger.Service;
using NCS.DSS.Sessions.ReferenceData;
using Newtonsoft.Json;
using NSubstitute;
using NUnit.Framework;

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
            _jsonHelper = Substitute.For<JsonHelper>();
            _sessionPatchService = Substitute.For<SessionPatchService>(_jsonHelper);
            _sessionPatch = Substitute.For<SessionPatch>();

            _json = JsonConvert.SerializeObject(_sessionPatch);

        }

        [Test]
        public void SessionPatchServiceTests_ReturnsNull_WhenSessionPatchIsNull()
        {
            var result = _sessionPatchService.Patch(string.Empty, Arg.Any<SessionPatch>());

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void SessionPatchServiceTests_CheckDateandTimeOfSessionIsUpdated_WhenPatchIsCalled()
        {
            var sessionPatch = new SessionPatch { DateandTimeOfSession = DateTime.MaxValue };

            var session = _sessionPatchService.Patch(_json, sessionPatch);

            // Assert
            Assert.AreEqual(DateTime.MaxValue, session.DateandTimeOfSession);
        }

        [Test]
        public void SessionPatchServiceTests_CheckVenuePostCodeIsUpdated_WhenPatchIsCalled()
        {
            var sessionPatch = new SessionPatch { VenuePostCode = "CV1 1VC" };

            var session = _sessionPatchService.Patch(_json, sessionPatch);

            // Assert
            Assert.AreEqual("CV1 1VC", session.VenuePostCode);
        }

        [Test]
        public void SessionPatchServiceTests_CheckSessionAttendedIsUpdated_WhenPatchIsCalled()
        {
            var sessionPatch = new SessionPatch { SessionAttended = true };

            var session = _sessionPatchService.Patch(_json, sessionPatch);

            // Assert
            Assert.AreEqual(true, session.SessionAttended);
        }

        [Test]
        public void SessionPatchServiceTests_CheckReasonForNonAttendanceIsUpdated_WhenPatchIsCalled()
        {
            var sessionPatch = new SessionPatch { ReasonForNonAttendance = ReasonForNonAttendance.Forgot };

            var session = _sessionPatchService.Patch(_json, sessionPatch);

            // Assert
            Assert.AreEqual(ReasonForNonAttendance.Forgot, session.ReasonForNonAttendance);
        }

        [Test]
        public void SessionPatchServiceTests_CheckLastModifiedDateIsUpdated_WhenPatchIsCalled()
        {
            var sessionPatch = new SessionPatch { LastModifiedDate = DateTime.MaxValue };

            var session = _sessionPatchService.Patch(_json, sessionPatch);

            // Assert
            Assert.AreEqual(DateTime.MaxValue, session.LastModifiedDate);
        }

        [Test]
        public void SessionPatchServiceTests_CheckLastModifiedByUpdated_WhenPatchIsCalled()
        {
            var sessionPatch = new SessionPatch { LastModifiedTouchpointId = "0000000111" };

            var session = _sessionPatchService.Patch(_json, sessionPatch);

            // Assert
            Assert.AreEqual("0000000111", session.LastModifiedTouchpointId);
        }

        [Test]
        public void SessionPatchServiceTests_CheckSubcontractorIdIsUpdated_WhenPatchIsCalled()
        {
            var sessionPatch = new SessionPatch { SubcontractorId = "0000000111" };

            var session = _sessionPatchService.Patch(_json, sessionPatch);

            // Assert
            Assert.AreEqual("0000000111", session.SubcontractorId);
        }
    }
}
