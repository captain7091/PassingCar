using Newtonsoft.Json;
using PassingCarApis.Extensions;

namespace PassingCarApis.Models.API.Ads
{
    public class ShippingFromListModel
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public ShippingState State { get; set; }
        public AddressDetails? From { get; set; }
        public AddressDetails? To { get; set; }
        public int OfferId { get; set; }
        public int AdsId { get; set; }
        public bool IsMyAds { get; set; }
        public string? ConfirmationCode { get; set; }
        public List<ShippmentHistoryModel>? History { get; set; }
        public ShippingFromListModel(ShippingFromDBModel shipFromDB)
        {
            if (shipFromDB != null)
            {
                Id = shipFromDB.Id;
                CreatedAt = shipFromDB.CreatedAt;
                State = shipFromDB.State;
                if (!string.IsNullOrEmpty(shipFromDB.From))
                {
                    try
                    {
                        From = JsonConvert.DeserializeObject<AddressDetails>(shipFromDB.From);
                    }
                    catch (Exception ex)
                    {
                        ex.CatchIt();
                        From = new AddressDetails();
                    }
                }
                else
                {
                    From = new AddressDetails();
                }
                if (!string.IsNullOrEmpty(shipFromDB.To))
                {
                    try
                    {
                        To = JsonConvert.DeserializeObject<AddressDetails>(shipFromDB.To);
                    }
                    catch (Exception ex)
                    {
                        ex.CatchIt();
                        To = new AddressDetails();
                    }
                }
                else
                {
                    To = new AddressDetails();
                }

                OfferId = shipFromDB.OfferId;
                AdsId = shipFromDB.AdsId;
                IsMyAds = shipFromDB.IsMyAds;
                ConfirmationCode = shipFromDB.ConfirmationCode;
                History = !string.IsNullOrEmpty(shipFromDB.History)
                    ? JsonConvert.DeserializeObject<List<ShippmentHistoryModel>>(shipFromDB.History)
                    : new List<ShippmentHistoryModel>();
            }
        }
    }
    public class ShippingFromDBModel
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public ShippingState State { get; set; }
        public string? From { get; set; }
        public string? To { get; set; }
        public int OfferId { get; set; }
        public int AdsId { get; set; }
        public bool IsMyAds { get; set; }
        public string? ConfirmationCode { get; set; }
        public string? History { get; set; }
    }
}
