using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using PassingCarApis.Configuration;
using PassingCarApis.Extensions;
using PassingCarApis.Utils;
using PassingCarApis.Models;
using PassingCarApis.Models.API;
using PassingCarApis.Models.API.User;
using PassingCarApis.Services;
using Stripe;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Twilio.Http;

namespace PassingCarApis.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class UserController : PassingCarBaseController
    {
        private readonly IConfiguration _configuration;
        private readonly AuthService _authService;
        public UserController(IConfiguration configuration, INotificationService notificationService, IHubService hubService, ILoginService loginService, AuthService authService) : base(notificationService, hubService, loginService)
        {
            _configuration = configuration;
            _authService = authService;
        }
        [HttpPost(Name = "Register")]
        public async Task<InsertResponse> Register(User user)
        {
            using SqlConnection connection = AppConfiguration.GetConnection();
      try
      {
        await connection.OpenAsync();
      }
      catch (Exception ex) { }
            
            try
            {
                if (!string.IsNullOrEmpty(user.Email) && user.Id <= 0)
                {
                    IEnumerable<UserDB> thisEmailUsers = await connection.QueryAsync<UserDB>($"Select * from [User] where Email = @Email", new { user.Email });
                    if (thisEmailUsers != null && thisEmailUsers.Count() > 0)
                    {
                        return new InsertResponse()
                        {
                            Success = false,
                            Id = 0,
                            ErrorMessage = $"This email address already exists",
                        };
                    }
                }
                if (!string.IsNullOrEmpty(user.HashedPassword))
                {
                    user.HashedPassword = user.HashedPassword.EncryptString();
                }
                if (user.Id > 0)
                {
                    UserDB currentUser = connection.QuerySingle<UserDB>($"Select * from [User] where Id = @Id", new { user.Id });
                    UserDB dbUser = new(user);
                    dbUser.Uber = currentUser.Uber || dbUser.Uber;
                    dbUser.JuridicPerson = currentUser.JuridicPerson || dbUser.JuridicPerson;
                    dbUser.Customer = currentUser.Customer || dbUser.Customer;
                    connection.Update(dbUser);
                    return new InsertResponse()
                    {
                        Success = true,
                        Id = user.Id,
                        ErrorMessage = string.Empty,
                    };
                }
                else
                {
                    UserDB dbUser = new(user);
                    long id = connection.Insert(dbUser);
                    return id > 0
                        ? new InsertResponse()
                        {
                            Success = true,
                            Id = (int)id,
                            ErrorMessage = string.Empty,
                        }
                        : new InsertResponse()
                        {
                            Success = false,
                            Id = 0,
                            ErrorMessage = $"Invalid returned ID",
                        };
                }
            }
            catch (Exception ex)
            {
                ex.CatchIt();
                return new InsertResponse()
                {
                    Success = false,
                    Id = 0,
                    ErrorMessage = ex.Message,
                };
            }
            finally
            {
                connection.Close();
            }
        }
        [HttpPost(Name = "Update")]
        public async Task<ApiBaseResponse> Update(User user)
        {
            try
            {
                using SqlConnection connection = AppConfiguration.GetConnection();
                connection.Open();
                try
                {
                    UserDB dbUser = new(user);
                    string query = $@"UPDATE [dbo].[User]
                                   SET [FacebookId] = @FacebookId
                                      ,[GoogleId] = @GoogleId
                                      ,[RegistrationType] = @RegistrationType
                                      ,[Name] = @Name
                                      ,[Surname] = @Surname
                                      ,[PhoneNumber] = @PhoneNumber
                                      ,[Email] = @Email
                                      ,[EmailVerified] = @EmailVerified
                                      ,[Photo] = @Photo
                                      ,[HashedPassword] = @HashedPassword
                                      ,[Uber] = @Uber
                                      ,[Customer] = @Customer
                                      ,[JuridicPerson] = @JuridicPerson
                                      ,[JuridicDetails] = @JuridicDetails
                                      ,[Range] = @Range
                                      ,[CreatedAt] = @CreatedAt
                                      ,[ModifiedAt] = @ModifiedAt
                                      ,[PersonalInfo] = @PersonalInfo
                                      ,[Address] = @Address
                                 WHERE Id = @Id";
                    _ = await connection.QueryAsync(query, dbUser);

                    return new ApiBaseResponse()
                    {
                        Success = true,
                        ErrorMessage = string.Empty,
                    };
                }
                catch (Exception ex)
                {
                    ex.CatchIt();
                    return new ApiBaseResponse()
                    {
                        Success = false,
                        ErrorMessage = ex.Message,
                    };
                }
                finally
                {
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                ex.CatchIt();
                return new ApiBaseResponse()
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                };
            }
        }
        [HttpPost(Name = "CheckConnection")]
        public ApiBaseResponse CheckConnection()
        {
            return new ApiBaseResponse()
            {
                Success = true,
            };
        }
        [HttpGet(Name = "GetUsers")]
        public GetUsersResponse GetUsers()
        {
            GetUsersResponse response = new();
            using SqlConnection connection = AppConfiguration.GetConnection();
            connection.Open();
            try
            {
                IEnumerable<UserDB> users = connection.GetAll<UserDB>();
                List<User> result = new();
                if (users != null && users.Count() > 0)
                {
                    foreach (UserDB item in users)
                    {
                        result.Add(new User(item));
                    }
                }
                response.Result = result;
                response.Success = true;
                response.ErrorMessage = string.Empty;
            }
            catch (Exception ex)
            {
                ex.CatchIt();
                response.Result = new List<User>();
                response.Success = false;
                response.ErrorMessage = ex.Message;
            }
            finally
            {
                connection.Close();
            }
            return response;
        }
        [HttpPost(Name = "Login")]
        public async Task<LoginResponse> Login(LoginRequest request)
        {
            try
            {
                LoginResponse response = new();
                if (request != null)
                {
                    if (string.IsNullOrEmpty(request.Email) && string.IsNullOrEmpty(request.PhoneNumber) && string.IsNullOrEmpty(request.GoogleId) && string.IsNullOrEmpty(request.FacebookId))
                    {
                        response = new LoginResponse()
                        {
                            Success = false,
                            ErrorMessage = $"Email or PhoneNumber is Required",
                            UserData = null
                        };
                    }
                    else
                    {
                        bool loginViaExternalId = string.IsNullOrEmpty(request.Password) && (!string.IsNullOrEmpty(request.GoogleId) || !string.IsNullOrEmpty(request.FacebookId));

                        if (!string.IsNullOrEmpty(request.Password) || loginViaExternalId)
                        {
                            request.Password ??= string.Empty;
                            string Password = request.Password.Trim().EncryptString();
                            using SqlConnection connection = AppConfiguration.GetConnection();
                            await connection.OpenAsync();
                            try
                            {
                                IEnumerable<UserDB> usersWithThisEmailAndPass = !loginViaExternalId
                                    ? string.IsNullOrEmpty(request.Email) ? await connection.QueryAsync<UserDB>($"Select * from [User] where PhoneNumber = @PhoneNumber AND HashedPassword = @Password", new { request.PhoneNumber, Password }) : await connection.QueryAsync<UserDB>($"Select * from [User] where Email = @Email AND HashedPassword = @Password", new { request.Email, Password })
                                    : !string.IsNullOrEmpty(request.GoogleId)
                                        ? await connection.QueryAsync<UserDB>($"Select * from [User] where GoogleId = @GoogleId OR (Email = @Email AND Email <> '' AND Email Is not NULL)", new { request.GoogleId, request.Email })
                                        : await connection.QueryAsync<UserDB>($"Select * from [User] where FacebookId = @FacebookId OR (Email = @Email AND Email <> '' AND Email Is not NULL)", new { request.FacebookId, request.Email });
                                if (usersWithThisEmailAndPass.HasValues())
                                {
                                    User user = new(usersWithThisEmailAndPass.First());
                                    ProfileType profile = ProfileType.Fisica;
                                    List<ProfileType> types = new List<ProfileType>();
                                    if (user.Uber)
                                        types.Add(ProfileType.JuridicaTransporte);
                                    if (user.Customer)
                                        types.Add(ProfileType.Fisica);
                                    if (user.JuridicPerson)
                                        types.Add(ProfileType.Juridica);
                                    if (types.Any() && types.Count == 1)
                                        profile = types.FirstOrDefault();
                                    string token = _authService.GenerateJwtToken(user, profile);
                                    response = new LoginResponse()
                                    {
                                        Success = true,
                                        ErrorMessage = string.Empty,
                                        UserData = request.OnlyToken ? null : user,
                                        Token = token,
                                    };
                                }
                                else
                                {
                                    response = new LoginResponse()
                                    {
                                        Success = false,
                                        ErrorMessage = $"Wrong username or password",
                                        UserData = null
                                    };
                                }
                            }
                            catch (Exception ex)
                            {
                                ex.CatchIt();
                                response = new LoginResponse()
                                {
                                    Success = false,
                                    ErrorMessage = $"Something went wrong. Details: {ex.Message}",
                                    UserData = null
                                };
                            }
                            finally
                            {
                                connection.Close();
                            }
                        }
                        else
                        {
                            response = new LoginResponse()
                            {
                                Success = false,
                                ErrorMessage = $"Password is Required for login via email",
                                UserData = null
                            };
                        }

                    }
                }
                else
                {
                    response = new LoginResponse()
                    {
                        Success = false,
                        ErrorMessage = $"Login Request is NULL",
                        UserData = null
                    };
                }
                return response;
            }
            catch (Exception ex)
            {
                ex.CatchIt();
                return new LoginResponse()
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                };
            }
        }



        [HttpPost(Name = "GetUserFromApiValid")]
        public async Task<ActionResult> GetUserFromApiValid(User user_)
        {

            using SqlConnection connection = AppConfiguration.GetConnection();
            connection.Open();
            try
            {
                var getLastValidationQuary = $@"
                    SELECT   Top(1)
      [CreatedAt]
  FROM [ValidationCode] where PhoneNumber = '{user_.PhoneNumber}'   ORDER BY[Id] DESC
                    ";

                var lastvalidation = await connection.QueryFirstOrDefaultAsync<DateTime>(getLastValidationQuary);

                if(lastvalidation != null)
                {
                    if(lastvalidation .AddMinutes(30) > DateTime.Now)
                    {
                        var getUserQuery = $@"
                            SELECT * FROM [User] WHERE PhoneNumber = '{user_.PhoneNumber}'
                        ";
                        var user = await connection.QueryFirstOrDefaultAsync<UserDB>(getUserQuery);
                        if (user != null)
                        {
                            return Ok( new User(user));
                        }
                    }
                    else
                    {
                        return Ok(new ApiBaseResponse { ErrorMessage = "Validation expired" , Success=false }); // Validation expired
                    }
                }


            }
            catch (Exception ex)
            {
                ex.CatchIt();
            }

            return Ok(new ApiBaseResponse { ErrorMessage = "", Success = false }); // Validation expired



        }


        [HttpPost(Name = "GetUserPublicData")]
        public async Task<UserPublicData> GetUserPublicData(GetUserProfile request)
        {
            using SqlConnection connection = AppConfiguration.GetConnection();
            connection.Open();
            try
            {
                UserPublicData response = new();
                try
                {
                    string query = $@"Select Count(Id) from [Ads] as PublishedAds
                               where UserId = @UserId";
                    response.PublishedAds = await connection.QueryFirstOrDefaultAsync<int>(query, new { request.UserId });
                }
                catch (Exception ex)
                {
                    ex.CatchIt();
                    response.ErrorMessage += $"PublishedAds: {ex.Message}";
                }
                try
                {
                    string query = $@"Select Count(Id) from [Ads] as PublishedAds
                               where UserId = @UserId
	                           and State = @DeliveredState";
                    response.SentPackages = await connection.QueryFirstOrDefaultAsync<int>(query, new { request.UserId, DeliveredState = AdsState.Delivered });
                }
                catch (Exception ex)
                {
                    ex.CatchIt();
                    response.ErrorMessage += $"SentPackages: {ex.Message}";
                }
                try
                {
                    string query = $@"Select Count(s.Id) from Shipping s
	                           inner join Offer o
	                           on s.OfferId = o.Id
                               where o.UserId = @UserId
	                            and ( s.State = @DeliveredState  OR s.State = @FinalizedState)";
                    response.TransportedPackages = await connection.QueryFirstOrDefaultAsync<int>(query,
                        new
                        {
                            request.UserId,
                            DeliveredState = ShippingState.Delivered,
                            FinalizedState = ShippingState.Finalized
                        });
                }
                catch (Exception ex)
                {
                    ex.CatchIt();
                    response.ErrorMessage += $"TransportedPackages: {ex.Message}";
                }
                try
                {
                    string query = $@"Select Count(s.Id) from Shipping s
	                           inner join Offer o
	                           on s.OfferId = o.Id
	                           inner join Ads a
	                           on a.Id = o.AdId
                               where (o.UserId = @UserId OR a.UserId = @UserId)
	                           and s.State = @CanceledState";
                    response.CanceledDeliveries = await connection.QueryFirstOrDefaultAsync<int>(query,
                        new
                        {
                            request.UserId,
                            CanceledState = ShippingState.Canceled
                        });
                }
                catch (Exception ex)
                {
                    ex.CatchIt();
                    response.ErrorMessage += $"CanceledDeliveries: {ex.Message}";
                }
                try
                {
                    string query = $@"Select Count(Id) from Offer
                               WHERE UserId = @UserId";
                    response.OffersSent = await connection.QueryFirstOrDefaultAsync<int>(query,
                        new
                        {
                            request.UserId,
                        });
                }
                catch (Exception ex)
                {
                    ex.CatchIt();
                    response.ErrorMessage += $"OffersSent: {ex.Message}";
                }
                try
                {
                    string query = $@"Select sum(CAST(o.Message as decimal)) from Shipping s
	                           inner join Offer o
	                           on s.OfferId = o.Id
	                           inner join Ads a
	                           on a.Id = o.AdId
                               where o.UserId = @UserId
	                            and ( s.State = @DeliveredState  OR s.State = @FinalizedState)";
                    response.SpentMoney = await connection.QueryFirstOrDefaultAsync<decimal>(query,
                        new
                        {
                            request.UserId,
                            DeliveredState = ShippingState.Delivered,
                            FinalizedState = ShippingState.Finalized
                        });
                }
                catch (Exception ex)
                {
                    ex.CatchIt();
                    response.ErrorMessage += $"SpentMoney: {ex.Message}";
                }
                try
                {
                    string query = $@"Select sum(CAST(o.Message as decimal)) from Shipping s
	                           inner join Offer o
	                           on s.OfferId = o.Id
	                           inner join Ads a
	                           on a.Id = o.AdId
                               where o.UserId = @UserId
	                           and ( s.State = @DeliveredState  OR s.State = @FinalizedState)";
                    response.EarnedMoney = await connection.QueryFirstOrDefaultAsync<decimal>(query,
                        new
                        {
                            request.UserId,
                            DeliveredState = ShippingState.Delivered,
                            FinalizedState = ShippingState.Finalized
                        });
                    if (response.EarnedMoney > 0)
                    {
                        response.EarnedMoney -= 0.1m * response.EarnedMoney;
                    }

                    query = $@"Select Top 1 CASE WHEN (SELECT AVG(Rating + 0.0)   FROM [dbo].[Review]   where ReviewedUserId = @UserId) is null THEN -1
                                    ELSE (SELECT AVG(Rating + 0.0)   FROM [dbo].[Review]   where ReviewedUserId = @UserId) end as UserRating
                                    from [User]";

                    response.Rating = await connection.QueryFirstOrDefaultAsync<double>(query, new { request.UserId });
                }
                catch (Exception ex)
                {
                    ex.CatchIt();
                    response.ErrorMessage += $"SpentMoney: {ex.Message}";
                }
                response.Name = connection.QueryFirstOrDefault<string>($@"Select Top 1 Surname from [User] where Id = @UserId", new { request.UserId });
                response.CreatedAt = connection.QueryFirstOrDefault<DateTime>($@"Select Top 1 CreatedAt from [User] where Id = @UserId", new { request.UserId });

                response.Success = true;
                return response;
            }
            catch (Exception ex)
            {
                ex.CatchIt();
                return new UserPublicData()
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                };
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }
        }
    }
}
