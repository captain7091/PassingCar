using PassingCar.Utils;

namespace PassingCar.Models.API.User
{
    public class LoginRequest
    {
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string GoogleId { get; set; }
        public string AppleId { get; set; }
        public string FacebookId { get; set; }
        public string Password { get; set; }
        public ProfileType? Profile { get; set; }
        public bool OnlyToken { get; set; }
    }
}
