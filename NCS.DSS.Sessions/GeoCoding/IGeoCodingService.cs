using System.Threading.Tasks;
using DFC.GeoCoding.Standard.AzureMaps.Model;

namespace NCS.DSS.Sessions.GeoCoding
{
    public interface IGeoCodingService
    {
        Task<Position> GetPositionForPostcodeAsync(string postcode);
    }
}
