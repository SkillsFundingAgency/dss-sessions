using System.Threading.Tasks;
using DFC.GeoCoding.Standard.AzureMaps.Model;
using DFC.GeoCoding.Standard.AzureMaps.Service;

namespace NCS.DSS.Sessions.GeoCoding
{
    public class GeoCodingService : IGeoCodingService
    {

        private readonly IAzureMapService _azureMapService;

        public GeoCodingService(IAzureMapService azureMapService)
        {
            _azureMapService = azureMapService;
        }

        public async Task<Position> GetPositionForPostcodeAsync(string postcode)
        {
            return await _azureMapService.GetPositionForAddress(postcode);
        }
    }
}
