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
            try
            {
                int userId = 0;
                ProfileType profile = ProfileType.Fisica;
                JwtSecurityToken jwtTokenDetails = new JwtSecurityTokenHandler().ReadJwtToken(jwtToken);
                IEnumerable<Claim> claimsWithIdVal = jwtTokenDetails.Claims.Where(c => c.Type == "Id");
                IEnumerable<Claim> claimsWithProfileVal = jwtTokenDetails.Claims.Where(c => c.Type == "Profile");
                if (claimsWithIdVal != null && claimsWithIdVal.Count() > 0)
                {
                    Claim userClaim = claimsWithIdVal.FirstOrDefault()!;
                    if (userClaim != null)
                    {
                        _ = int.TryParse(userClaim.Value, out userId);
                    }
                }
                if (claimsWithProfileVal != null && claimsWithProfileVal.Count() > 0)
                {
                    Claim userClaim = claimsWithProfileVal.FirstOrDefault()!;
                    if (userClaim != null)
                    {
                        _ = Enum.TryParse(userClaim.Value, out profile);
                    }
                }
                return new()
                {
                    Id = userId,
                    Profile = profile,
                };
            }
            catch (Exception ex)
            {
                ex.CatchIt();
                return new();
            }
        }
    }
}
