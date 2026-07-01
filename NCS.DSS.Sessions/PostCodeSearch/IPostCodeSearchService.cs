namespace NCS.DSS.Sessions.PostCodeSearch
{
    public interface IPostCodeSearchService
    {
        Task<AddressPosition> GetPositionForPostcodeAsync(string postcode);
    }
}
