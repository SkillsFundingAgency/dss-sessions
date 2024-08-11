using DFC.GeoCoding.Standard.AzureMaps.Model;
using System.Threading.Tasks;

namespace NCS.DSS.Sessions.GeoCoding
{
    public interface IGeoCodingService
    {
        Task<Position> GetPositionForPostcodeAsync(string postcode);
    }
}
