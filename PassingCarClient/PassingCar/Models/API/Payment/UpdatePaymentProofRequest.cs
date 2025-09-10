namespace PassingCar.Models.API.Payment
{
    public class UpdatePaymentProofRequest
    {
        public int PaymentId { get; set; }
        public PaymentProofModel ProofModel { get; set; }
    }
}
