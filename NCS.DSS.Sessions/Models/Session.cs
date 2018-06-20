using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using NCS.DSS.Sessions.ReferenceData;

namespace NCS.DSS.Sessions.Models
{
    public class Session
    {
        [Required]
        public Guid SessionId { get; set; }
        public Guid InteractionId { get; set; }
        public DateTime DateandTimeOfSession { get; set; }

        [RegularExpression(@"([Gg][Ii][Rr] 0[Aa]{2})|((([A-Za-z][0-9]{1,2})|(([A-Za-z][A-Ha-hJ-Yj-y][0-9]{1,2})|(([A - Za - z][0 - 9][A - Za - z]) | 
                                            ([A - Za - z][A - Ha - hJ - Yj - y][0 - 9]?[A - Za - z]))))\\s?[0-9] [A-Za-z]{2})", 
                                            ErrorMessage = "Postcode must be in valid UK format as specified in the UK Government Data Standard.")]
        [StringLength(10)]
        public string VenuePostCode { get; set; }
        public bool SessionAttended { get; set; }
        public ReasonForNonAttendance ReasonForNonAttendanceId { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public Guid LastModifiedTouchpointId { get; set; }
    }
}
