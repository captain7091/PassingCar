namespace PassingCarApis.Models
{
    public class TaxDetails
    {
        public decimal Price { get; set; }
        public decimal RestPrice { get; set; }
        public decimal BaseImposit { get; set; }
        public decimal Imposit { get; set; }
        public decimal RestClientPrice { get; set; }
        public TaxDetails(decimal Price)
        {
            this.Price = Price;
            decimal passingCarComision = 0.20m * Price;
            passingCarComision = Math.Round(passingCarComision, 2);
            RestClientPrice = Price - passingCarComision;
            BaseImposit = passingCarComision / 1.21m;
            BaseImposit = Math.Round(BaseImposit, 2);
            Imposit = passingCarComision - BaseImposit;
            RestPrice = RestClientPrice;
        }

    }
}
