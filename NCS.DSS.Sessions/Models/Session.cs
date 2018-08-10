using System;
using System.ComponentModel.DataAnnotations;
using NCS.DSS.Sessions.ReferenceData;
using NCS.DSS.Sessions.Annotations;

namespace NCS.DSS.Sessions.Models
{
    public class Session : ISession
    {
        private const string PostcodeRegEx = @"([Gg][Ii][Rr] 0[Aa]{2})|((([A-Za-z][0-9]{1,2})|(([A-Za-z][A-Ha-hJ-Yj-y][0-9]{1,2})|(([A-Za-z][0-9][A-Za-z])|([A-Za-z][A-Ha-hJ-Yj-y][0-9]?[A-Za-z]))))\s?[0-9][A-Za-z]{2})";

        [Example(Description = "b8592ff8-af97-49ad-9fb2-e5c3c717fd85")]
        [Display(Description = "Unique identifier of the appointment record")]
        [Newtonsoft.Json.JsonProperty(PropertyName = "id")]
        public Guid? SessionId { get; set; }

        [Display(Description = "Unique identifier of a customer.")]
        [Example(Description = "2730af9c-fc34-4c2b-a905-c4b584b0f379")]
        public Guid? CustomerId { get; set; }

        [Example(Description = "b8592ff8-af97-49ad-9fb2-e5c3c717fd85")]
        [Display(Description = "Unique identifier to the related interaction resource")]
        public Guid? InteractionId { get; set; }

        [Required]
        [Display(Description = "Date and time of the session with the customer")]
        [Example(Description = "2018-06-21T14:45:00")]
        public DateTime? DateandTimeOfSession { get; set; }

        [Display(Description = "PostCode of the session or appointment venue where applicable")]
        [RegularExpression(PostcodeRegEx, ErrorMessage = "Postcode must be in valid UK format as specified in the UK Government Data Standard.")]
        [StringLength(10)]
        public string VenuePostCode { get; set; }

        [Display(Description = "Indicator to say whether the session was attended or not")]
        [Example(Description = "true/false")]
        public bool? SessionAttended { get; set; }

        [Display(Description = "See DSS Reference Data Resource for values  (Reason For Non Attendance) reference data")]
        [Example(Description ="1")]
        public ReasonForNonAttendance? ReasonForNonAttendance { get; set; }

        [Display(Description = "Date and time of the last modification to the record.  This time is auto-generated by CDS 2.0 if a value is not supplied")]
        [Example(Description = "2018-06-21T14:45:00")]
        public DateTime? LastModifiedDate { get; set; }

        [StringLength(10, MinimumLength = 10)]
        [Display(Description = "Identifier of the touchpoint who made the last change to the record")]
        [Example(Description = "0000000001")]
        public string LastModifiedTouchpointId { get; set; }

        public void SetDefaultValues()
        {
             SessionId = Guid.NewGuid();

            if (!LastModifiedDate.HasValue)
                LastModifiedDate = DateTime.UtcNow;

            if (ReasonForNonAttendance == null)
                ReasonForNonAttendance = ReferenceData.ReasonForNonAttendance.NotKnown;
        }

        public void SetIds(Guid customerId, Guid interactionId, string touchpointId)
        {
            SessionId = Guid.NewGuid();
            CustomerId = customerId;
            InteractionId = interactionId;
            LastModifiedTouchpointId = touchpointId;
        }

        public void Patch(SessionPatch sessionPatch)
        {
            if(sessionPatch == null)
                return;

            if(sessionPatch.DateandTimeOfSession.HasValue)
                DateandTimeOfSession = sessionPatch.DateandTimeOfSession;

            if (!string.IsNullOrEmpty(sessionPatch.VenuePostCode))
                VenuePostCode = sessionPatch.VenuePostCode;

            if (sessionPatch.SessionAttended.HasValue)
                SessionAttended = sessionPatch.SessionAttended;

            if (sessionPatch.ReasonForNonAttendance.HasValue)
                ReasonForNonAttendance = sessionPatch.ReasonForNonAttendance.Value;

            if (sessionPatch.LastModifiedDate.HasValue)
                LastModifiedDate = sessionPatch.LastModifiedDate;

            if (!string.IsNullOrEmpty(sessionPatch.LastModifiedTouchpointId))
                LastModifiedTouchpointId = sessionPatch.LastModifiedTouchpointId;

        }
    }
}