﻿using DFC.GeoCoding.Standard.AzureMaps.Model;
using DFC.GeoCoding.Standard.AzureMaps.Service;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace NCS.DSS.Sessions.GeoCoding
{

    public class GeoCodingService : IGeoCodingService
    {
        private ILogger _logger;

        private readonly IAzureMapService _azureMapService;

        public GeoCodingService(IAzureMapService azureMapService, ILogger logger)
        {
            _logger = logger;
            _azureMapService = azureMapService;
        }

        public async Task<Position> GetPositionForPostcodeAsync(string postcode)
        {
            if (string.IsNullOrEmpty(postcode))
            {
                _logger.LogInformation($"GeoCodingService {postcode} is returning null");
                return null;
            }

            return await _azureMapService.GetPositionForAddress(postcode);
        }
    }
}
