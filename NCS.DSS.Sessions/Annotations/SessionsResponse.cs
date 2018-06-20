using System;

namespace NCS.DSS.Sessions.Annotations
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class SessionsResponse : Attribute
    {
        public int HttpStatusCode { get; set; }
        public string Description { get; set; }
        public bool ShowSchema { get; set; }
    }
}
