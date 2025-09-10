namespace PassingCar.Models.API.Ads
{
    public class ChangeOfferStateInput
    {
        public int OfferId { get; set; }
        public OfferState State { get; set; }
    }
}
