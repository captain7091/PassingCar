namespace PassingCar.Hubs
{
    public class AdsHubMethods
    {
        public HubMethod NewAds => new HubMethod()
        {
            Hub = "AdsHub",
            Method = "NewAds"
        };
        public HubMethod ShippingStateChanged => new HubMethod()
        {
            Hub = "AdsHub",
            Method = "ShippingStateChanged"
        };
        public HubMethod AdsStateChanged => new HubMethod()
        {
            Hub = "AdsHub",
            Method = "AdsStateChanged"
        };
        public HubMethod OfferStateChanged => new HubMethod()
        {
            Hub = "AdsHub",
            Method = "OfferStateChanged"
        };
        public HubMethod NewOffer => new HubMethod()
        {
            Hub = "AdsHub",
            Method = "NewOffer"
        };
        public HubMethod NewNotification => new HubMethod()
        {
            Hub = "AdsHub",
            Method = "NewNotification"
        };
    }
    public class ChatHubMethods
    {
        public HubMethod NewMessage => new HubMethod()
        {
            Hub = "ChatHub",
            Method = "NewMessage"
        };
    }
    public class HubMethod
    {
        public string Hub { get; set; }
        public string Method { get; set; }
    }
}
