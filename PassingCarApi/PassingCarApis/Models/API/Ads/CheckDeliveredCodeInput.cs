namespace PassingCarApis.Models.API.Ads
{
    public class CheckDeliveredCodeInput
    {
        public int ShippingId { get; set; }
        public string? ValidationCode { get; set; }
    }
}
