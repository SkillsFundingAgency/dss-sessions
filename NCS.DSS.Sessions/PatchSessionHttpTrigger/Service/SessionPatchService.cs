using System;
using System.Collections.Generic;
using System.Text;
using DFC.JSON.Standard;
using Microsoft.Extensions.Logging;
using NCS.DSS.Sessions.Models;
using Newtonsoft.Json.Linq;

namespace NCS.DSS.Sessions.PatchSessionHttpTrigger.Service
{
    public class SessionPatchService : ISessionPatchService
    {
        private IJsonHelper _jsonHelper;
        private ILogger _logger;

        public SessionPatchService(IJsonHelper jsonHelper,ILogger logger )
        {
            _jsonHelper = jsonHelper;
            _logger = logger;
        }

        public string Patch(string sessionJson, SessionPatch sessionPatch)
        {
            if (string.IsNullOrEmpty(sessionJson))
            {
                _logger.LogInformation($"SessionPatchService sessionJson is null");
                return null;
            }

            var obj = JObject.Parse(sessionJson);

            if (sessionPatch.DateandTimeOfSession.HasValue)
            {
                _logger.LogInformation($"SessionPatchService sessionPatch.DateandTimeOfSession.HasValue ");
                _jsonHelper.UpdatePropertyValue(obj["DateandTimeOfSession"], sessionPatch.DateandTimeOfSession);
            }

            if (!string.IsNullOrEmpty(sessionPatch.VenuePostCode))
            {
              _jsonHelper.UpdatePropertyValue(obj["VenuePostCode"], sessionPatch.VenuePostCode);
            }

            if (sessionPatch.SessionAttended.HasValue)
            {
                _logger.LogInformation($"SessionPatchService sessionPatch.SessionAttended.HasValue ");
                _jsonHelper.UpdatePropertyValue(obj["SessionAttended"], sessionPatch.SessionAttended);
            }

            if (sessionPatch.ReasonForNonAttendance.HasValue)
            {
                _logger.LogInformation($"SessionPatchService ReasonForNonAttendance HasValue");
                _jsonHelper.UpdatePropertyValue(obj["ReasonForNonAttendance"], sessionPatch.ReasonForNonAttendance.Value);
            }

            if (sessionPatch.LastModifiedDate.HasValue)
            {
                _logger.LogInformation($"SessionPatchService LastModifiedDate HasValue");
                _jsonHelper.UpdatePropertyValue(obj["LastModifiedDate"], sessionPatch.LastModifiedDate);
            }

            if (!string.IsNullOrEmpty(sessionPatch.LastModifiedTouchpointId))
            {
                _logger.LogInformation($"SessionPatchService LastModifiedTouchpointId HasValue");
                _jsonHelper.UpdatePropertyValue(obj["LastModifiedTouchpointId"], sessionPatch.LastModifiedTouchpointId);
            }

            if (!string.IsNullOrEmpty(sessionPatch.SubcontractorId))
            {
                if (obj["SubcontractorId"] == null)
                {
                    _logger.LogInformation($"SessionPatchService SubcontractorId is null");
                    _jsonHelper.CreatePropertyOnJObject(obj, "SubcontractorId", sessionPatch.SubcontractorId);
                }
                else
                    _jsonHelper.UpdatePropertyValue(obj["SubcontractorId"], sessionPatch.SubcontractorId);
            }

            if (sessionPatch.Longitude.HasValue)
            {
                if (obj["Longitude"] == null)
                {
                    _jsonHelper.CreatePropertyOnJObject(obj, "Longitude", sessionPatch.Longitude);
                    _logger.LogInformation($"SessionPatchService Longitude is null");
                }
                else
                    _jsonHelper.UpdatePropertyValue(obj["Longitude"], sessionPatch.Longitude);
            }

            if (sessionPatch.Latitude.HasValue)
            {
                if (obj["Latitude"] == null)
                {
                    _jsonHelper.CreatePropertyOnJObject(obj, "Latitude", sessionPatch.Latitude);
                    _logger.LogInformation($"SessionPatchService Latitude is null");
                }
                else
                    _jsonHelper.UpdatePropertyValue(obj["Latitude"], sessionPatch.Latitude);
            }

            return obj.ToString();

        }
    }
}