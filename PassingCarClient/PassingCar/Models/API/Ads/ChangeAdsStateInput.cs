namespace PassingCar.Models.API.Ads
{
    public class ChangeAdsStateInput
    {
        public int AdId { get; set; }
        public AdsState State { get; set; }
    }
}
