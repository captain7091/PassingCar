namespace PassingCar.Models.API.Validation
{
    public class CheckPhoneValidationCodeRequest
    {
        public string PhoneNumber { get; set; }
        public string ValidationCode { get; set; }
    }
}
