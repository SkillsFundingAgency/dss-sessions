namespace NCS.DSS.Sessions.PostCodeSearch
{
    public class PostCodeSearchServiceOptions
    {
        public bool UseOsApi { get; set; } = false;
        public string ApiUrl { get; set; }
        public string ApiKey { get; set; }
    }
}
