using DFC.GeoCoding.Standard.OrdnanceSurvey.Models;
using DFC.GeoCoding.Standard.OrdnanceSurvey.Services;
using Microsoft.Extensions.Logging;

namespace NCS.DSS.Sessions.GeoCoding
{

    public class GeoCodingService : IGeoCodingService
    {
        private readonly ILogger<GeoCodingService> _logger;

        private readonly IOSService _OSService;

        public GeoCodingService(IOSService OSService, ILogger<GeoCodingService> logger)
        {
            _logger = logger;
            _OSService = OSService;
        }

        public async Task<Position> GetPositionForPostcodeAsync(string postcode)
        {
            if (!string.IsNullOrEmpty(postcode))
            {
                return await _OSService.GetPositionForPostcodeAsync(postcode);
            }

            _logger.LogInformation("Ordnance Survey Service is retuning null for postcode: {Postcode}", postcode);
            return null;

        }
    }
}
