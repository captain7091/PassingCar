namespace PassingCar.Models.API.Validation
{
    public class SendValidationCodeOnPhoneRequest
    {
        public string PhoneNumber { get; set; }
        public bool ForRegister { get; set; }
        public bool Uber { get; set; }
        public bool Customer { get; set; }
        public bool JuridicPerson { get; set; }
    }
}
