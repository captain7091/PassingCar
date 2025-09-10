using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using PassingCarApis.Models;
using PassingCarApis.Models.API.Ads;

namespace PassingCarApis.Hubs
{
    [Authorize]
    public class AdsHub : Hub
    {
        public Task NewAds(AdFromAdsListModel ads)
        {
            return Clients.All.SendAsync("NewAds", ads);
        }
        public Task ShippingStateChanged(ChangesShippingStateInput stateInput)
        {
            return Clients.All.SendAsync("ShippingStateChanged", stateInput);
        }
        public Task AdsStateChanged(ChangeAdsStateInput stateInput)
        {
            return Clients.All.SendAsync("AdsStateChanged", stateInput);
        }
        public Task OfferStateChanged(ChangeOfferStateInput stateInput)
        {
            return Clients.All.SendAsync("OfferStateChanged", stateInput);
        }
        public Task NewOffer(SendOfferInput newOffer)
        {
            return Clients.All.SendAsync("NewOffer", newOffer);
        }
        public async Task NewNotification(Notification notification)
        {
            await Clients.Group(notification.UserId.ToString()).SendAsync("NewNotification", notification);
        }
    }
}
