using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NCS.DSS.Sessions.Models;
using NCS.DSS.Sessions.ReferenceData;

namespace NCS.DSS.Sessions.Validation
{
    public class Validate : IValidate
    {
        public List<ValidationResult> ValidateResource(ISession resource)
        {
            var context = new ValidationContext(resource, null, null);
            var results = new List<ValidationResult>();

            Validator.TryValidateObject(resource, context, results, true);
            ValidateSessionRules(resource, results);

            return results;
        }

        private void ValidateSessionRules(ISession sessionResource, List<ValidationResult> results)
        {
            if (sessionResource == null)
                return;

            if (sessionResource.LastModifiedDate.HasValue && sessionResource.LastModifiedDate.Value > DateTime.UtcNow)
                results.Add(new ValidationResult("Last Modified Date must be less the current date/time", new[] { "LastModifiedDate" }));

            if (sessionResource.ReasonForNonAttendance.HasValue && !Enum.IsDefined(typeof(ReasonForNonAttendance), sessionResource.ReasonForNonAttendance.Value))
                results.Add(new ValidationResult("Please supply a valid Reason For Non Attendance", new[] { "ReasonForNonAttendance" }));

        }

    }
}
