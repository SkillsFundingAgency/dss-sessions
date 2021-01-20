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
        
        [Test]
        public void ValidateTests_ReturnValidationResult_WhenSessionsIsNotSuppliedForPost()
        {
            var diversity = new Models.Session();

            var validation = new Validate();

            var result = validation.ValidateResource(diversity);

            // Assert
            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenDateandTimeOfSessionIsNotSuppliedForPost()
        {
            var diversity = new Models.Session();

            var validation = new Validate();

            var result = validation.ValidateResource(diversity);

            // Assert
            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenReasonForNonAttendanceIsNotValid()
        {
            var diversity = new Models.Session
            {
                DateandTimeOfSession = DateTime.UtcNow,
                ReasonForNonAttendance = (ReasonForNonAttendance) 100
            };

            var validation = new Validate();

            var result = validation.ValidateResource(diversity);

            // Assert
            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
        }
        
        [Test]
        public void ValidateTests_ReturnValidationResult_WhenLastModifiedDateIsInTheFuture()
        {
            var diversity = new Models.Session
            {
                DateandTimeOfSession = DateTime.UtcNow,
                LastModifiedDate = DateTime.MaxValue
            };

            var validation = new Validate();

            var result = validation.ValidateResource(diversity);

            // Assert
            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenReasonForNonAttendanceIsProvidedWhenASessionAttended()
        {
            // Arrange
            var diversity = new Models.Session
            {
                ReasonForNonAttendance = ReasonForNonAttendance.NotKnown,
                SessionAttended = true
            };
            var validation = new Validate();

            // Act 
            var result = validation.ValidateResource(diversity);

            // Assert
            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.That(result.Any(x=> x.ErrorMessage == "ReasonForNonAttendance cannot be provided when SessionAttended is true"));
        }

        public void ValidateTests_DoesNotReturnValidationResult_WhenReasonForNonAttendanceIsProvidedWhenASessionAttendedIsNull()
        {
            // Arrange
            var session = new Models.Session
            {
                ReasonForNonAttendance = ReasonForNonAttendance.NotKnown,
                SessionAttended = null
            };
            var validation = new Validate();

            // Act 
            var result = validation.ValidateResource(session);

            // Assert
            Assert.IsInstanceOf<List<ValidationResult>>(result);
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
            var validation = new Validate();

            // Act 
            var result = validation.ValidateResource(session);

            // Assert
            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.That(!result.Any(x => x.ErrorMessage == "ReasonForNonAttendance cannot be provided when SessionAttended is true"));
        }
    }
}