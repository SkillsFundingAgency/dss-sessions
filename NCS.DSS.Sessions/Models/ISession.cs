using System;
using NCS.DSS.Sessions.ReferenceData;

namespace NCS.DSS.Sessions.Models
{
    public interface ISession
    {
        DateTime? DateandTimeOfSession { get; set; }
        string VenuePostCode { get; set; }
        bool? SessionAttended { get; set; }
        ReasonForNonAttendance? ReasonForNonAttendance { get; set; }
        DateTime? LastModifiedDate { get; set; }
        string LastModifiedTouchpointId { get; set; }

        void SetDefaultValues();
    }
}