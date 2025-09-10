namespace PassingCarApis.Models.API.Ads
{
    public class GetNextAdsResponse : ApiBaseResponse
    {
        public IEnumerable<AdFromAdsListModel>? AdsItem { get; set; }
    }
}
