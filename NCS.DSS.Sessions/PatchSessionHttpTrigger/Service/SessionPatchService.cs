using System;
using System.Collections.Generic;
using System.Text;
using DFC.JSON.Standard;
using NCS.DSS.Sessions.Models;
using Newtonsoft.Json.Linq;

namespace NCS.DSS.Sessions.PatchSessionHttpTrigger.Service
{
    public class SessionPatchService : ISessionPatchService
    {
        private IJsonHelper _jsonHelper;

        public SessionPatchService(IJsonHelper jsonHelper)
        {
            _jsonHelper = jsonHelper;
        }

        public string Patch(string sessionJson, SessionPatch sessionPatch)
        {
            if (string.IsNullOrEmpty(sessionJson))
                return null;

            var obj = JObject.Parse(sessionJson);

            if (sessionPatch.DateandTimeOfSession.HasValue)
                _jsonHelper.UpdatePropertyValue(obj["DateandTimeOfSession"], sessionPatch.DateandTimeOfSession);

            if (!string.IsNullOrEmpty(sessionPatch.VenuePostCode))
                _jsonHelper.UpdatePropertyValue(obj["VenuePostCode"], sessionPatch.VenuePostCode);

            if (sessionPatch.SessionAttended.HasValue)
                _jsonHelper.UpdatePropertyValue(obj["SessionAttended"], sessionPatch.SessionAttended);

            if (sessionPatch.ReasonForNonAttendance.HasValue)
                _jsonHelper.UpdatePropertyValue(obj["ReasonForNonAttendance"], sessionPatch.ReasonForNonAttendance.Value);

            if (sessionPatch.LastModifiedDate.HasValue)
                _jsonHelper.UpdatePropertyValue(obj["LastModifiedDate"], sessionPatch.LastModifiedDate);

            if (!string.IsNullOrEmpty(sessionPatch.LastModifiedTouchpointId))
                _jsonHelper.UpdatePropertyValue(obj["LastModifiedTouchpointId"], sessionPatch.LastModifiedTouchpointId);

            if (!string.IsNullOrEmpty(sessionPatch.SubcontractorId))
            {
                if (obj["SubcontractorId"] == null)
                    _jsonHelper.CreatePropertyOnJObject(obj, "SubcontractorId", sessionPatch.SubcontractorId);
                else
                    _jsonHelper.UpdatePropertyValue(obj["SubcontractorId"], sessionPatch.SubcontractorId);
            }

            return obj.ToString();

        }
    }
}