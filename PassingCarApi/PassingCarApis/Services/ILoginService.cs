using PassingCarApis.Models;

namespace PassingCarApis.Services
{
    public interface ILoginService
    {
        public UserIdAndProfile GetUserProfileFromToken(string jwtToken);
        public Task<BaseUserDetails> GetUserDetails(int userId);
    }
}
