namespace PassingCarApis.Models.API.Ads
{
    public class ChangeShippingStateResponse : ApiBaseResponse
    {
        public List<ShippmentHistoryModel>? History { get; set; }
    }
}
