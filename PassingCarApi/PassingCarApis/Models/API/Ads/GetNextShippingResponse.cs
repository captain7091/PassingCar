namespace PassingCarApis.Models.API.Ads
{
    public class GetNextShippingResponse : ApiBaseResponse
    {
        public IEnumerable<ShippingFromListModel>? ShippingItems { get; set; }
    }
}
