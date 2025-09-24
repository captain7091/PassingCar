using Dapper;
using Microsoft.Data.SqlClient;
using PassingCarApis.Configuration;
using PassingCarApis.Extensions;
using PassingCarApis.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace PassingCarApis.Services
{
    public class LoginService : ILoginService
    {
        private readonly AuthService _authService;

        public LoginService(AuthService authService)
        {
            _authService = authService;
        }

        public async Task<BaseUserDetails> GetUserDetails(int userId)
        {
            BaseUserDetails response = new();
            using SqlConnection connection = AppConfiguration.GetConnection();
            connection.Open();
            try
            {
                response = await connection.QueryFirstOrDefaultAsync<BaseUserDetails>($@" Select Top 1 
                                    u.Photo as UserProfilePhoto,
                                    u.Name as UserName,
                                    u.JuridicDetails,
                                    CASE WHEN (SELECT AVG(Rating + 0.0)   FROM [dbo].[Review]   where ReviewedUserId = u.Id) is null THEN -1
                                    ELSE (SELECT AVG(Rating + 0.0)   FROM [dbo].[Review]   where ReviewedUserId = u.Id) end as UserRating
                                    From [User] where Id = @UserId", new
                {
                    UserId = userId,
                });
            }
            catch (Exception ex)
            {
                ex.CatchIt();
                response = new();
            }
            finally
            {
                connection.Close();
            }
            return response;
        }

        public UserIdAndProfile GetUserProfileFromToken(string jwtToken)
        {
            return _authService.GetUserProfileFromToken(jwtToken);
        }
    }
}
