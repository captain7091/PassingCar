namespace PassingCar.Models.API.Ads
{
    public class GetPaymentProofResponse : ApiBaseResponse
    {
        public byte[] PaymentProofBytes { get; set; }
    }
}
