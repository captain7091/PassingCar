namespace PassingCar.Models
{
    public class JuridicPersonDetails
    {
        public string CompanyName { get; set; }
        public string NIF { get; set; }
        public bool IsShippingCompany { get; set; }
        public AddressInfo AddressInfo { get; set; }
    }
}
