using DFC.GeoCoding.Standard.AzureMaps.Model;
using DFC.JSON.Standard.Attributes;
using DFC.Swagger.Standard.Annotations;
using NCS.DSS.Sessions.ReferenceData;
using System.ComponentModel.DataAnnotations;

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
        [Example(Description = "true")]
        public bool? SessionAttended { get; set; }

        [Display(Description = "Reason For Non Attendance reference data.")]
        [Example(Description = "1")]
        public ReasonForNonAttendance? ReasonForNonAttendance { get; set; }

        [Display(Description = "Date and time of the last modification to the record.  This time is auto-generated by CDS 2.0 if a value is not supplied")]
        [Example(Description = "2018-06-21T14:45:00")]
        public DateTime? LastModifiedDate { get; set; }

        [StringLength(10, MinimumLength = 10)]
        [RegularExpression(@"^[0-9]+$")]
        [Display(Description = "Identifier of the touchpoint who made the last change to the record")]
        [Example(Description = "0000000001")]
        public string LastModifiedTouchpointId { get; set; }

        [StringLength(50)]
        [RegularExpression(@"^[0-9]+$")]
        [Display(Description = "Identifier supplied by the touchpoint to indicate their subcontractor")]
        [Example(Description = "01234567899876543210")]
        public string SubcontractorId { get; set; }

        [JsonIgnoreOnSerialize]
        public decimal? Longitude { get; set; }

        [JsonIgnoreOnSerialize]
        public decimal? Latitude { get; set; }

        [JsonIgnoreOnSerialize]
        public string CreatedBy { get; set; }

        public void SetDefaultValues()
        {
            SessionId = Guid.NewGuid();

            if (!LastModifiedDate.HasValue)
                LastModifiedDate = DateTime.UtcNow;

            if (ReasonForNonAttendance == null && SessionAttended == false)
                ReasonForNonAttendance = ReferenceData.ReasonForNonAttendance.NotKnown;

        }

        public void SetIds(Guid customerId, Guid interactionId, string touchpointId, string subcontractorId)
        {
            SessionId = Guid.NewGuid();
            CustomerId = customerId;
            InteractionId = interactionId;
            LastModifiedTouchpointId = touchpointId;
            SubcontractorId = subcontractorId;
            CreatedBy = touchpointId;
        }

        public void SetLongitudeAndLatitude(Position position)
        {
            if (position == null)
                return;

            Longitude = (decimal)position.Lon;
            Latitude = (decimal)position.Lat;

        }
    }
}