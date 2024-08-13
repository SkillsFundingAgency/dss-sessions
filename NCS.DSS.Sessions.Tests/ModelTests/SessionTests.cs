using NCS.DSS.Sessions.ReferenceData;
using NSubstitute;
using NUnit.Framework;
using System;

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
            Assert.That(session.LastModifiedDate, Is.Not.Null);
            Assert.That(session.ReasonForNonAttendance, Is.Null);
        }

        [Test]
        public void SessionsTests_CheckLastModifiedDateDoesNotGetPopulated_WhenSetDefaultValuesIsCalled()
        {
            var session = new Models.Session { LastModifiedDate = DateTime.MaxValue };

            session.SetDefaultValues();

            // Assert
            Assert.That(session.LastModifiedDate, Is.EqualTo(DateTime.MaxValue));
        }

        [Test]
        public void SessionsTests_CheckReasonForNonAttendanceDoesNotGetPopulated_WhenSetDefaultValuesIsCalled()
        {
            var session = new Models.Session { ReasonForNonAttendance = ReasonForNonAttendance.PersonalSituation };

            session.SetDefaultValues();

            // Assert
            Assert.That(session.ReasonForNonAttendance, Is.EqualTo(ReasonForNonAttendance.PersonalSituation));
        }


        [Test]
        public void SessionsTests_CheckSessionsIdIsSet_WhenSetIdsIsCalled()
        {
            var session = new Models.Session();

            session.SetIds(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>());

            // Assert
            Assert.That(session.SessionId, Is.Not.EqualTo(Guid.Empty));
        }

        [Test]
        public void SessionsTests_CheckCustomerIdIsSet_WhenSetIdsIsCalled()
        {
            var session = new Models.Session();

            var customerId = Guid.NewGuid();
            session.SetIds(customerId, Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>());

            // Assert
            Assert.That(session.CustomerId, Is.EqualTo(customerId));
        }

        [Test]
        public void SessionsTests_CheckInteractionIdIsSet_WhenSetIdsIsCalled()
        {
            var session = new Models.Session();

            var interactionId = Guid.NewGuid();
            session.SetIds(Arg.Any<Guid>(), interactionId, Arg.Any<string>(), Arg.Any<string>());

            // Assert
            Assert.That(session.InteractionId, Is.EqualTo(interactionId));
        }

        [Test]
        public void SessionsTests_CheckLastModifiedTouchpointIdIsSet_WhenSetIdsIsCalled()
        {
            var session = new Models.Session();

            session.SetIds(Arg.Any<Guid>(), Arg.Any<Guid>(), "0000000000", Arg.Any<string>());

            // Assert
            Assert.That(session.LastModifiedTouchpointId, Is.EqualTo("0000000000"));
        }

        [Test]
        public void SessionsTests_CheckSubcontractorIdIsSet_WhenSetIdsIsCalled()
        {
            var session = new Models.Session();

            session.SetIds(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), "0000000000");

            // Assert
            Assert.That(session.SubcontractorId, Is.EqualTo("0000000000"));
        }

        [Test]
        public void SessionsTests_ReasonForNonAttendanceIsSetToUnknown_WhenSetDefaultValuesIsCalled()
        {
            // Arrange
            var session = new Models.Session { LastModifiedDate = DateTime.MaxValue, ReasonForNonAttendance = null, SessionAttended = false };

            // Act
            session.SetDefaultValues();

            // Assert
            Assert.That(session.ReasonForNonAttendance, Is.EqualTo(ReasonForNonAttendance.NotKnown));
        }

        [Test]
        public void SessionsTests_ReasonForNonAttendanceIsNotSetToUnknown_WhenSetDefaultValuesIsCalled()
        {
            // Arrange
            var session = new Models.Session { LastModifiedDate = DateTime.MaxValue, ReasonForNonAttendance = ReasonForNonAttendance.Forgot, SessionAttended = false };

            // Act
            session.SetDefaultValues();

            // Assert
            Assert.That(session.ReasonForNonAttendance, Is.EqualTo(ReasonForNonAttendance.Forgot));
        }
    }
}