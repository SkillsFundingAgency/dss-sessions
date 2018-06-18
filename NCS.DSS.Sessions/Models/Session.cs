using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace NCS.DSS.Sessions.Models
{
    public class Session
    {
        [Required]
        public Guid SessionId { get; set; }
        public Guid InteractionId { get; set; }
        public DateTime DateandTimeOfSession { get; set; }

        [StringLength(10)]
        public string VenuePostCode { get; set; }
        public bool SessionAttended { get; set; }
        public int ReasonForNonAttendanceId { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public Guid LastModifiedTouchpointId { get; set; }
    }
}
