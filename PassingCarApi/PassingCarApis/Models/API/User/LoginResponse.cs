namespace PassingCarApis.Models.API.User
{
    public class LoginResponse : ApiBaseResponse
    {
        public Models.User? UserData { get; set; }
        public string? Token { get; set; }
    }
}
