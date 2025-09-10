namespace PassingCar.Models.API.Ads
{
    public class ChangesShippingStateInput
    {
        public int ShippingId { get; set; }
        public ShippingState State { get; set; }
    }
}
