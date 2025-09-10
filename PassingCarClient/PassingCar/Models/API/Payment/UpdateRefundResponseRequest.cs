using System;
using System.Collections.Generic;

namespace PassingCar.Models.API.Payment
{
    public class UpdateRefundResponseRequest
    {
        public int PaymentId { get; set; }
        public RefundResponse Refund { get; set; }
    }
    public class RefundResponse
    {
        public string Id { get; set; }
        public string Object { get; set; }
        public long Amount { get; set; }
        public string BalanceTransactionId { get; set; }
        public string ChargeId { get; set; }
        public DateTime Created { get; set; }
        public string Currency { get; set; }
        public string Description { get; set; }
        public string FailureBalanceTransactionId { get; set; }
        public string FailureReason { get; set; }
        public string InstructionsEmail { get; set; }
        public Dictionary<string, string> Metadata { get; set; }
        public string PaymentIntentId { get; set; }
        public string Reason { get; set; }
        public string ReceiptNumber { get; set; }
        public string SourceTransferReversalId { get; set; }
        public string Status { get; set; }
    }
}
