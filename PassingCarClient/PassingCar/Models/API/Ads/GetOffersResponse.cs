using PassingCar.ViewModels;
using System.Collections.Generic;

namespace PassingCar.Models.API.Ads
{
    public class GetOffersResponse : ApiBaseResponse
    {
        public List<AdsParent> Ads { get; set; }
    }
}
