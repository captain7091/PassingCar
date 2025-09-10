using System;

namespace PassingCar.Models.API.Ads
{
    public class GetNextShippingInput
    {
        public DateTime FirstRequestTime { get; set; }
        public int LastShippingLoaded { get; set; }
    }
}
