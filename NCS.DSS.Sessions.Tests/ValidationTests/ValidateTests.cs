using NCS.DSS.Sessions.ReferenceData;
using NCS.DSS.Sessions.Validation;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace NCS.DSS.Sessions.Tests.ValidationTests
{
    [TestFixture]
    public class ValidateTests
    {
        private IValidate _validate;

        [SetUp]
        public void Setup()
        {
            _validate = new Validate();
        }


        [Test]
        public void ValidateTests_ReturnValidationResult_WhenSessionsIsNotSuppliedForPost()
        {
            var session = new Models.Session();

            var result = _validate.ValidateResource(session);

            // Assert
            Assert.That(result, Is.InstanceOf<List<ValidationResult>>());
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenDateandTimeOfSessionIsNotSuppliedForPost()
        {
            var session = new Models.Session();

            var result = _validate.ValidateResource(session);

            // Assert
            Assert.That(result, Is.InstanceOf<List<ValidationResult>>());
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenReasonForNonAttendanceIsNotValid()
        {
            var session = new Models.Session
            {
                DateandTimeOfSession = DateTime.UtcNow,
                ReasonForNonAttendance = (ReasonForNonAttendance)100
            };


            var result = _validate.ValidateResource(session);

            // Assert
            Assert.That(result, Is.InstanceOf<List<ValidationResult>>());
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenLastModifiedDateIsInTheFuture()
        {
            var session = new Models.Session
            {
                DateandTimeOfSession = DateTime.UtcNow,
                LastModifiedDate = DateTime.MaxValue
            };

            var result = _validate.ValidateResource(session);

            // Assert
            Assert.That(result, Is.InstanceOf<List<ValidationResult>>());
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenReasonForNonAttendanceIsProvidedWhenASessionAttended()
        {
            // Arrange
            var session = new Models.Session
            {
                ReasonForNonAttendance = ReasonForNonAttendance.NotKnown,
                SessionAttended = true
            };

            // Act 
            var result = _validate.ValidateResource(session);

            // Assert
            Assert.That(result, Is.InstanceOf<List<ValidationResult>>());
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Any(x => x.ErrorMessage == "ReasonForNonAttendance cannot be provided when SessionAttended is true"));
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenLastModifiedTouchpointIdIsValidForPost()
        {
            var session = new Models.Session
            {
                LastModifiedTouchpointId = "0000000001",
                DateandTimeOfSession = DateTime.UtcNow

            };

            var result = _validate.ValidateResource(session);

            Assert.That(result, Is.InstanceOf<List<ValidationResult>>());
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count.Equals(0));
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenLastModifiedTouchpointIdIsInvalidForPost()
        {
            var session = new Models.Session
            {
                LastModifiedTouchpointId = "000000000A",
                DateandTimeOfSession = DateTime.UtcNow

            };

            var result = _validate.ValidateResource(session);

            Assert.That(result, Is.InstanceOf<List<ValidationResult>>());
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count.Equals(1));
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenSubcontractorIdIsValidForPost()
        {
            var session = new Models.Session
            {
                LastModifiedTouchpointId = "0000000001",
                DateandTimeOfSession = DateTime.UtcNow,
                SubcontractorId = "01234567899876543210"
            };

            var result = _validate.ValidateResource(session);

            Assert.That(result, Is.InstanceOf<List<ValidationResult>>());
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count.Equals(0));
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenSubcontractorIdIsInvalidForPost()
        {
            var session = new Models.Session
            {
                LastModifiedTouchpointId = "0000000001",
                DateandTimeOfSession = DateTime.UtcNow,
                SubcontractorId = "0123[]6789!£6543210"
            };

            var result = _validate.ValidateResource(session);

            Assert.That(result, Is.InstanceOf<List<ValidationResult>>());
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count.Equals(1));
        }

        public void ValidateTests_DoesNotReturnValidationResult_WhenReasonForNonAttendanceIsProvidedWhenASessionAttendedIsNull()
        {
            // Arrange
            var session = new Models.Session
            {
                ReasonForNonAttendance = ReasonForNonAttendance.NotKnown,
                SessionAttended = null
            };

            // Act 
            var result = _validate.ValidateResource(session);

            // Assert
            Assert.That(result, Is.InstanceOf<List<ValidationResult>>());
            Assert.That(!result.Any(x => x.ErrorMessage == "ReasonForNonAttendance cannot be provided when SessionAttended is true"));
        }

        public void ValidateTests_DoesNotReturnValidationResult_WhenReasonForNonAttendanceIsProvidedWhenASessionAttendedIsFalse()
        {
            // Arrange
            var session = new Models.Session
            {
                ReasonForNonAttendance = ReasonForNonAttendance.NotKnown,
                SessionAttended = false
            };

            // Act 
            var result = _validate.ValidateResource(session);

            // Assert
            Assert.That(result, Is.InstanceOf<List<ValidationResult>>());
            Assert.That(!result.Any(x => x.ErrorMessage == "ReasonForNonAttendance cannot be provided when SessionAttended is true"));
        }
    }
}