namespace NCS.DSS.Sessions.Models
{
    public class Interaction
    {
        public Guid? id { get; set; }
        public Guid? CustomerId { get; set; }
        public string TouchpointId { get; set; }
        public Guid? AdviserDetailsId { get; set; }

    }
}
