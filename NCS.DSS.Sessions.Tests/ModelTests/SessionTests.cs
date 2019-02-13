using System;
using NCS.DSS.Sessions.ReferenceData;
using NSubstitute;
using NUnit.Framework;

namespace NCS.DSS.Sessions.Tests.ModelTests
{
    [TestFixture]
    public class SessionsTests
    {

        [Test]
        public void SessionsTests_PopulatesDefaultValues_WhenSetDefaultValuesIsCalled()
        {
            var session = new Models.Session();
            session.SetDefaultValues();

            // Assert
            Assert.IsNotNull(session.LastModifiedDate);
            Assert.AreEqual(ReasonForNonAttendance.NotKnown, session.ReasonForNonAttendance);
        }

        [Test]
        public void SessionsTests_CheckLastModifiedDateDoesNotGetPopulated_WhenSetDefaultValuesIsCalled()
        {
            var session = new Models.Session { LastModifiedDate = DateTime.MaxValue };

            session.SetDefaultValues();

            // Assert
            Assert.AreEqual(DateTime.MaxValue, session.LastModifiedDate);
        }
        
        [Test]
        public void SessionsTests_CheckReasonForNonAttendanceDoesNotGetPopulated_WhenSetDefaultValuesIsCalled()
        {
            var session = new Models.Session { ReasonForNonAttendance = ReasonForNonAttendance.PersonalSituation };

            session.SetDefaultValues();

            // Assert
            Assert.AreEqual(ReasonForNonAttendance.PersonalSituation, session.ReasonForNonAttendance);
        }

        
        [Test]
        public void SessionsTests_CheckSessionsIdIsSet_WhenSetIdsIsCalled()
        {
            var session = new Models.Session();

            session.SetIds(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>());

            // Assert
            Assert.AreNotSame(Guid.Empty, session.SessionId);
        }

        [Test]
        public void SessionsTests_CheckCustomerIdIsSet_WhenSetIdsIsCalled()
        {
            var session = new Models.Session();

            var customerId = Guid.NewGuid();
            session.SetIds(customerId, Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>());

            // Assert
            Assert.AreEqual(customerId, session.CustomerId);
        }

        [Test]
        public void SessionsTests_CheckInteractionIdIsSet_WhenSetIdsIsCalled()
        {
            var session = new Models.Session();

            var interactionId = Guid.NewGuid();
            session.SetIds(Arg.Any<Guid>(), interactionId, Arg.Any<string>(), Arg.Any<string>());

            // Assert
            Assert.AreEqual(interactionId, session.InteractionId);
        }

        [Test]
        public void SessionsTests_CheckLastModifiedTouchpointIdIsSet_WhenSetIdsIsCalled()
        {
            var session = new Models.Session();

            session.SetIds(Arg.Any<Guid>(), Arg.Any<Guid>(), "0000000000", Arg.Any<string>());

            // Assert
            Assert.AreEqual("0000000000", session.LastModifiedTouchpointId);
        }

        [Test]
        public void SessionsTests_CheckSubcontractorIdIsSet_WhenSetIdsIsCalled()
        {
            var session = new Models.Session();

            session.SetIds(Arg.Any<Guid>(), Arg.Any<Guid>(),  Arg.Any<string>(), "0000000000");

            // Assert
            Assert.AreEqual("0000000000", session.SubcontractorId);
        }
    }
}