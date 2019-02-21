using NCS.DSS.Sessions.Helpers;
using NCS.DSS.Sessions.Models;
using Newtonsoft.Json.Linq;

namespace NCS.DSS.Sessions.PatchSessionHttpTrigger.Service
{
    public class SessionPatchService : ISessionPatchService
    {
        public string Patch(string sessionJson, SessionPatch sessionPatch)
        {
            if (string.IsNullOrEmpty(sessionJson))
                return null;

            var obj = JObject.Parse(sessionJson);

            if (sessionPatch.DateandTimeOfSession.HasValue)
                JsonHelper.UpdatePropertyValue(obj["DateandTimeOfSession"], sessionPatch.DateandTimeOfSession);

            if (!string.IsNullOrEmpty(sessionPatch.VenuePostCode))
                JsonHelper.UpdatePropertyValue(obj["VenuePostCode"], sessionPatch.VenuePostCode);

            if (sessionPatch.SessionAttended.HasValue)
                JsonHelper.UpdatePropertyValue(obj["SessionAttended"], sessionPatch.SessionAttended);

            if (sessionPatch.ReasonForNonAttendance.HasValue)
                JsonHelper.UpdatePropertyValue(obj["ReasonForNonAttendance"],
                    sessionPatch.ReasonForNonAttendance.Value);

            if (sessionPatch.LastModifiedDate.HasValue)
                JsonHelper.UpdatePropertyValue(obj["LastModifiedDate"], sessionPatch.LastModifiedDate);

            if (!string.IsNullOrEmpty(sessionPatch.LastModifiedTouchpointId))
                JsonHelper.UpdatePropertyValue(obj["LastModifiedTouchpointId"], sessionPatch.LastModifiedTouchpointId);

            return obj.ToString();

        }
    }
}
