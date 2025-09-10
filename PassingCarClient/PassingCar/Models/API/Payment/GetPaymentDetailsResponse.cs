using System;

namespace PassingCar.Models.API.Payment
{
    public class GetPaymentDetailsResponse : ApiBaseResponse
    {
        public PaymentDetails PaymentDetails { get; set; }
    }
    public class PaymentDetails
    {
        public int PaymentId { get; set; }
        public int UserId { get; set; }
        public int OfferId { get; set; }
        public ChargeData ChargeRequest { get; set; }
        public ChargeResponse ChargeResponse { get; set; }
        public RefundData RefundRequest { get; set; }
        public RefundResponse RefundResponse { get; set; }
        public PaymentProofModel PaymentProofModel { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }
}
