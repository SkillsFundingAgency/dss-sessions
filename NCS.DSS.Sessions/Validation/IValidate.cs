using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NCS.DSS.Sessions.Models;

namespace NCS.DSS.Sessions.Validation
{
    public interface IValidate
    {
        List<ValidationResult> ValidateResource(ISession resource);
    }
}