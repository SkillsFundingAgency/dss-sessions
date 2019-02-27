using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NCS.DSS.Sessions.ReferenceData;
using NCS.DSS.Sessions.Validation;
using NUnit.Framework;

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
    }
}