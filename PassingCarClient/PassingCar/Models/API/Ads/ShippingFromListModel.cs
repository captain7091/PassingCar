using System;
using System.Collections.Generic;

namespace PassingCar.Models.API.Ads
{
    public class ShippingFromListModel
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public ShippingState State { get; set; }
        public AddressDetails From { get; set; }
        public AddressDetails To { get; set; }
        public int OfferId { get; set; }
        public int AdsId { get; set; }
        public bool IsMyAds { get; set; }
        public string ConfirmationCode { get; set; }
        public List<ShippmentHistoryModel> History { get; set; }
    }
}
