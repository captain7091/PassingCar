using System;
using System.Collections.Generic;

namespace PassingCar.Models.API.Ads
{
    public class CheckAdsUpdatesInput
    {
        public List<AdsUpdateDetails> AdsFromLocal { get; set; }
    }
    public class AdsUpdateDetails
    {
        public int AdId { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }
}
