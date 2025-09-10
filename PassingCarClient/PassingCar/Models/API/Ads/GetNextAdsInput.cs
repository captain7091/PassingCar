using System;

namespace PassingCar.Models.API.Ads
{
    public class GetNextAdsInput
    {
        public AdsFilter Filter { get; set; }
        public DateTime FirstRequestTime { get; set; }
        public int LastAdsLoaded { get; set; }
    }
}
