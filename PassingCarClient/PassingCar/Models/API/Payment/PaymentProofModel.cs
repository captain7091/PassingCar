using System;

namespace PassingCar.Models.API.Payment
{
    public class PaymentProofModel
    {
        public string ClientName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public int ZipCode { get; set; }
        public string NIF { get; set; }
        public int NoFactura { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime ExpDate { get; set; }
        public string Packet { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }
        public decimal Price { get; set; }
        public decimal RestPrice { get; set; }
        public decimal BaseImposit { get; set; }
        public decimal Imposit { get; set; }
        public decimal RestClientPrice { get; set; }
    }
}
