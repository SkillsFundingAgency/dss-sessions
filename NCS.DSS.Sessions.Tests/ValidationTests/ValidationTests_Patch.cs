using NCS.DSS.Sessions.Validation;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NCS.DSS.Sessions.Tests.ValidationTests
{
    [TestFixture]
    public  class ValidationTests_Patch
    {
        private IValidate _validate;

        [SetUp]
        public void Setup()
        {
            _validate = new Validate();
        }
        [Test]
        public void ValidateTests_ReturnValidationResult_WhenLastModifiedTouchpointIdIsValidForPost()
        {
            var session = new Models.SessionPatch
            {
                LastModifiedTouchpointId = "0000000001",
                DateandTimeOfSession = DateTime.UtcNow

            };

            var result = _validate.ValidateResource(session);

            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.That(result.Count.Equals(0));
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenLastModifiedTouchpointIdIsInvalidForPost()
        {
            var session = new Models.SessionPatch
            {
                LastModifiedTouchpointId = "000000000A",
                DateandTimeOfSession = DateTime.UtcNow

            };

            var result = _validate.ValidateResource(session);

            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.That(result.Count.Equals(1));
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenSubcontractorIdIsValidForPost()
        {
            var session = new Models.SessionPatch
            {
                LastModifiedTouchpointId = "0000000001",
                DateandTimeOfSession = DateTime.UtcNow,
                SubcontractorId = "01234567899876543210"
            };

            var result = _validate.ValidateResource(session);

            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.That(result.Count.Equals(0));
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenSubcontractorIdIsInvalidForPost()
        {
            var session = new Models.SessionPatch
            {
                LastModifiedTouchpointId = "0000000001",
                DateandTimeOfSession = DateTime.UtcNow,
                SubcontractorId = "0123[]6789!£6543210"
            };

            var result = _validate.ValidateResource(session);

            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.That(result.Count.Equals(1));
        }

    }
}
