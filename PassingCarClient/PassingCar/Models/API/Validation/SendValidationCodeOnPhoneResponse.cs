using System;

namespace PassingCar.Models.API.Validation
{
    public class SendValidationCodeOnPhoneResponse
    {
        public bool Sent { get; set; }
        public string ErrorMessage { get; set; }
        public string Username { get; set; }
        public string Useremail { get; set; }
        public int UserId { get; set; }
        public DateTime ExpirationDate { get; set; }
    }
}
