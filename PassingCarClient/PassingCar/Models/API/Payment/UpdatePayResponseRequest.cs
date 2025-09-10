using System;
using System.Collections.Generic;

namespace PassingCar.Models.API.Payment
{
    public class UpdatePayResponseRequest
    {
        public int PaymentId { get; set; }
        public ChargeResponse ChargeResponse { get; set; }
    }
    public class ChargeResponse
    {
        public string Id { get; set; }
        public string Object { get; set; }
        public long Amount { get; set; }
        public long AmountRefunded { get; set; }
        public string ApplicationId { get; set; }
        public long? ApplicationFeeAmount { get; set; }
        public string CalculatedStatementDescriptor { get; set; }
        public bool Captured { get; set; }
        public DateTime Created { get; set; }
        public string Currency { get; set; }
        public string Description { get; set; }
        public bool Disputed { get; set; }
        public string FailureCode { get; set; }
        public string FailureMessage { get; set; }
        public bool Livemode { get; set; }
        public Dictionary<string, string> Metadata { get; set; }
        public bool Paid { get; set; }
        public string PaymentMethod { get; set; }
        public string ReceiptEmail { get; set; }
        public string ReceiptNumber { get; set; }
        public string ReceiptUrl { get; set; }
        public bool Refunded { get; set; }
        public string StatementDescriptor { get; set; }
        public string StatementDescriptorSuffix { get; set; }
        public string Status { get; set; }
        public string TransferGroup { get; set; }
    }
}
