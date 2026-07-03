using DFC.GeoCoding.Standard.OrdnanceSurvey.Models;

namespace NCS.DSS.Sessions.GeoCoding
{
    public interface IGeoCodingService
    {
        Task<Position> GetPositionForPostcodeAsync(string postcode);
    }
}
