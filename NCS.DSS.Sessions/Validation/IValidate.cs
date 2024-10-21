using NCS.DSS.Sessions.Models;
using System.ComponentModel.DataAnnotations;

namespace NCS.DSS.Sessions.Validation
{
    public interface IValidate
    {
        List<ValidationResult> ValidateResource(ISession resource);
    }
}