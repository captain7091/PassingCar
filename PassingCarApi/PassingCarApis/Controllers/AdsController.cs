using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PassingCarApis.Configuration;
using PassingCarApis.Extensions;
using PassingCarApis.Models;
using PassingCarApis.Models.API;
using PassingCarApis.Models.API.Ads;
using PassingCarApis.Models.API.Ads.Photo;
using PassingCarApis.Models.API.NotificationNS;
using PassingCarApis.Models.API.Payment;
using PassingCarApis.Models.API.User;
using PassingCarApis.Services;
using Stripe;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Twilio.Http;
using Review = PassingCarApis.Models.Review;

namespace PassingCarApis.Controllers
{
  [Authorize(AuthenticationSchemes = "Bearer")] 
  [Route("[controller]/[action]")]
    [ApiController]
    public class AdsController : PassingCarBaseController
    {
        private UserIdAndProfile? userIdAndProfile;
        public AdsController(INotificationService notificationService, IHubService hubService, ILoginService loginService) : base(notificationService, hubService, loginService)
        {
        }
        private async Task<UserIdAndProfile> GetUserIdAndProfileAsync()
        {
            if (userIdAndProfile == null)
            {
                // TEMPORARY FIX: For testing without authentication
                // TODO: Re-enable proper token validation
                try
                {
                    string? token = await HttpContext.GetTokenAsync("access_token");
                    if (!string.IsNullOrEmpty(token))
                    {
                        userIdAndProfile = loginService.GetUserProfileFromToken(token);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[AdsController] Token validation failed: {ex.Message}");
                }
                
                // Fallback for testing - use default user
                if (userIdAndProfile == null || userIdAndProfile.Id <= 0)
                {
                    userIdAndProfile = new UserIdAndProfile
                    {
                        Id = 1, // Default test user ID
                        Profile = ProfileType.Fisica // Default profile
                    };
                    System.Diagnostics.Debug.WriteLine("[AdsController] Using fallback user for testing");
                }
            }
            return userIdAndProfile;
        }
        [HttpPost(Name = "HandleException")]
        public async Task<bool> HandleException(HandleExceptionInput ex)
        {
            UserIdAndProfile loggedUser = await GetUserIdAndProfileAsync();
            if (ex != null && !string.IsNullOrEmpty(ex.Exception))
            {
                if (loggedUser.Id > 0)
                {
                    ex.Exception.AddToLog(loggedUser.Id);
                }
                else
                {
                    ex.Exception.AddToLog(0);
                }
            }
            return true;
        }
    
        [HttpPost(Name = "Add")]
        public async Task<InsertResponse> Add(AddAdsModel adsModel)
        {
            UserIdAndProfile loggedUser = await GetUserIdAndProfileAsync();
            if (loggedUser.Id > 0)
            {
                using SqlConnection connection = AppConfiguration.GetConnection();
                connection.Open();
                try
                {
                    Ads ads = new()
                    {
                        UserId = loggedUser.Id,
                        Title = adsModel.Title,
                        Photo1 = !string.IsNullOrEmpty(adsModel.Photo1) ? Convert.FromBase64String(adsModel.Photo1) : null,
                        Photo2 = !string.IsNullOrEmpty(adsModel.Photo2) ? Convert.FromBase64String(adsModel.Photo2) : null,
                        Photo3 = !string.IsNullOrEmpty(adsModel.Photo3) ? Convert.FromBase64String(adsModel.Photo3) : null,
                        Photo4 = !string.IsNullOrEmpty(adsModel.Photo4) ? Convert.FromBase64String(adsModel.Photo4) : null,
                        Description = adsModel.Description,
                        From = JsonConvert.SerializeObject(adsModel.From),
                        To = JsonConvert.SerializeObject(adsModel.To),
                        Price = adsModel.Price,
                        Size = JsonConvert.SerializeObject(adsModel.Size),
                        Weight = adsModel.Weigth,
                        Fragile = adsModel.IsFragile.ToString(),
                        UserProfile = loggedUser.Profile,
                        CreatedAt = DateTime.Now,
                        State = AdsState.Posted
                    };
                    long id = connection.Insert(ads);
                    System.Diagnostics.Debug.WriteLine($"[API] Ad created with ID: {id}, State: {ads.State}, UserId: {ads.UserId}");
                    if (id > 0)
                    {
                        ads.ID = (int)id;
                        try
                        {
                            BaseUserDetails user = await loginService.GetUserDetails(loggedUser.Id);
                            AdFromAdsListModel adItem = new()
                            {
                                AdsFrom = JsonConvert.SerializeObject(adsModel.From),
                                AdsId = (int)id,
                                State = AdsState.Posted,
                                AdsPrice = adsModel.Price,
                                AdsTitle = adsModel.Title,
                                UserProfile = adsModel.ProfileType,
                                AdsTo = JsonConvert.SerializeObject(adsModel.To),
                                FirstAdsImage = (!string.IsNullOrEmpty(adsModel.Photo1))
                                    ? Convert.FromBase64String(adsModel.Photo1)
                                    : (!string.IsNullOrEmpty(adsModel.Photo2))
                                    ? Convert.FromBase64String(adsModel.Photo2)
                                    : (!string.IsNullOrEmpty(adsModel.Photo3))
                                    ? Convert.FromBase64String(adsModel.Photo3)
                                    : (!string.IsNullOrEmpty(adsModel.Photo4))
                                    ? Convert.FromBase64String(adsModel.Photo4)
                                    : null,
                                IsFavorite = false,
                                PostedTime = DateTime.Now,
                                UserName = user != null ? (loggedUser.Profile == ProfileType.Fisica ? user.UserName : user.JuridicName) : string.Empty,
                                UserProfilePhoto = user?.UserProfilePhoto,
                                UserRating = user != null ? user.UserRating : 0,
                                UserId = loggedUser.Id
                            };
                            await hubService.SendHubMessage("AdsHub", await HttpContext.GetTokenAsync("access_token") ?? string.Empty, "NewAds", adItem);
                        }
                        catch (Exception ex)
                        {
                            ex.CatchIt();
                        }
                        return new InsertResponse()
                        {
                            Success = true,
                            Id = (int)id,
                            ErrorMessage = string.Empty,
                        };
                    }
                    else
                    {
                        return new InsertResponse()
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
            else
            {
                return new InsertResponse()
                {
                    Success = false,
                    Id = 0,
                    ErrorMessage = $"You must be logged in!",
                };
            }
        }
        [HttpPost(Name = "SendOffer")]
        public async Task<SendOfferResult> SendOffer(SendOfferInput input)
        {
            SendOfferResult response = new();

            UserIdAndProfile loggedUser = await GetUserIdAndProfileAsync();
            if (loggedUser.Id > 0)
            {
                using SqlConnection connection = AppConfiguration.GetConnection();
                connection.Open();
                try
                {
                    Offer offer = new()
                    {
                        AdId = input.AdId,
                        UserId = loggedUser.Id,
                        UserProfile = loggedUser.Profile,
                        CreatedAt = DateTime.Now,
                        Message = input.Message,
                        State = OfferState.Sent
                    };
                    long id = connection.Insert(offer);
                    if (id > 0)
                    {
                        response.OfferId = int.Parse(id.ToString());
                        response.Success = true;
                        response.ErrorMessage = string.Empty;
                        string query = $"Select UserId from Ads Where Id = @Id";
                        int adsuserId = await connection.QueryFirstOrDefaultAsync<int>(query, new { Id = input.AdId });
                        query = $"Select UserProfile from Ads Where Id = @Id";
                        string profileToLog = await connection.QueryFirstOrDefaultAsync<string>(query, new { Id = input.AdId });
                        int value;
                        ProfileType adsuserProfile = int.TryParse(profileToLog, out value) ? (ProfileType)value : (ProfileType)Enum.Parse(typeof(ProfileType), profileToLog);

                        query = $"Select Title from Ads Where Id = @Id";
                        string adsTitle = await connection.QueryFirstOrDefaultAsync<string>(query, new { Id = input.AdId });

                        try
                        {
                            await hubService.SendHubMessage("AdsHub", await HttpContext.GetTokenAsync("access_token") ?? string.Empty, "NewOffer", input);
                            _ = await notificationService.AddNotification(hubService, await HttpContext.GetTokenAsync("access_token") ?? string.Empty, new Notification
                            {
                                CreatedAt = DateTime.Now,
                                Message = $"Tienes nueva oferta de {input.Message} para {adsTitle}",
                                ShellRoute = $"//MyAdsPage",
                                UserId = adsuserId,
                                UserProfile = adsuserProfile
                            });
                        }
                        catch (Exception ex)
                        {
                            ex.CatchIt();
                        }
                    }
                    else
                    {
                        response.Success = false;
                        response.ErrorMessage = $"Invalid returned ID";
                    }
                }
                catch (Exception ex)
                {
                    ex.CatchIt();
                    response.Success = false;
                    response.ErrorMessage = ex.Message;
                }
                finally
                {
                    connection.Close();
                }
            }
            else
            {
                response.Success = false;
                response.ErrorMessage = $"You must be logged in!";
            }
            return response;
        }
        [HttpPost(Name = "ChangeOfferState")]
        public async Task<ApiBaseResponse> ChangeOfferState(ChangeOfferStateInput input)
        {
            ApiBaseResponse response = new();

            UserIdAndProfile loggedUser = await GetUserIdAndProfileAsync();
            if (loggedUser.Id > 0)
            {
                using SqlConnection connection = AppConfiguration.GetConnection();
                connection.Open();
                try
                {
                    string query = $@" Update Offer Set State = @State,
                                ModifiedAt = GETDATE()
                                where Id = @Id";
                    _ = await connection.QueryAsync(query, new { Id = input.OfferId, input.State });
                    response.Success = true;
                    response.ErrorMessage = string.Empty;

                    try
                    {
                        await hubService.SendHubMessage("AdsHub", await HttpContext.GetTokenAsync("access_token") ?? string.Empty, "OfferStateChanged", input);
                    }
                    catch (Exception ex)
                    {
                        ex.CatchIt();
                    }
                     query = $"Select UserId from Offer Where Id = @Id";
                     int adsuserId = await connection.QueryFirstOrDefaultAsync<int>(query, new { Id = input.OfferId });
                     query = $@"Select AdId from Offer o
                                             where Id = @Id";
                     int adId = await connection.QueryFirstOrDefaultAsync<int>(query, new { Id = input.OfferId });
                    query = $"Select Title from Ads Where Id = @Id";
                    string adsTitle = await connection.QueryFirstOrDefaultAsync<string>(query, new { Id = adId });

                    query = $"Select UserProfile from Ads Where Id = @Id";
                    string profileToLog = await connection.QueryFirstOrDefaultAsync<string>(query, new { Id = adId });
                    int value;
                    ProfileType adsuserProfile = int.TryParse(profileToLog, out value) ? (ProfileType)value : (ProfileType)Enum.Parse(typeof(ProfileType), profileToLog);


                    _ = await notificationService.AddNotification(hubService, await HttpContext.GetTokenAsync("access_token") ?? string.Empty, new Notification
                    {
                        CreatedAt = DateTime.Now,
                        Message = NotificationMessage(input.State, adsTitle),
                        ShellRoute = $"//MyShippingPage" ,
                        UserId = adsuserId,
                        UserProfile = adsuserProfile
                    });
                    AdsState? adsState;
                    switch (input.State)
                    {
                        case OfferState.Pending:
                            adsState = null;
                            break;
                        case OfferState.Sent:
                            adsState = null;
                            break;
                        case OfferState.Seen:
                            adsState = null;
                            break;
                        case OfferState.PaymentPending:
                            adsState = AdsState.PaymentPending;
                            break;
                        case OfferState.Accepted:
                            try
                            {
                                query = $"Select Email from [User] where Id = @UserId";
                                EmailService seriveEmail = new();
                                EmailSendReq request = new()
                                {
                                    Body = GetEmailBody(),
                                    OfferId = input.OfferId,
                                    To = connection.QueryFirstOrDefault<string>(query, new { UserId = loggedUser.Id })
                                };
                                ApiBaseResponse sendEmailResponse = seriveEmail.Send(request);
                                JsonConvert.SerializeObject(new
                                {
                                    request.To,
                                    response
                                }).AddToLog();
                                query = @$"Select u.Email
                                    from Offer o 
                                    inner join [User] u
                                    on o.UserId = u.Id
                                    where o.Id =  @OfferId";
                                request = new EmailSendReq()
                                {
                                    Body = GetEmailBody(),
                                    OfferId = input.OfferId,
                                    To = connection.QueryFirstOrDefault<string>(query, new { input.OfferId })
                                };
                                response = seriveEmail.Send(request);
                                JsonConvert.SerializeObject(new
                                {
                                    request.To,
                                    response
                                }).AddToLog();
                            }
                            catch (Exception ex)
                            {
                                ex.CatchIt();
                            }
                            adsState = AdsState.InTransit;
                            break;
                        case OfferState.Rejected:
                            adsState = null;
                            break;
                        case OfferState.Canceled:
                            adsState = null;
                            break;
                        default:
                            adsState = null;
                            break;
                    }
                    if (adsState.HasValue)
                    {
                        try
                        {
                            ChangeAdsStateInput changeStateInput = new()
                            {
                                AdId = adId,
                                State = adsState.Value
                            };
                            ApiBaseResponse adsStateChanged = await ChangeAdsState(changeStateInput);
                        }
                        catch (Exception ex)
                        {
                            ex.CatchIt();
                        }
                    }
                    if (input.State == OfferState.Accepted)
                    {
                        query = $@"Update Chat
                                            Set ModifiedAt = GETDATE(),
                                            State = @State
                                            where AdId in (Select AdId from Offer 
                                            where Id = @Id)
                                            and UberUserId in (Select UserId from Offer 
                                            where Id = @Id)";
                        _ = await connection.QueryAsync(query, new { Id = input.OfferId, State = ChateState.Open });

                        Models.Shipping shipping = new()
                        {
                            State = ShippingState.PendingForPickup,
                            OfferId = input.OfferId,
                            CreatedAt = DateTime.Now,
                            History = JsonConvert.SerializeObject(new List<ShippmentHistoryModel>()
                            {
                                new ShippmentHistoryModel()
                                {
                                    CreatedAt= DateTime.Now,
                                    Message =  "The payment was made successfully"
                                }                               ,
                                new ShippmentHistoryModel()
                                {
                                    CreatedAt= DateTime.Now.AddMinutes(1),
                                    Message =  "The shipment data has been successfully saved"
                                },
                                new ShippmentHistoryModel()
                                {
                                    CreatedAt= DateTime.Now.AddMinutes(1),
                                    Message =  "The chat was unlocked"
                                }
                            })
                        };
                        long id = connection.Insert(shipping);
                        if (id > 0)
                        {
                            response.Success = true;
                            response.ErrorMessage = string.Empty;
                        }
                        else
                        {
                            response.Success = false;
                            response.ErrorMessage = $"Failed to add shipping";
                        }
                    }
                }
                catch (Exception ex)
                {
                    ex.CatchIt();
                    response.Success = false;
                    response.ErrorMessage = ex.Message;
                }
                finally
                {
                    connection.Close();
                }
            }
            else
            {
                response.Success = false;
                response.ErrorMessage = $"You must be logged in!";
            }
            return response;
        }
        [HttpPost(Name = "ChangeAdsState")]
        public async Task<ApiBaseResponse> ChangeAdsState(ChangeAdsStateInput input)
        {
            ApiBaseResponse response = new();

            UserIdAndProfile loggedUser = await GetUserIdAndProfileAsync();

            if (loggedUser.Id > 0)
            {
                using SqlConnection connection = AppConfiguration.GetConnection();
                connection.Open();
                try
                {
                    string query = $@"Update Ads
                                            Set ModifiedAt = GETDATE(),
                                            State = @State
                                            where Id = @Id";
                    _ = await connection.QueryAsync(query, new { Id = input.AdId, input.State });
                    response.Success = true;
                    response.ErrorMessage = string.Empty;
                    try
                    {
                        await hubService.SendHubMessage("AdsHub", await HttpContext.GetTokenAsync("access_token") ?? string.Empty, "AdsStateChanged", input);
                    }
                    catch (Exception ex)
                    {
                        ex.CatchIt();
                    }
                }
                catch (Exception ex)
                {
                    ex.CatchIt();
                }
                finally
                {
                    connection.Close();
                }
            }
            else
            {
                response.Success = false;
                response.ErrorMessage = $"You must be logged in!";
            }
            return response;
        }
        [HttpPost(Name = "ChangeShippingState")]
        public async Task<ChangeShippingStateResponse> ChangeShippingState(ChangesShippingStateInput input)
        {
            ChangeShippingStateResponse response = new();
            UserIdAndProfile loggedUser = await GetUserIdAndProfileAsync();
            if (loggedUser.Id > 0)
            {
                using SqlConnection connection = AppConfiguration.GetConnection();
                connection.Open();
                try
                {
                    string query = $@"Select History from Shipping
                                            where Id = @Id";
                    string historySerialized = await connection.QueryFirstOrDefaultAsync<string>(query, new { Id = input.ShippingId });
                    List<ShippmentHistoryModel> history = new();
                    if (!string.IsNullOrEmpty(historySerialized))
                    {
                        history = JsonConvert.DeserializeObject<List<ShippmentHistoryModel>>(historySerialized) ?? new();
                    }
                    if (history != null)
                    {
                        ShippmentHistoryModel newHistory;
                        switch (input.State)
                        {
                            case ShippingState.PendingForPickup:
                                newHistory = new ShippmentHistoryModel()
                                {
                                    CreatedAt = DateTime.Now,
                                    Message = $"Shippment pending for pickup"
                                };
                                history.Add(newHistory);
                                break;
                            case ShippingState.UberPickupConfirmed:
                                newHistory = new ShippmentHistoryModel()
                                {
                                    CreatedAt = DateTime.Now,
                                    Message = $"Uber confirmed pickup"
                                };
                                history.Add(newHistory);
                                break;
                            case ShippingState.ClientPickupConfirmed:
                                newHistory = new ShippmentHistoryModel()
                                {
                                    CreatedAt = DateTime.Now,
                                    Message = $"Client confirmed pickup"
                                };
                                history.Add(newHistory);
                                string validationCode = new Random().Next(10000, 99999).ToString();
                                query = $@"Update Shipping
                                            Set ModifiedAt = GETDATE(),
                                             ConfirmationCode = @Code
                                            where Id = @Id";
                                _ = await connection.QueryAsync(query, new { Id = input.ShippingId, Code = validationCode });
                                query = $@"Select Top 1  a.[To]
                                    from Shipping s
                                    inner Join Offer o
                                    on o.Id = s.OfferId
                                    inner Join Ads a
                                    on a.Id = o.AdId
                                    where s.Id = @Id";
                                string addressTo = await connection.QueryFirstOrDefaultAsync<string>(query, new { Id = input.ShippingId });
                                if (!string.IsNullOrEmpty(addressTo))
                                {
                                    AddressDetails toAddress = JsonConvert.DeserializeObject<AddressDetails>(addressTo) ?? new();
                                    if (toAddress != null && !string.IsNullOrEmpty(toAddress.PhoneNumber))
                                    {
                                        SmsServices smsService = new();
                                        string message = $"Your package is on the way. The confirmation code for uber is {validationCode}";
                                        ApiBaseResponse sendResponse = await smsService.Send(toAddress.PhoneNumber, message);
                                        if (sendResponse.Success)
                                        {
                                            newHistory = new ShippmentHistoryModel()
                                            {
                                                CreatedAt = DateTime.Now,
                                                Message = $"Validation code was sent to {toAddress.PhoneNumber}"
                                            };
                                            history.Add(newHistory);
                                        }
                                        else
                                        {
                                            newHistory = new ShippmentHistoryModel()
                                            {
                                                CreatedAt = DateTime.Now,
                                                Message = $"No se pudo enviar el código de validación. {sendResponse.ErrorMessage}"
                                            };
                                            history.Add(newHistory);
                                        }
                                    }
                                    else
                                    {
                                        newHistory = new ShippmentHistoryModel()
                                        {
                                            CreatedAt = DateTime.Now,
                                            Message = $"No se pudo enviar el código de validación. Unable to take PhoneNumber to. Validation code is available to client on this page"
                                        };
                                        history.Add(newHistory);
                                    }
                                }
                                else
                                {
                                    newHistory = new ShippmentHistoryModel()
                                    {
                                        CreatedAt = DateTime.Now,
                                        Message = $"No se pudo enviar el código de validación. Unable to take PhoneNumber to. Validation code is available to client on this page"
                                    };
                                    history.Add(newHistory);
                                }
                                try
                                {
                                    query = $@"Select Top 1  o.AdId
                                    from Shipping s
                                    inner Join Offer o
                                    on o.Id = s.OfferId
                                    where s.Id = @Id";
                                    ChangeAdsStateInput changeStateInput = new()
                                    {
                                        AdId = await connection.QueryFirstOrDefaultAsync<int>(query, new { Id = input.ShippingId }),
                                        State = AdsState.InTransit
                                    };
                                    ApiBaseResponse adsStateChanged = await ChangeAdsState(changeStateInput);
                                }
                                catch (Exception ex)
                                {
                                    ex.CatchIt();
                                }
                                break;
                            case ShippingState.Delivered:
                                newHistory = new ShippmentHistoryModel()
                                {
                                    CreatedAt = DateTime.Now,
                                    Message = $"Package was delivered"
                                };
                                history.Add(newHistory);
                                break;
                            case ShippingState.Finalized:
                                newHistory = new ShippmentHistoryModel()
                                {
                                    CreatedAt = DateTime.Now,
                                    Message = $"Everything is done!"
                                };
                                history.Add(newHistory);
                                try
                                {
                                    query = $@"Select Top 1  o.AdId
                                    from Shipping s
                                    inner Join Offer o
                                    on o.Id = s.OfferId
                                    where s.Id = @Id";
                                    ChangeAdsStateInput changeStateInput = new()
                                    {
                                        AdId = await connection.QueryFirstOrDefaultAsync<int>(query, new { Id = input.ShippingId }),
                                        State = AdsState.Delivered
                                    };
                                    ApiBaseResponse adsStateChanged = await ChangeAdsState(changeStateInput);

                                    query = $@"Select Top 1  o.Message
                                    from Shipping s
                                    inner Join Offer o
                                    on o.Id = s.OfferId
                                    where s.Id = @Id";
                                    string amountStr = await connection.QueryFirstOrDefaultAsync<string>(query, new { Id = input.ShippingId });
                                    TaxDetails taxDetails = new(0);
                                    if (!string.IsNullOrEmpty(amountStr))
                                    {
                                        decimal amount = decimal.Parse(amountStr);
                                        if (amount > 0)
                                        {
                                            taxDetails = new(amount);
                                        }
                                    }

                                    query = $@"Select Sold from [User]
									Where Id = @UserId";

                                    string userSold = await connection.QueryFirstOrDefaultAsync<string>(query, new { UserId = loggedUser.Id, UserProfile = loggedUser.Profile.ToString() });

                                    Dictionary<ProfileType, decimal>? profilesSolds = null;
                                    if (!string.IsNullOrEmpty(userSold))
                                    {
                                        profilesSolds = JsonConvert.DeserializeObject<Dictionary<ProfileType, decimal>>(userSold);
                                        if (profilesSolds?.ContainsKey(loggedUser.Profile) ?? false)
                                        {
                                            profilesSolds[loggedUser.Profile] += (taxDetails.RestPrice - taxDetails.Price) / 100;
                                        }
                                        else
                                        {
                                            if (profilesSolds is null)
                                            {
                                                profilesSolds = new();
                                            }
                                            profilesSolds.Add(loggedUser.Profile, (taxDetails.RestPrice - taxDetails.Price) / 100);
                                        }
                                    }
                                    if (profilesSolds is null)
                                    {
                                        profilesSolds = new();
                                    }
                                    query = $@"UPDATE [User]  
                                                    SET Sold = @Sold
                                                where Id = @Id";
                                    _ = await connection.QueryAsync(query, new { Id = loggedUser.Id, Sold = JsonConvert.SerializeObject(profilesSolds) });

                                    Offer offer = await connection.QueryFirstOrDefaultAsync<Offer>($@"Select o.* from Offer o
                                                                        inner Join Shipping s
                                                                        on s.OfferId = o.Id
                                                                        where s.Id = @ShippingId", new { input.ShippingId });
                                    query = $@"Update Chat
                                            Set ModifiedAt = GETDATE(),
                                            State = @State
                                            where AdId in (Select AdId from Offer 
                                            where Id = @Id)
                                            and UberUserId in (Select UserId from Offer 
                                            where Id = @Id)";
                                    _ = await connection.QueryAsync(query, new { offer.Id, State = ChateState.Closed });
                                }
                                catch (Exception ex)
                                {
                                    ex.CatchIt();
                                }
                                break;
                            case ShippingState.Canceled:
                                try
                                {
                                    query = $@"Select Top 1  o.Message
                                    from Shipping s
                                    inner Join Offer o
                                    on o.Id = s.OfferId
                                    where s.Id = @Id";
                                    string amountStr = await connection.QueryFirstOrDefaultAsync<string>(query, new { Id = input.ShippingId });
                                    TaxDetails taxDetails = new(0);
                                    if (!string.IsNullOrEmpty(amountStr))
                                    {
                                        decimal amount = decimal.Parse(amountStr);
                                        if (amount > 0)
                                        {
                                            taxDetails = new(amount);
                                        }
                                    }
                                    query = $@"Select Top 1  o.UserId
                                    from Shipping s
                                    inner Join Offer o
                                    on o.Id = s.OfferId
                                    where s.Id = @Id";
                                    int canceledUserId = await connection.QueryFirstOrDefaultAsync<int>(query, new { Id = input.ShippingId });
                                    decimal amountToRefund;
                                    JsonConvert.SerializeObject(new { canceledUserId, loggedUser.Id, step = "Before update sold" }).AddToLog();
                                    if (canceledUserId == loggedUser.Id)
                                    {
                                        newHistory = new ShippmentHistoryModel()
                                        {
                                            CreatedAt = DateTime.Now,
                                            Message = $"Shippent was canceled by transporter"
                                        };
                                        history.Add(newHistory);
                                        newHistory = new ShippmentHistoryModel()
                                        {
                                            CreatedAt = DateTime.Now,
                                            Message = $"Money will be fully refunded in 3-5 working days. Transporter Sold was decreased"
                                        };
                                        history.Add(newHistory);
                                        amountToRefund = taxDetails.Price;


                                        query = $@"Select Sold from [User]
									Where Id = @UserId";

                                        string userSold = await connection.QueryFirstOrDefaultAsync<string>(query, new { UserId = loggedUser.Id, UserProfile = loggedUser.Profile.ToString() });

                                        Dictionary<ProfileType, decimal>? profilesSolds = null;
                                        if (!string.IsNullOrEmpty(userSold))
                                        {
                                            profilesSolds = JsonConvert.DeserializeObject<Dictionary<ProfileType, decimal>>(userSold);
                                            if (profilesSolds?.ContainsKey(loggedUser.Profile) ?? false)
                                            {
                                                profilesSolds[loggedUser.Profile] -= (taxDetails.RestPrice - taxDetails.Price) / 100;
                                            }
                                            else
                                            {
                                                if (profilesSolds is null)
                                                {
                                                    profilesSolds = new();
                                                }
                                                profilesSolds.Add(loggedUser.Profile, -1 * ((taxDetails.RestPrice - taxDetails.Price) / 100));
                                            }
                                        }
                                        if (profilesSolds is null)
                                        {
                                            profilesSolds = new();
                                        }
                                        query = $@"UPDATE [User]  
                                                    SET  Sold =  @Sold
                                                where Id = @Id";
                                        _ = await connection.QueryAsync(query, new { Id = loggedUser.Id, Sold = JsonConvert.SerializeObject(profilesSolds) });
                                    }
                                    else
                                    {
                                        newHistory = new ShippmentHistoryModel()
                                        {
                                            CreatedAt = DateTime.Now,
                                            Message = $"Shippent was canceled by client"
                                        };
                                        history.Add(newHistory);
                                        newHistory = new ShippmentHistoryModel()
                                        {
                                            CreatedAt = DateTime.Now,
                                            Message = $"Money will be refunded without PassingCar commision in 3-5 working days."
                                        };
                                        history.Add(newHistory);
                                        amountToRefund = taxDetails.RestPrice;
                                        JsonConvert.SerializeObject(new { amountToRefund, step = "After on else not update sold" }).AddToLog();
                                    }
                                    #region Refund
                                    query = @$"Select Top (1) p.* from Payment p
                                        inner Join Shipping s
                                        on s.OfferId = p.OfferId
                                        where s.Id =  @ShipId";
                                    Payment payment = await connection.QueryFirstOrDefaultAsync<Payment>(query,
                                        new
                                        {
                                            ShipId = input.ShippingId
                                        });
                                    if (payment != null)
                                    {
                                        PaymentDetails paymentDetails = new()
                                        {
                                            PaymentId = payment.Id,
                                            UserId = payment.UserId,
                                            OfferId = payment.OfferId,
                                            CreatedAt = payment.CreatedAt,
                                            ModifiedAt = payment.ModifiedAt,
                                        };
                                        if (!string.IsNullOrEmpty(payment.ChargeRequestJson))
                                        {
                                            paymentDetails.ChargeRequest = JsonConvert.DeserializeObject<ChargeData>(payment.ChargeRequestJson);
                                        }
                                        if (!string.IsNullOrEmpty(payment.ChargeResponseJson))
                                        {
                                            paymentDetails.ChargeResponse = JsonConvert.DeserializeObject<ChargeResponse>(payment.ChargeResponseJson);
                                        }
                                        if (!string.IsNullOrEmpty(payment.RefundRequestJson))
                                        {
                                            paymentDetails.RefundRequest = JsonConvert.DeserializeObject<RefundData>(payment.RefundRequestJson);
                                        }
                                        if (!string.IsNullOrEmpty(payment.RefundResponseJson))
                                        {
                                            paymentDetails.RefundResponse = JsonConvert.DeserializeObject<RefundResponse>(payment.RefundResponseJson);
                                        }
                                        if (!string.IsNullOrEmpty(payment.PaymentProofModelJson))
                                        {
                                            paymentDetails.PaymentProofModel = JsonConvert.DeserializeObject<PaymentProofModel>(payment.PaymentProofModelJson);
                                        }
                                        if (!string.IsNullOrEmpty(payment.PaymentProofFromTransporterModelJson))
                                        {
                                            paymentDetails.PaymentProofFromTransporterModel = JsonConvert.DeserializeObject<PaymentProofModel>(payment.PaymentProofFromTransporterModelJson);
                                        }
                                        if (paymentDetails.ChargeResponse != null)
                                        {
                                            StripeConfiguration.ApiKey = "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";
                                            RefundService refundService = new();
                                            RefundCreateOptions refundOptions = new()
                                            {
                                                Charge = paymentDetails.ChargeResponse.Id,
                                                Amount = Convert.ToInt64(amountToRefund)
                                            };
                                            //var serviceToken = new TokenService();
                                            //var newToken = serviceToken.Create(token);
                                            Refund refund = refundService.Create(refundOptions);
                                            query = @$"Select Top (1) p.Id from Payment p
                                                        inner Join Shipping s
                                                        on s.OfferId = p.OfferId
                                                        where s.Id =  @ShipId";
                                            int paymentId = await connection.QueryFirstOrDefaultAsync<int>(query,
                                                new
                                                {
                                                    ShipId = input.ShippingId
                                                });
                                            RefundData refundData = new()
                                            {
                                                Amount = refundOptions.Amount,
                                                Charge = refundOptions.Charge,
                                                Currency = refundOptions.Currency,
                                                Customer = refundOptions.Customer,
                                                InstructionsEmail = refundOptions.InstructionsEmail,
                                                Metadata = refundOptions.Metadata,
                                                Origin = refundOptions.Origin,
                                                PaymentIntent = refundOptions.PaymentIntent,
                                                Reason = refundOptions.Reason,
                                                RefundApplicationFee = refundOptions.RefundApplicationFee,
                                                ReverseTransfer = refundOptions.ReverseTransfer,
                                                ExtraParams = refundOptions.ExtraParams,
                                            };
                                            RefundResponse refundResponse = new()
                                            {
                                                Id = refund.Id,
                                                Object = refund.Object,
                                                Amount = refund.Amount,
                                                BalanceTransactionId = refund.BalanceTransactionId,
                                                ChargeId = refund.ChargeId,
                                                Created = refund.Created,
                                                Currency = refund.Currency,
                                                Description = refund.Description,
                                                FailureBalanceTransactionId = refund.FailureBalanceTransactionId,
                                                FailureReason = refund.FailureReason,
                                                InstructionsEmail = refund.InstructionsEmail,
                                                Metadata = refund.Metadata,
                                                PaymentIntentId = refund.PaymentIntentId,
                                                Reason = refund.Reason,
                                                ReceiptNumber = refund.ReceiptNumber,
                                                SourceTransferReversalId = refund.SourceTransferReversalId,
                                                Status = refund.Status
                                            };
                                            PaymentController paymentController = new(notificationService, hubService, loginService);
                                            try
                                            {
                                                _ = await paymentController.UpdateRefund(new UpdateRefundRequest()
                                                {
                                                    PaymentId = paymentId,
                                                    Refund = refundData
                                                });
                                                _ = await paymentController.UpdateRefundResponse(new UpdateRefundResponseRequest()
                                                {
                                                    PaymentId = paymentId,
                                                    Refund = refundResponse
                                                });
                                            }
                                            catch (Exception ex)
                                            {
                                                ex.CatchIt();
                                            }
                                        }
                                    }
                                    #endregion
                                }
                                catch (Exception ex)
                                {
                                    ex.CatchIt();
                                }
                                break;
                            default:
                                break;
                        }
                    }
                    query = $@"Update Shipping
                                            Set ModifiedAt = GETDATE(),
                                            State = @State, 
                                            History = @History
                                            where Id = @Id";
                    _ = await connection.QueryAsync(query, new { Id = input.ShippingId, input.State, History = JsonConvert.SerializeObject(history) });
                    response.History = history;
                    response.Success = true;
                    response.ErrorMessage = string.Empty;
                    try
                    {
                        await hubService.SendHubMessage("AdsHub", await HttpContext.GetTokenAsync("access_token") ?? string.Empty, "ShippingStateChanged", input);
                    }
                    catch (Exception ex)
                    {
                        ex.CatchIt();
                    }
                }
                catch (Exception ex)
                {
                    ex.CatchIt();
                    response.Success = false;
                    response.ErrorMessage = ex.Message;
                }
                finally
                {
                    connection.Close();
                }
            }
            else
            {
                response.Success = false;
                response.ErrorMessage = $"You must be logged in!";
            }
            return response;
        }
        [HttpPost(Name = "ResetSold")]
        public async Task<bool> ResetSold(ResetSoldInput input)
        {
            UserIdAndProfile loggedUser = await GetUserIdAndProfileAsync();
            using SqlConnection connection = AppConfiguration.GetConnection();
            connection.Open();
            try
            {
                string query = $@"Select Sold from [User]
									Where Id = @UserId";

                string userSold = await connection.QueryFirstOrDefaultAsync<string>(query, new { UserId = loggedUser.Id, UserProfile = loggedUser.Profile.ToString() });

                Dictionary<ProfileType, decimal>? profilesSolds = null;
                if (!string.IsNullOrEmpty(userSold))
                {
                    profilesSolds = JsonConvert.DeserializeObject<Dictionary<ProfileType, decimal>>(userSold);
                    if (profilesSolds?.ContainsKey(loggedUser.Profile) ?? false)
                    {
                        profilesSolds[loggedUser.Profile] = input.Sold;
                    }
                    else
                    {
                        if (profilesSolds is null)
                        {
                            profilesSolds = new();
                        }
                        profilesSolds.Add(loggedUser.Profile, input.Sold);
                    }
                }
                if (profilesSolds is null)
                {
                    profilesSolds = new();
                    profilesSolds.Add(loggedUser.Profile, input.Sold);
                }
                query = $@"UPDATE [User]  
                                                    SET Sold =  @Sold
                                                where Id = @Id";
                _ = await connection.QueryAsync(query, new { Id = loggedUser.Id, Sold = JsonConvert.SerializeObject(profilesSolds) });
                return true;
            }
            catch (Exception ex)
            {
                ex.CatchIt();
            }
            finally
            {
                connection.Close();
            }
            return false;
        }

        [HttpPost(Name = "GetNextAds")]
        public async Task<GetNextAdsResponse> GetNextAds(GetNextAdsInput input)
        {
            GetNextAdsResponse response = new();
            UserIdAndProfile loggedUser = await GetUserIdAndProfileAsync();
            System.Diagnostics.Debug.WriteLine($"[API] GetNextAds called by user {loggedUser.Id}, LastLoadedAds: {input?.LastAdsLoaded}");
            
            if (loggedUser.Id <= 0)
            {
                response.Success = false;
                response.ErrorMessage = "You must be logged in!";
                return response;
            }

            using SqlConnection connection = AppConfiguration.GetConnection();
            await connection.OpenAsync(); // Use async Open
            try
            {
                // Build filter string more efficiently
                var filterBuilder = new System.Text.StringBuilder();
                if (input.Filter != null)
                {
                    // Handle user filtering: either show only own ads or all users' ads
                    if (input.Filter.OnlyOwnAds)
                    {
                        filterBuilder.Append("AND a.UserId = @UserId AND a.UserProfile = @UserProfile ");
                    }
                    else if (!input.Filter.ShowAllUsers)
                    {
                        // If not showing all users and not only own ads, still filter by current user
                        filterBuilder.Append("AND a.UserId = @UserId AND a.UserProfile = @UserProfile ");
                    }
                    // If ShowAllUsers = true and OnlyOwnAds = false, no user filtering (show all users' ads)
                    
                    if (input.Filter.FromLastXDays > 0)
                    {
                        filterBuilder.Append($"AND a.CreatedAt > DATEADD(day,-{input.Filter.FromLastXDays},GETDATE()) ");
                    }
                    if (input.Filter.MinPrice.HasValue)
                    {
                        filterBuilder.Append($"AND a.Price >= {input.Filter.MinPrice.Value} ");
                    }
                    if (input.Filter.MaxPrice.HasValue)
                    {
                        filterBuilder.Append($"AND a.Price <= {input.Filter.MaxPrice.Value} ");
                    }
                    if (input.Filter.OnlyFavorites)
                    {
                        filterBuilder.Append("AND EXISTS(SELECT 1 FROM FavoriteAds WHERE AdId = a.Id AND UserId = @UserId AND UserProfile = @UserProfile) ");
                    }
                    if (input.Filter.Active)
                    {
                        filterBuilder.Append("AND a.State < 2 ");
                    }
                    if (input.Filter.UserType.HasValue)
                    {
                        if (input.Filter.UserType.Value == UserType.Individuals)
                        {
                            filterBuilder.Append("AND ISNULL(JSON_VALUE(u.JuridicDetails, '$.CompanyName'), '') = '' ");
                        }
                        else
                        {
                            filterBuilder.Append("AND ISNULL(JSON_VALUE(u.JuridicDetails, '$.CompanyName'), '') <> '' ");
                        }
                    }
                    if (!string.IsNullOrEmpty(input.Filter.ProvincieFrom))
                    {
                        filterBuilder.Append($"AND JSON_VALUE(a.[From], '$.Provincie') = '{input.Filter.ProvincieFrom}' ");
                    }
                    if (!string.IsNullOrEmpty(input.Filter.ProvincieTo))
                    {
                        filterBuilder.Append($"AND JSON_VALUE(a.[To], '$.Provincie') = '{input.Filter.ProvincieTo}' ");
                    }
                    if (input.Filter.OnlyNextOne)
                    {
                        filterBuilder.Append($"AND a.Id = {input.LastAdsLoaded + 1} ");
                    }
                }
                
                string moreFilter = filterBuilder.ToString();
                
                // Optimized query with better performance
                // FIXED: Always exclude ads with state >= 3 (AcceptedForTransit and above) - these are ads where payment has been completed
                string query = $@"SELECT TOP 50
                                    COALESCE(a.Photo1, a.Photo2, a.Photo3, a.Photo4) AS FirstAdsImage,
                                    a.Title as AdsTitle,
                                    CASE WHEN EXISTS(SELECT 1 FROM FavoriteAds WHERE AdId = a.Id AND UserId = @UserId AND UserProfile = @UserProfile) 
                                         THEN 1 ELSE 0 END as IsFavorite,
                                    a.Id as AdsId,
                                    a.State as State,
                                    a.[From] as AdsFrom,
                                    a.[To] as AdsTo,
                                    a.Price as AdsPrice,
                                    u.Photo as UserProfilePhoto,
                                    a.UserProfile,
                                    CASE WHEN (a.UserProfile IS NULL OR a.UserProfile = '0' OR a.UserProfile = 'Fisica') 
                                         THEN u.Name
                                         ELSE ISNULL(JSON_VALUE(u.JuridicDetails, '$.CompanyName'), '') END as UserName,
                                    u.Id as UserId,
                                    a.CreatedAt as PostedTime,
                                    a.ModifiedAt,
                                    ISNULL((SELECT AVG(CAST(Rating AS FLOAT)) FROM [dbo].[Review] 
                                           WHERE ReviewedUserId = u.Id AND ReviewedUserProfile = @UserProfile), -1) as UserRating
                                FROM Ads a WITH (NOLOCK)
                                LEFT JOIN [User] u WITH (NOLOCK) ON a.UserId = u.ID
                                WHERE a.Id > @LastLoadedAds AND a.State < 3 {moreFilter}
                                {GetOrderByClause(input.Filter?.Relevance)}";
                
                var parameters = new { 
                    UserId = loggedUser.Id, 
                    UserProfile = loggedUser.Profile.ToString(), 
                    LastLoadedAds = input.LastAdsLoaded 
                };
                
                var adsItems = (await connection.QueryAsync<AdFromAdsListModel>(query, parameters)).ToList();

                System.Diagnostics.Debug.WriteLine($"[API] GetNextAds query returned {adsItems.Count} items for user {loggedUser.Id}");

                response = new GetNextAdsResponse()
                {
                    Success = true,
                    ErrorMessage = string.Empty,
                    AdsItem = adsItems
                };
                return response;
            }
            catch (Exception ex)
            {
                ex.CatchIt();
                response.Success = false;
                response.ErrorMessage = ex.Message;
                return response;
            }
            finally
            {
                await connection.CloseAsync(); // Use async Close
            }
        }

        [HttpGet(Name = "GetMyAds")]
        public async Task<GetNextAdsResponse> GetMyAds()
        {
            GetNextAdsResponse response = new();
            UserIdAndProfile loggedUser = await GetUserIdAndProfileAsync();
            System.Diagnostics.Debug.WriteLine($"[API] GetMyAds called by user {loggedUser.Id}");
            
            if (loggedUser.Id <= 0)
            {
                response.Success = false;
                response.ErrorMessage = "You must be logged in!";
                return response;
            }

            using SqlConnection connection = AppConfiguration.GetConnection();
            await connection.OpenAsync();
            try
            {
                // Simple query to get only logged-in user's ads with photos
                string query = @"SELECT TOP 50
                                    COALESCE(a.Photo1, a.Photo2, a.Photo3, a.Photo4) AS FirstAdsImage,
                                    a.Title as AdsTitle,
                                    0 as IsFavorite,
                                    a.Id as AdsId,
                                    a.State as State,
                                    a.[From] as AdsFrom,
                                    a.[To] as AdsTo,
                                    a.Price as AdsPrice,
                                    u.Photo as UserProfilePhoto,
                                    a.UserProfile,
                                    u.Name as UserName,
                                    u.Id as UserId,
                                    a.CreatedAt as PostedTime,
                                    a.ModifiedAt,
                                    -1 as UserRating
                                FROM Ads a WITH (NOLOCK)
                                LEFT JOIN [User] u WITH (NOLOCK) ON a.UserId = u.ID
                                WHERE a.UserId = @UserId
                                ORDER BY a.CreatedAt DESC";
                
                var parameters = new { 
                    UserId = loggedUser.Id
                };
                
                var adsItems = (await connection.QueryAsync<AdFromAdsListModel>(query, parameters)).ToList();

                System.Diagnostics.Debug.WriteLine($"[API] GetMyAds query returned {adsItems.Count} items for user {loggedUser.Id}");

                response = new GetNextAdsResponse()
                {
                    Success = true,
                    ErrorMessage = string.Empty,
                    AdsItem = adsItems
                };
                return response;
            }
            catch (Exception ex)
            {
                ex.CatchIt();
                response.Success = false;
                response.ErrorMessage = ex.Message;
                return response;
            }
            finally
            {
                await connection.CloseAsync();
            }
        }

        [HttpGet(Name = "GetAllUsersAds")]
        public async Task<GetNextAdsResponse> GetAllUsersAds()
        {
            GetNextAdsResponse response = new();
            UserIdAndProfile loggedUser = await GetUserIdAndProfileAsync();
            System.Diagnostics.Debug.WriteLine($"[API] GetAllUsersAds called by user {loggedUser.Id}");
            
            if (loggedUser.Id <= 0)
            {
                response.Success = false;
                response.ErrorMessage = "You must be logged in!";
                return response;
            }

            using SqlConnection connection = AppConfiguration.GetConnection();
            await connection.OpenAsync();
            try
            {
        // Lightweight query to get all users' ads (similar to GetMyAds but for all users)
        // FIXED: Always exclude ads with state >= 3 (AcceptedForTransit and above) - these are ads where payment has been completed
        string query = @"SELECT TOP 50
                                    COALESCE(a.Photo1, a.Photo2, a.Photo3, a.Photo4) AS FirstAdsImage,
                                    a.Title as AdsTitle,
                                    0 as IsFavorite,
                                    a.Id as AdsId,
                                    a.State as State,
                                    a.[From] as AdsFrom,
                                    a.[To] as AdsTo,
                                    a.Price as AdsPrice,
                                    u.Photo as UserProfilePhoto,
                                    a.UserProfile,
                                    u.Name as UserName,
                                    u.Id as UserId,
                                    a.CreatedAt as PostedTime,
                                    a.ModifiedAt,
                                    -1 as UserRating
                                FROM Ads a WITH (NOLOCK)
                                LEFT JOIN [User] u WITH (NOLOCK) ON a.UserId = u.ID
                                WHERE a.State < 3
                                ORDER BY a.CreatedAt DESC";

        var parameters = new { 
                    UserId = loggedUser.Id,
                    UserProfile = loggedUser.Profile.ToString()
                };
                
                var adsItems = (await connection.QueryAsync<AdFromAdsListModel>(query, parameters)).ToList();

                System.Diagnostics.Debug.WriteLine($"[API] GetAllUsersAds query returned {adsItems.Count} items for user {loggedUser.Id}");

                response = new GetNextAdsResponse()
                {
                    Success = true,
                    ErrorMessage = string.Empty,
                    AdsItem = adsItems
                };
                return response;
            }
            catch (Exception ex)
            {
                ex.CatchIt();
                response.Success = false;
                response.ErrorMessage = ex.Message;
                return response;
            }
            finally
            {
                await connection.CloseAsync();
            }
        }

        [HttpGet(Name = "GetMyOffersAds")]
        public async Task<GetNextAdsResponse> GetMyOffersAds()
        {
            GetNextAdsResponse response = new();
            UserIdAndProfile loggedUser = await GetUserIdAndProfileAsync();
            System.Diagnostics.Debug.WriteLine($"[API] GetMyOffersAds called by user {loggedUser.Id}");
            
            if (loggedUser.Id <= 0)
            {
                response.Success = false;
                response.ErrorMessage = "You must be logged in!";
                return response;
            }

            using SqlConnection connection = AppConfiguration.GetConnection();
            await connection.OpenAsync();
            try
            {
                // Query to get ads where driver has made offers but payment is still pending
                string query = @"SELECT TOP 50
                                    COALESCE(a.Photo1, a.Photo2, a.Photo3, a.Photo4) AS FirstAdsImage,
                                    a.Title as AdsTitle,
                                    0 as IsFavorite,
                                    a.Id as AdsId,
                                    a.State as State,
                                    a.[From] as AdsFrom,
                                    a.[To] as AdsTo,
                                    a.Price as AdsPrice,
                                    u.Photo as UserProfilePhoto,
                                    a.UserProfile,
                                    CASE WHEN (a.UserProfile IS NULL OR a.UserProfile = '0' OR a.UserProfile = 'Fisica') 
                                         THEN u.Name
                                         ELSE ISNULL(JSON_VALUE(u.JuridicDetails, '$.CompanyName'), '') END as UserName,
                                    u.Id as UserId,
                                    a.CreatedAt as PostedTime,
                                    a.ModifiedAt,
                                    -1 as UserRating
                                FROM Ads a WITH (NOLOCK)
                                INNER JOIN Offer o WITH (NOLOCK) ON a.Id = o.AdId
                                LEFT JOIN [User] u WITH (NOLOCK) ON a.UserId = u.ID
                                WHERE o.UserId = @UserId 
                                  AND o.UserProfile = @UserProfile
                                  AND o.State IN (0, 1, 2)  -- Pending, Sent, Seen (not PaymentPending, Accepted, Rejected, Canceled)
                                ORDER BY o.CreatedAt DESC";

                var parameters = new { 
                    UserId = loggedUser.Id,
                    UserProfile = (int)loggedUser.Profile  // Convert enum to int for database comparison
                };
                
                var adsItems = (await connection.QueryAsync<AdFromAdsListModel>(query, parameters)).ToList();

                System.Diagnostics.Debug.WriteLine($"[API] GetMyOffersAds query returned {adsItems.Count} items for user {loggedUser.Id}");
                System.Diagnostics.Debug.WriteLine($"[API] GetMyOffersAds - User {loggedUser.Id} has offers on {adsItems.Count} ads");

                response = new GetNextAdsResponse()
                {
                    Success = true,
                    ErrorMessage = string.Empty,
                    AdsItem = adsItems
                };
                return response;
            }
            catch (Exception ex)
            {
                ex.CatchIt();
                System.Diagnostics.Debug.WriteLine($"[API] GetMyOffersAds ERROR: {ex.Message}");
                response.Success = false;
                response.ErrorMessage = ex.Message;
                return response;
            }
            finally
            {
                await connection.CloseAsync();
            }
        }

        [HttpGet(Name = "GetMyCompletedOffersAds")]
        public async Task<GetNextAdsResponse> GetMyCompletedOffersAds()
        {
            GetNextAdsResponse response = new();
            UserIdAndProfile loggedUser = await GetUserIdAndProfileAsync();
            System.Diagnostics.Debug.WriteLine($"[API] GetMyCompletedOffersAds called by user {loggedUser.Id}");
            
            if (loggedUser.Id <= 0)
            {
                response.Success = false;
                response.ErrorMessage = "You must be logged in!";
                return response;
            }

            using SqlConnection connection = AppConfiguration.GetConnection();
            await connection.OpenAsync();
            try
            {
                // Query to get ads where driver has made offers and payment is completed (state >= 3)
                string query = @"SELECT TOP 50
                                    COALESCE(a.Photo1, a.Photo2, a.Photo3, a.Photo4) AS FirstAdsImage,
                                    a.Title as AdsTitle,
                                    0 as IsFavorite,
                                    a.Id as AdsId,
                                    a.State as State,
                                    a.[From] as AdsFrom,
                                    a.[To] as AdsTo,
                                    a.Price as AdsPrice,
                                    u.Photo as UserProfilePhoto,
                                    a.UserProfile,
                                    CASE WHEN (a.UserProfile IS NULL OR a.UserProfile = '0' OR a.UserProfile = 'Fisica') 
                                         THEN u.Name
                                         ELSE ISNULL(JSON_VALUE(u.JuridicDetails, '$.CompanyName'), '') END as UserName,
                                    u.Id as UserId,
                                    a.CreatedAt as PostedTime,
                                    a.ModifiedAt,
                                    -1 as UserRating
                                FROM Ads a WITH (NOLOCK)
                                INNER JOIN Offer o WITH (NOLOCK) ON a.Id = o.AdId
                                LEFT JOIN [User] u WITH (NOLOCK) ON a.UserId = u.ID
                                WHERE o.UserId = @UserId 
                                  AND o.State >= 3  -- PaymentPending, Accepted, Rejected, Canceled (payment completed)
                                ORDER BY o.CreatedAt DESC";

                var parameters = new { 
                    UserId = loggedUser.Id,
                    //UserProfile = (int)loggedUser.Profile  // Convert enum to int for database comparison
                };
                
                var adsItems = (await connection.QueryAsync<AdFromAdsListModel>(query, parameters)).ToList();

                System.Diagnostics.Debug.WriteLine($"[API] GetMyCompletedOffersAds query returned {adsItems.Count} items for user {loggedUser.Id}");

                response = new GetNextAdsResponse()
                {
                    Success = true,
                    ErrorMessage = string.Empty,
                    AdsItem = adsItems
                };
                return response;
            }
            catch (Exception ex)
            {
                ex.CatchIt();
                response.Success = false;
                response.ErrorMessage = ex.Message;
                return response;
            }
            finally
            {
                await connection.CloseAsync();
            }
        }

        [HttpPost(Name = "CheckAdsUpdates")]
        public async Task<GetNextAdsResponse> CheckAdsUpdates(CheckAdsUpdatesInput input)
        {
            GetNextAdsResponse response = new();
            UserIdAndProfile loggedUser = await GetUserIdAndProfileAsync();
            if (loggedUser.Id > 0)
            {
                using SqlConnection connection = AppConfiguration.GetConnection();
                connection.Open();
                try
                {
                    if (input != null && input.AdsFromLocal != null && input.AdsFromLocal.Any())
                    {
                        List<AdFromAdsListModel> AdsItems = new();
                        foreach (AdsUpdateDetails item in input.AdsFromLocal)
                        {
                            string checkUpdate = @$"Select
                                    CASE WHEN ModifiedAt > @ModifiedAt THEN 1
                                    WHEN ModifiedAt is not NULL AND @ModifiedAt is NULL THEN 1
                                    ELSE 0 end as NeedUpdate
                                    from Ads
                                    where Id = @AdsId";
                            bool NeedUpdate = await connection.QueryFirstOrDefaultAsync<bool>(checkUpdate, new { AdsId = item.AdId, item.ModifiedAt });
                            if (NeedUpdate)
                            {

                                string query = $@"Select Top 1
                                    case
                                    When a.Photo1 is not null then a.Photo1  
                                    When a.Photo2 is not null then a.Photo2
                                    When a.Photo3 is not null then a.Photo3
                                    else a.Photo4
                                    end
                                    AS FirstAdsImage,
                                    a.Title as AdsTitle,
                                    0 as IsFavorite,
                                    a.Id as AdsId,
                                    a.State as State,
                                    a.[From] as AdsFrom,
                                    a.[To] as AdsTo,
                                    a.Price as AdsPrice,
                                    u.Photo as UserProfilePhoto,
                                    a.UserProfile,
                                    CASE WHEN (a.UserProfile is null OR a.UserProfile = '0' OR a.UserProfile = 'Fisica') THEN  u.Name
                                    ELSE ISNULL(JSON_VALUE(u.JuridicDetails, '$.CompanyName'), '') end as UserName,
                                    u.Id as UserId,
                                    a.CreatedAt as PostedTime,
                                    a.ModifiedAt,
                                    CASE WHEN (SELECT AVG(Rating + 0.0)   FROM [dbo].[Review]   where ReviewedUserId = u.Id  AND ReviewedUserProfile = a.UserProfile) is null THEN -1
                                    ELSE (SELECT AVG(Rating + 0.0)   FROM [dbo].[Review]   where ReviewedUserId = u.Id  AND ReviewedUserProfile = a.UserProfile) end as UserRating
                                    from Ads a
                                    left Join [User] u
                                    on a.UserId = u.ID
                                    where a.Id = @AdsId AND a.State < 3 ";
                                AdFromAdsListModel updatedAds = await connection.QueryFirstOrDefaultAsync<AdFromAdsListModel>(query, new { AdsId = item.AdId });
                                AdsItems.Add(updatedAds);
                            }
                        }
                        response = new GetNextAdsResponse()
                        {
                            Success = true,
                            ErrorMessage = string.Empty,
                            AdsItem = AdsItems
                        };
                    }
                    else
                    {
                        response = new GetNextAdsResponse()
                        {
                            Success = true,
                            ErrorMessage = string.Empty,
                            AdsItem = new List<AdFromAdsListModel>()
                        };
                    }
                }
                catch (Exception ex)
                {
                    ex.CatchIt();
                    response.Success = false;
                    response.ErrorMessage = ex.Message;
                }
                finally
                {
                    connection.Close();
                }
            }
            else
            {
                response.Success = false;
                response.ErrorMessage = $"You must be logged in!";
            }
            return response;
        }
        [HttpPost(Name = "GetNextShipping")]
        public async Task<GetNextShippingResponse> GetNextShipping(GetNextShippingInput input)
        {
            GetNextShippingResponse response = new();
            UserIdAndProfile loggedUser = await GetUserIdAndProfileAsync();
            if (loggedUser.Id > 0)
            {
                using SqlConnection connection = AppConfiguration.GetConnection();
                connection.Open();
                try
                {
                    string query = $@"Select Top 50 s.Id, s.CreatedAt, s.State, a.[From], a.[To], o.Id as OfferId, a.Id as AdsId, s.ConfirmationCode, s.History,
                                    CASE WHEN (a.UserId = @UserId) THEN 1
									ELSE 0
									END  as IsMyAds
                                    from Shipping s
                                    inner Join Offer o
                                    on o.Id = s.OfferId
                                    inner Join Ads a
                                    on a.Id = o.AdId
                                    where s.Id > @LastShippingLoaded
                                    and ((a.UserId = @UserId) or (o.UserId = @UserId))
                                    order by s.Id desc ";
                    IEnumerable<ShippingFromDBModel> ship = await connection.QueryAsync<ShippingFromDBModel>(query, new { UserId = loggedUser.Id, input.LastShippingLoaded }); ;
                    response = new GetNextShippingResponse()
                    {
                        Success = true,
                        ErrorMessage = string.Empty,
                        ShippingItems = new List<ShippingFromListModel>()
                    };
                    if (ship != null && ship.Count() > 0)
                    {
                        foreach (ShippingFromDBModel item in ship)
                        {
                            ((List<ShippingFromListModel>)response.ShippingItems).Add(new ShippingFromListModel(item));
                        }
                    }
                    return response;
                }
                catch (Exception ex)
                {
                    ex.CatchIt();
                    response.Success = false;
                    response.ErrorMessage = ex.Message;
                }
                finally
                {
                    connection.Close();
                }
            }
            else
            {
                response.Success = false;
                response.ErrorMessage = $"You must be logged in!";
            }
            return response;
        }
        [HttpPost(Name = "GetOtherUserName")]
        public async Task<GetOtherUserNameResult> GetOtherUserName(GetOtherUserNameInput input)
        {
            GetOtherUserNameResult response = new();
            UserIdAndProfile loggedUser = await GetUserIdAndProfileAsync();
            if (loggedUser.Id > 0)
            {
                using SqlConnection connection = AppConfiguration.GetConnection();
                connection.Open();
                try
                {
                    string query = $@"Select Top 1 
									CASE WHEN up.Id = @UserId AND (o.UserProfile is null OR o.UserProfile = '0' OR o.UserProfile = 'Fisica') THEN  CONCAT(u.Name,' ',u.Surname)
                                    WHEN up.Id = 1 THEN  ISNULL(JSON_VALUE(u.JuridicDetails, '$.CompanyName'), '')
									WHEN (a.UserProfile is null OR a.UserProfile = '0' OR a.UserProfile = 'Fisica') THEN  CONCAT(up.Name,' ',up.Surname)
                                    ELSE ISNULL(JSON_VALUE(up.JuridicDetails, '$.CompanyName'), '') end as name
                                    from Shipping s
                                    inner Join Offer o
                                    on o.Id = s.OfferId
									inner join [User] u
									on o.UserId = u.Id
									inner join Ads a
									on o.AdId = a.Id
									inner join [User] up
									on a.UserId = up.Id
                                    where s.Id = @Id ";
                    string name = await connection.QueryFirstOrDefaultAsync<string>(query, new { Id = input.ShippingId, UserId = loggedUser.Id });
                    response = new GetOtherUserNameResult()
                    {
                        Success = true,
                        ErrorMessage = string.Empty,
                        Name = name
                    };
                    return response;
                }
                catch (Exception ex)
                {
                    ex.CatchIt();
                    response.Success = false;
                    response.ErrorMessage = ex.Message;
                }
                finally
                {
                    connection.Close();
                }
            }
            else
            {
                response.Success = false;
                response.ErrorMessage = $"You must be logged in!";
            }
            return response;
        }
        [HttpPost(Name = "GetAdsDetails")]
        public async Task<GetAdsDetailsResponse> GetAdsDetails(GetAdsDetailsInput input)
        {
            try
            {
                GetAdsDetailsResponse response = new();
                UserIdAndProfile loggedUser = await GetUserIdAndProfileAsync();
                if (loggedUser.Id > 0)
                {
                    using SqlConnection connection = AppConfiguration.GetConnection();
                    connection.Open();
                    try
                    {
                        string query = $@"Select a.State as State, a.Description as Description, a.ViewsCounter as ViewCounter, u.JuridicDetails as JuridicDetails,
                                   CASE WHEN (a.UserId = @UserId AND a.UserProfile =  @UserProfile) THEN 1
									ELSE 0
									END  as IsMyAds
                                    from Ads a
                                    left join [User] u
                                    on a.UserId = u.ID
                                    where a.Id = @AdsId";

                        response.AdsMoreDetails = await connection.QueryFirstOrDefaultAsync<AdsMoreDetails>(query, new { input.AdsId, UserId = loggedUser.Id, UserProfile = loggedUser.Profile.ToString() });

                        if (response.AdsMoreDetails != null)
                        {
                            query = $"Select Photo1, Photo2, Photo3, Photo4 from Ads where Id = @AdsId";
                            AdsPhotosModel photos = await connection.QueryFirstOrDefaultAsync<AdsPhotosModel>(query, new { input.AdsId });

                            if (photos != null)
                            {
                                response.AdsMoreDetails.NextPhotos = new List<byte[]>();
                                bool firstPhotoSkiped = false;
                                if (photos.Photo1 != null && photos.Photo1.Length > 0)
                                {
                                    firstPhotoSkiped = true;
                                }
                                if (photos.Photo2 != null && photos.Photo2.Length > 0)
                                {
                                    if (firstPhotoSkiped)
                                    {
                                        response.AdsMoreDetails.NextPhotos.Add(photos.Photo2);
                                    }
                                    else
                                    {
                                        firstPhotoSkiped = true;
                                    }
                                }
                                if (photos.Photo3 != null && photos.Photo3.Length > 0)
                                {
                                    if (firstPhotoSkiped)
                                    {
                                        response.AdsMoreDetails.NextPhotos.Add(photos.Photo3);
                                    }
                                    else
                                    {
                                        firstPhotoSkiped = true;
                                    }
                                }
                                if (photos.Photo4 != null && photos.Photo4.Length > 0)
                                {
                                    if (firstPhotoSkiped)
                                    {
                                        response.AdsMoreDetails.NextPhotos.Add(photos.Photo4);
                                    }
                                    else
                                    {
                                        firstPhotoSkiped = true;
                                    }
                                }
                            }
                            if (response.AdsMoreDetails.IsMyAds)
                            {
                                try
                                {
                                    query = $@"Select Distinct
                                    CASE WHEN (o.UserProfile is null OR o.UserProfile = 0 OR o.UserProfile = 1) THEN  CONCAT(u.Name,' ',u.Surname)
                                    ELSE ISNULL(JSON_VALUE(u.JuridicDetails, '$.CompanyName'), '') end as UserName,
                                    u.Photo as UserProfilePhoto,
                                    CASE WHEN (SELECT AVG(Rating + 0.0)   FROM [dbo].[Review]   where ReviewedUserId = u.Id AND ReviewedUserProfile = o.UserProfile) is null THEN -1
                                    ELSE (SELECT AVG(Rating + 0.0)   FROM [dbo].[Review]   where ReviewedUserId = u.Id AND ReviewedUserProfile = o.UserProfile) end as UserRating
									,u.Id  as UserId,
                                    o.UserProfile as UserProfiledString,
									CASE WHEN ISNULL(u.JuridicDetails,'') = '' THEN 'Private'
                                    ELSE 'Juridic' end as UserType,
									c.Id as ChatId
									from [User] u
									inner join  Offer o
									on o.UserId = u.Id
									left join Chat c
									on c.UberUserId = u.Id and c.UberUserProfile = o.UserProfile and c.AdId = @AdsId
									where o.AdId = @AdsId";

                                    response.AdsMoreDetails.OffersGroups = (await connection.QueryAsync<GroupOfOffers>(query, new { input.AdsId })).ToList();

                                    if (response.AdsMoreDetails.OffersGroups != null && response.AdsMoreDetails.OffersGroups.Count > 0)
                                    {
                                        foreach (GroupOfOffers group in response.AdsMoreDetails.OffersGroups)
                                        {
                                            int value;
                                            group.UserProfile = int.TryParse(group.UserProfiledString, out value) ? (ProfileType)value : (ProfileType)Enum.Parse(typeof(ProfileType), group.UserProfiledString ?? "Fisica");
                                            int indexOfGroup = response.AdsMoreDetails.OffersGroups.IndexOf(group);
                                            long chatid = 0;
                                            if (!group.ChatId.HasValue || (group.ChatId.HasValue && group.ChatId.Value == 0))
                                            {

                                                Chat chat = new()
                                                {
                                                    AdId = input.AdsId,
                                                    CustomerUserId = loggedUser.Id,
                                                    CustomerUserProfile = loggedUser.Profile,
                                                    UberUserId = group.UserId,
                                                    UberUserProfile = group.UserProfile,
                                                    State = ChateState.Censored,
                                                    CreatedAt = DateTime.Now,
                                                };
                                                chatid = connection.Insert(chat);
                                                response.AdsMoreDetails.OffersGroups[indexOfGroup].ChatId = (int)chatid;
                                            }
                                            query = @$"Select  o.Id, o.Message as Price, o.State, o.CreatedAt
                                            from Offer o
                                            left
                                            join [User] u
                                            on o.UserId = u.Id
                                            where AdId = @AdsId AND u.Id = @UserId   AND o.UserProfile = @UserProfile";

                                            response.AdsMoreDetails.OffersGroups[indexOfGroup].Offers = (await connection.QueryAsync<OfferDetails>(query, new { input.AdsId, group.UserId, UserProfile = group.UserProfile.ToString() })).ToList();

                                            if (response.AdsMoreDetails.OffersGroups[indexOfGroup].Offers != null && response.AdsMoreDetails.OffersGroups[indexOfGroup].Offers.Count > 0)
                                            {
                                                IEnumerable<OfferDetails> acceptedOffers = response.AdsMoreDetails.OffersGroups[indexOfGroup].Offers.Where(o => o.State is OfferState.Accepted or OfferState.PaymentPending);
                                                if (acceptedOffers != null && acceptedOffers.Any())
                                                {
                                                    response.AdsMoreDetails.OffersGroups = new List<GroupOfOffers>()
                                                        {
                                                           group
                                                        };
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    ex.CatchIt();
                                    response.AdsMoreDetails.OffersGroups = new List<GroupOfOffers>();
                                }
                            }
                            else
                            {
                                query = @$"Select  o.Id, o.Message as Price, o.State, o.CreatedAt
                                            from Offer o
                                            where o.AdId = @AdsId AND o.UserId = @UserId AND o.UserProfile = @UserProfile
											order by o.CreatedAt";
                                try
                                {
                                    response.AdsMoreDetails.SentOffers = (await connection.QueryAsync<OfferDetails>(query, new { UserId = loggedUser.Id, UserProfile = loggedUser.Profile.ToString(), input.AdsId })).ToList();
                                    query = $@"SELECT TOP (1) [Id]
                                              FROM [dbo].[Chat]
                                              where AdId = @AdId and UberUserId = @UserId AND UberUserProfile = @UserProfile";
                                    response.AdsMoreDetails.ChatIdForViewer = await connection.QueryFirstOrDefaultAsync<int>(query, new { AdId = input.AdsId, UserProfile = loggedUser.Profile.ToString(), UserId = loggedUser.Id });


                                    long chatid = 0;
                                    if (!response.AdsMoreDetails.ChatIdForViewer.HasValue || (response.AdsMoreDetails.ChatIdForViewer.HasValue && response.AdsMoreDetails.ChatIdForViewer.Value == 0))
                                    {
                                        Chat chat = new()
                                        {
                                            AdId = input.AdsId,
                                            CustomerUserId = connection.QueryFirstOrDefault<int>($"Select UserId from Ads where Id = @AdsId", new { input.AdsId }),
                                            UberUserId = loggedUser.Id,
                                            State = ChateState.Censored,
                                            CreatedAt = DateTime.Now,
                                        };
                                        chatid = connection.Insert(chat);
                                        response.AdsMoreDetails.ChatIdForViewer = (int)chatid;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    ex.CatchIt();
                                    response.AdsMoreDetails.SentOffers = new List<OfferDetails>();
                                }
                            }
                            response.Success = true;
                            response.ErrorMessage = string.Empty;
                        }
                        else
                        {
                            response = new GetAdsDetailsResponse()
                            {
                                Success = false,
                                ErrorMessage = $"Invalid first query response"
                            };
                        }
                        return response;
                    }
                    catch (Exception ex)
                    {
                        ex.CatchIt();
                        response.Success = false;
                        response.ErrorMessage = ex.Message;
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
                else
                {
                    response.Success = false;
                    response.ErrorMessage = $"You must be logged in!";
                }
                return response;
            }
            catch (Exception ex)
            {
                ex.CatchIt();
                GetAdsDetailsResponse response = new();
                response.Success = false;
                response.ErrorMessage = ex.Message;
                return response;
            }
        }
        [HttpPost(Name = "GetMyOffers")]
        public async Task<GetOffersResponse> GetMyOffers()
        {
            GetOffersResponse response = new()
            {
                Ads = new List<AdsParent>()
            };
            UserIdAndProfile loggedUser = await GetUserIdAndProfileAsync();
            if (loggedUser.Id > 0)
            {
                using SqlConnection connection = AppConfiguration.GetConnection();
                connection.Open();
                try
                {
                    string query = $@"  Select 
                                    a.Id as AdsId,
                                    NULL AS FirstPhoto,
									a.Title, a.CreatedAt from Ads a
									Where Id in (Select AdId from Offer)
									AND UserId = @UserId";


                    IEnumerable<AdsParent> adsWithOffers = await connection.QueryAsync<AdsParent>(query, new {UserId = loggedUser.Id });
                    if (adsWithOffers != null && adsWithOffers.Count() > 0)
                    {
                        foreach (AdsParent ad in adsWithOffers)
                        {
                            try
                            {
                                query = $@"Select Distinct
                                    CASE WHEN (o.UserProfile is null OR o.UserProfile = 0 OR o.UserProfile = 1) THEN  CONCAT(u.Name,' ',u.Surname)
                                    ELSE ISNULL(JSON_VALUE(u.JuridicDetails, '$.CompanyName'), '') end as UserName,
                                   NULL as UserProfilePhoto,
                                    CASE WHEN (SELECT AVG(Rating + 0.0)   FROM [dbo].[Review]   where ReviewedUserId = u.Id AND ReviewedUserProfile = o.UserProfile) is null THEN -1
                                    ELSE (SELECT AVG(Rating + 0.0)   FROM [dbo].[Review]   where ReviewedUserId = u.Id AND ReviewedUserProfile = o.UserProfile) end as UserRating
									,u.Id  as UserId,
									CASE WHEN ISNULL(u.JuridicDetails,'') = '' THEN 'Private'
                                    ELSE 'Juridic' end as UserType,
									c.Id as ChatId
									from [User] u
									inner join  Offer o
									on o.UserId = u.Id
									left join Chat c
									on c.UberUserId = u.Id and c.AdId = @AdsId
									where o.AdId = @AdsId";

                                ad.OffersGroups = (await connection.QueryAsync<GroupOfOffers>(query, new { ad.AdsId })).ToList();
                                if (ad.OffersGroups != null && ad.OffersGroups.Count > 0)
                                {
                                    foreach (GroupOfOffers group in ad.OffersGroups)
                                    {
                                        int indexOfGroup = ad.OffersGroups.IndexOf(group);
                                        query = @$"Select  o.Id, o.Message as Price, o.State, o.CreatedAt, o.UserProfile
                                            from Offer o
                                            left
                                            join [User] u
                                            on o.UserId = u.Id
                                            where AdId = @AdsId AND u.Id = @UserId";
                                        ad.OffersGroups[indexOfGroup].Offers = (await connection.QueryAsync<OfferDetails>(query, new { ad.AdsId, group.UserId })).ToList();
                                        if (ad.OffersGroups[indexOfGroup].Offers != null && ad.OffersGroups[indexOfGroup].Offers.Count > 0)
                                        {
                                            IEnumerable<OfferDetails> acceptedOffers = ad.OffersGroups[indexOfGroup].Offers.Where(o => o.State is OfferState.Accepted or OfferState.PaymentPending);
                                            if (acceptedOffers != null && acceptedOffers.Any())
                                            {
                                                ad.OffersGroups = new List<GroupOfOffers>()
                                                        {
                                                           group
                                                        };
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                ex.CatchIt();
                                ad.OffersGroups = new List<GroupOfOffers>();
                            }
                            response.Ads.Add(ad);
                        }

                    }


                    query = $@"  Select 
                                    a.Id as AdsId,
                                    NULL  AS FirstPhoto,
									(SELECT TOP (1) [Id]
										  FROM [dbo].[Chat]
										  where AdId = a.Id and UberUserId = @UserId and UberUserProfile = @UserProfile) as ChatIdForViewer,
									a.Title, a.CreatedAt from Ads a
									Where Id in (Select AdId from Offer Where UserId =  @UserId and UserProfile = @UserProfile)";


                    IEnumerable<AdsParent> adsSentOffers = await connection.QueryAsync<AdsParent>(query, new { UserId = loggedUser.Id, UserProfile = loggedUser.Profile.ToString() });
                    if (adsSentOffers != null && adsSentOffers.Count() > 0)
                    {
                        query = @$"Select  o.Id, o.Message as Price, o.State, o.CreatedAt
                                            from Offer o
                                            left
                                            join [User] u
                                            on o.UserId = u.Id
                                            where AdId = @AdsId AND u.Id = @UserId AND o.UserProfile = @UserProfile";
                        foreach (AdsParent item in adsSentOffers)
                        {
                            item.SentOffers = (await connection.QueryAsync<OfferDetails>(query, new { item.AdsId, UserId = loggedUser.Id, UserProfile = loggedUser.Profile.ToString() })).ToList();
                            response.Ads.Add(item);
                        }
                    }
                    response.Success = true;
                    return response;
                }
                catch (Exception ex)
                {
                    ex.CatchIt();
                    response.Success = false;
                    response.ErrorMessage = ex.Message;
                }
                finally
                {
                    connection.Close();
                }
            }
            else
            {
                response.Success = false;
                response.ErrorMessage = $"You must be logged in!";
            }
            if (response.Success)
            {
                try
                {
                    response.Ads = response.Ads.OrderByDescending(a => a.OffersGroups.Max(o => o.Offers.Select(os => os.CreatedAt))).ToList();
                }
                catch (Exception ex)
                {
                    ex.CatchIt();
                }
            }
            return response;
        }
        [HttpPost(Name = "GetMyNotifications")]
        public async Task<GetNotificationsResponse> GetMyNotifications()
        {
            GetNotificationsResponse response = new()
            {
                Notifications = new List<Notification>()
            };
            UserIdAndProfile loggedUser = await GetUserIdAndProfileAsync();
            if (loggedUser.Id > 0)
            {
                using SqlConnection connection = AppConfiguration.GetConnection();
                connection.Open();
                try
                {
                    string query = $@"Select * from [Notification]
									Where UserId = @UserId AND UserProfile = @UserProfile";

                    response.Notifications = await connection.QueryAsync<Notification>(query, new { UserId = loggedUser.Id, UserProfile = loggedUser.Profile.ToString() });

                    response.Success = true;
                    return response;
                }
                catch (Exception ex)
                {
                    ex.CatchIt();
                    response.Success = false;
                    response.ErrorMessage = ex.Message;
                }
                finally
                {
                    connection.Close();
                }
            }
            else
            {
                response.Success = false;
                response.ErrorMessage = $"You must be logged in!";
            }
            return response;
        }
        [HttpPost(Name = "GetUserRatingAndSold")]
        [ResponseCache(Duration = 300)] // Cache for 5 minutes
        public async Task<GetUserRatingAndSoldResponse> GetUserRatingAndSold()
        {
            GetUserRatingAndSoldResponse response = new();
            UserIdAndProfile loggedUser = await GetUserIdAndProfileAsync();
            if (loggedUser.Id > 0)
            {
                using SqlConnection connection = AppConfiguration.GetConnection();
                connection.Open();
                try
                {
                    string query = $@"Select Top 1 CASE WHEN (SELECT AVG(Rating + 0.0)   FROM [dbo].[Review]   where ReviewedUserId = @UserId  AND ReviewedUserProfile = @UserProfile) is null THEN -1
                                    ELSE (SELECT AVG(Rating + 0.0)   FROM [dbo].[Review]   where ReviewedUserId = @UserId AND ReviewedUserProfile = @UserProfile) end as UserRating
                                    from [User]";

                    response.Rating = await connection.QueryFirstOrDefaultAsync<double>(query, new { UserId = loggedUser.Id, UserProfile = loggedUser.Profile.ToString() });

                    query = $@"Select Sold from [User]
									Where Id = @UserId";

                    string userSold = await connection.QueryFirstOrDefaultAsync<string>(query, new { UserId = loggedUser.Id, UserProfile = loggedUser.Profile.ToString() });
                    response.Sold = 0;
                    if (!string.IsNullOrEmpty(userSold))
                    {
                        var profileSolds = JsonConvert.DeserializeObject<Dictionary<ProfileType, decimal>>(userSold);
                        if (profileSolds?.ContainsKey(loggedUser.Profile) ?? false)
                        {
                            response.Sold = profileSolds[loggedUser.Profile];
                        }
                    }

                    response.Success = true;
                    return response;
                }
                catch (Exception ex)
                {
                    ex.CatchIt();
                    response.Success = false;
                    response.ErrorMessage = ex.Message;
                }
                finally
                {
                    connection.Close();
                }
            }
            else
            {
                response.Success = false;
                response.ErrorMessage = $"You must be logged in!";
            }
            return response;
        }
        [HttpPost(Name = "GetReviews")]
        public async Task<GetReviewsResponse> GetReviews(GetReviewsInput input)
        {
            GetReviewsResponse response = new();
            UserIdAndProfile loggedUser = await GetUserIdAndProfileAsync();
            if (loggedUser.Id > 0)
            {
                if (input != null && input.UserId.HasValue && input.UserId.Value > 0)
                {
                    loggedUser.Id = input.UserId.Value;
                }
                using SqlConnection connection = AppConfiguration.GetConnection();
                connection.Open();
                try
                {
                    string query = $@"SELECT r.Rating, r.Message,CONCAT(u.Name,' ',u.Surname) as Name, NULL as ProfilePhoto  FROM [dbo].[Review]  r
                                inner join [User] u
                                on u.Id = r.ReviewerUserId
                                where ReviewedUserId = @UserId  AND ReviewedUserProfile = @UserProfile";

                    response.Reviews = await connection.QueryAsync<ReviewM>(query, new { UserId = loggedUser.Id, UserProfile = loggedUser.Profile.ToString() });
                    response.Success = true;
                    return response;
                }
                catch (Exception ex)
                {
                    ex.CatchIt();
                    response.Success = false;
                    response.ErrorMessage = ex.Message;
                }
                finally
                {
                    connection.Close();
                }
            }
            else
            {
                response.Success = false;
                response.ErrorMessage = $"You must be logged in!";
            }
            return response;
        }
        [HttpPost(Name = "GetUserPhoto")]
        public async Task<GetPhotoResponse> GetUserPhoto(GetPhotoRequest input)
        {
            GetPhotoResponse response = new();
            UserIdAndProfile loggedUser = await GetUserIdAndProfileAsync();
            if (loggedUser.Id > 0)
            {
                using SqlConnection connection = AppConfiguration.GetConnection();
                connection.Open();
                try
                {
                    string query = $@"SELECT Photo as Photo  FROM [dbo].[User] 
                                where Id = @UserId";

                    response.Photo = await connection.QueryFirstOrDefaultAsync<byte[]>(query, new { UserId = input.Id });
                    response.Success = true;
                    return response;
                }
                catch (Exception ex)
                {
                    ex.CatchIt();
                    response.Success = false;
                    response.ErrorMessage = ex.Message;
                }
                finally
                {
                    connection.Close();
                }
            }
            else
            {
                response.Success = false;
                response.ErrorMessage = $"You must be logged in!";
            }
            return response;
        }
        [HttpPost(Name = "GetAdsPhoto")]
        public async Task<GetPhotoResponse> GetAdsPhoto(GetPhotoRequest input)
        {
            GetPhotoResponse response = new();
            UserIdAndProfile loggedUser = await GetUserIdAndProfileAsync();
            if (loggedUser.Id > 0)
            {
                using SqlConnection connection = AppConfiguration.GetConnection();
                connection.Open();
                try
                {
                    string query = $@"Select 
                                    case
                                    When a.Photo1 is not null then a.Photo1  
                                    When a.Photo2 is not null then a.Photo2
                                    When a.Photo3 is not null then a.Photo3
                                    else a.Photo4
                                    end
                                    AS Photo
									from Ads a
									Where Id = @AdsId";

                    response.Photo = await connection.QueryFirstOrDefaultAsync<byte[]>(query, new { AdsId = input.Id });
                    response.Success = true;
                    return response;
                }
                catch (Exception ex)
                {
                    ex.CatchIt();
                    response.Success = false;
                    response.ErrorMessage = ex.Message;
                }
                finally
                {
                    connection.Close();
                }
            }
            else
            {
                response.Success = false;
                response.ErrorMessage = $"You must be logged in!";
            }
            return response;
        }
        [HttpPost(Name = "RemoveNotification")]
        public async Task<ApiBaseResponse> RemoveNotification(DeleteNotificationInput input)
        {
            ApiBaseResponse response = new();
            UserIdAndProfile loggedUser = await GetUserIdAndProfileAsync();
            if (loggedUser.Id > 0)
            {
                using SqlConnection connection = AppConfiguration.GetConnection();
                connection.Open();
                try
                {
                    _ = connection.Query("Delete from [Notification] where Id = @NotificationId AND UserId = @UserId", new
                    {
                        input.NotificationId,
                        UserId = loggedUser.Id,
                        UserProfile = loggedUser.Profile.ToString(),
                    });
                    response.Success = true;
                    response.ErrorMessage = string.Empty;
                }
                catch (Exception ex)
                {
                    ex.CatchIt();
                    response.Success = false;
                    response.ErrorMessage = ex.Message;
                }
                finally
                {
                    connection.Close();
                }
            }
            else
            {
                response.Success = false;
                response.ErrorMessage = $"You must be logged in!";
            }
            return response;
        }
        [HttpPost(Name = "AddToFavorite")]
        public async Task<ApiBaseResponse> AddToFavorite(AddToFavoriteInput input)
        {
            ApiBaseResponse response = new();
            UserIdAndProfile loggedUser = await GetUserIdAndProfileAsync();
            if (loggedUser.Id > 0)
            {
                using SqlConnection connection = AppConfiguration.GetConnection();
                connection.Open();
                try
                {
                    FavoriteAds favAd = new()
                    {
                        AdId = input.AdsId,
                        UserId = loggedUser.Id,
                        UserProfile = loggedUser.Profile,
                        CreatedAt = DateTime.Now
                    };
                    long id = connection.Insert(favAd);
                    if (id > 0)
                    {
                        response.Success = true;
                        response.ErrorMessage = string.Empty;
                    }
                    else
                    {
                        response.Success = false;
                        response.ErrorMessage = $"Invalid returned ID";
                    }
                }
                catch (Exception ex)
                {
                    ex.CatchIt();
                    response.Success = false;
                    response.ErrorMessage = ex.Message;
                }
                finally
                {
                    connection.Close();
                }
            }
            else
            {
                response.Success = false;
                response.ErrorMessage = $"You must be logged in!";
            }
            return response;
        }
        [HttpPost(Name = "RemoveFromFavorite")]
        public async Task<ApiBaseResponse> RemoveFromFavorite(AddToFavoriteInput input)
        {
            ApiBaseResponse response = new();
            UserIdAndProfile loggedUser = await GetUserIdAndProfileAsync();
            if (loggedUser.Id > 0)
            {
                using SqlConnection connection = AppConfiguration.GetConnection();
                connection.Open();
                try
                {
                    _ = connection.Query("Delete from [FavoriteAds] where AdId = @AdId AND UserId = @UserId AND UserProfile = @UserProfile", new
                    {
                        AdId = input.AdsId,
                        UserId = loggedUser.Id,
                        UserProfile = loggedUser.Profile
                    });
                    response.Success = true;
                    response.ErrorMessage = string.Empty;
                }
                catch (Exception ex)
                {
                    ex.CatchIt();
                    response.Success = false;
                    response.ErrorMessage = ex.Message;
                }
                finally
                {
                    connection.Close();
                }
            }
            else
            {
                response.Success = false;
                response.ErrorMessage = $"You must be logged in!";
            }
            return response;
        }
        [HttpPost(Name = "IncrementViewCounter")]
        public async Task<ApiBaseResponse> IncrementViewCounter(IncrementViewCounterInput input)
        {
            ApiBaseResponse response = new();
            using SqlConnection connection = AppConfiguration.GetConnection();
            connection.Open();
            try
            {
                _ = await connection.QueryAsync("Update [Ads] set ViewsCounter = ViewsCounter + 1 where Id =  @AdId ", new
                {
                    AdId = input.AdsId,
                });
                response.Success = true;
                response.ErrorMessage = string.Empty;
            }
            catch (Exception ex)
            {
                ex.CatchIt();
                response.Success = false;
                response.ErrorMessage = ex.Message;
            }
            finally
            {
                connection.Close();
            }

            return response;
        }
        [HttpPost(Name = "GetMonthlyStatus")]
        public async Task<MonthlyStatus> GetMonthlyStatus(MonthlyStatusInput input)
        {
            MonthlyStatus response = input.DateTime > DateTime.Now ? new() :
                new();
            UserIdAndProfile loggedUser = await GetUserIdAndProfileAsync();
            if (loggedUser.Id > 0)
            {
                try
                {
                    using SqlConnection connection = AppConfiguration.GetConnection();
                    response.ErrorMessage = string.Empty;
                    connection.Open();
                    try
                    {
                        string query = $@"Select Count(Id) from [Ads] as PublishedAds
                               where MONTH(CreatedAt) = MONTH(@Date)
                               and YEAR(CreatedAt) = YEAR(@Date)
	                           and UserId = @UserId AND UserProfile = @UserProfile";
                        response.PublishedAds = await connection.QueryFirstOrDefaultAsync<int>(query, new { Date = input.DateTime, UserId = loggedUser.Id, UserProfile = loggedUser.Profile.ToString() });
                    }
                    catch (Exception ex)
                    {
                        ex.CatchIt();
                        response.ErrorMessage += $"PublishedAds: {ex.Message}";
                    }
                    try
                    {
                        string query = $@"Select Count(Id) from [Ads] as PublishedAds
                               where MONTH(CreatedAt) = MONTH(@Date)
                               and YEAR(CreatedAt) = YEAR(@Date)
	                           and UserId = @UserId AND UserProfile = @UserProfile
	                           and State = @DeliveredState";
                        response.SentPackages = await connection.QueryFirstOrDefaultAsync<int>(query, new { Date = input.DateTime, UserId = loggedUser.Id, UserProfile = loggedUser.Profile.ToString(), DeliveredState = AdsState.Delivered });
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
                               where MONTH(s.CreatedAt) = MONTH(@Date)
                               and YEAR(s.CreatedAt) = YEAR(@Date)
	                           and o.UserId = @UserId AND o.UserProfile = @UserProfile
	                            and ( s.State = @DeliveredState  OR s.State = @FinalizedState)";
                        response.TransportedPackages = await connection.QueryFirstOrDefaultAsync<int>(query,
                            new
                            {
                                Date = input.DateTime,
                                UserId = loggedUser.Id,
                                DeliveredState = ShippingState.Delivered,
                                FinalizedState = ShippingState.Finalized,
                                UserProfile = loggedUser.Profile.ToString()
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
                               where MONTH(s.CreatedAt) = MONTH(@Date)
                               and YEAR(s.CreatedAt) = YEAR(@Date)
	                           and ((o.UserId = @UserId AND o.UserProfile = @UserProfile) OR (a.UserId = @UserId AND a.UserProfile = @UserProfile))
	                           and s.State = @CanceledState";
                        response.CanceledDeliveries = await connection.QueryFirstOrDefaultAsync<int>(query,
                            new
                            {
                                Date = input.DateTime,
                                UserId = loggedUser.Id,
                                CanceledState = ShippingState.Canceled,
                                UserProfile = loggedUser.Profile.ToString()
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
                               WHERE MONTH(CreatedAt) = MONTH(@Date)
                               AND YEAR(CreatedAt) = YEAR(@Date)
	                            AND UserId = @UserId AND UserProfile = @UserProfile";
                        response.OffersSent = await connection.QueryFirstOrDefaultAsync<int>(query,
                            new
                            {
                                Date = input.DateTime,
                                UserId = loggedUser.Id,
                                UserProfile = loggedUser.Profile.ToString()
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
                               where MONTH(s.CreatedAt) = MONTH(@Date)
                               and YEAR(s.CreatedAt) = YEAR(@Date)
	                           and o.UserId = @UserId  AND o.UserProfile = @UserProfile
	                            and ( s.State = @DeliveredState  OR s.State = @FinalizedState)";
                        response.SpentMoney = await connection.QueryFirstOrDefaultAsync<decimal?>(query,
                            new
                            {
                                Date = input.DateTime,
                                UserId = loggedUser.Id,
                                DeliveredState = ShippingState.Delivered,
                                FinalizedState = ShippingState.Finalized,
                                UserProfile = loggedUser.Profile.ToString()
                            }) ?? 0;
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
                               where MONTH(s.CreatedAt) = MONTH(@Date)
                               and YEAR(s.CreatedAt) = YEAR(@Date)
	                           and o.UserId = @UserId  AND o.UserProfile = @UserProfile
	                           and ( s.State = @DeliveredState  OR s.State = @FinalizedState)";
                        response.EarnedMoney = await connection.QueryFirstOrDefaultAsync<decimal?>(query,
                            new
                            {
                                Date = input.DateTime,
                                UserId = loggedUser.Id,
                                DeliveredState = ShippingState.Delivered,
                                FinalizedState = ShippingState.Finalized,
                                UserProfile = loggedUser.Profile.ToString()
                            }) ?? 0;
                        if (response.EarnedMoney > 0)
                        {
                            response.EarnedMoney -= 0.1m * response.EarnedMoney;
                        }
                    }
                    catch (Exception ex)
                    {
                        ex.CatchIt();
                        response.ErrorMessage += $"EarnedMoney: {ex.Message}";
                    }
                    response.Success = true;
                }
                catch (Exception ex)
                {
                    response.Success = false;
                    response.ErrorMessage = JsonConvert.SerializeObject(ex);
                }
            }
            else
            {
                response.Success = false;
                response.ErrorMessage = $"You must be logged in!";
            }
            return response;
        }
        [HttpPost(Name = "DeleteAccount")]
        public async Task<ApiBaseResponse> DeleteAccount()
        {
            ApiBaseResponse response = new();
            UserIdAndProfile loggedUser = await GetUserIdAndProfileAsync();
            if (loggedUser.Id > 0)
            {
                using SqlConnection connection = AppConfiguration.GetConnection();
                connection.Open();
                try
                {
                    string query = $@"DECLARE @AdsIdents table (Id int);
                                INSERT INTO @AdsIdents
	                                Select Id from Ads
							     WHERE UserId = @UserId;

                                DECLARE @ChatIdents table (Id int);
                                INSERT INTO @ChatIdents
	                                Select Id from Chat 
	                                where AdId in (Select * from @AdsIdents)
                                    or UberUserId = @UserId;
	
                                Delete from ChatMessage
                                where ChatId in (Select * from @ChatIdents);

                                Delete from Chat
                                where Id in (Select * from @ChatIdents);

                                Delete from FavoriteAds
                                where AdId in (Select * from @AdsIdents)
                                or UserId = @UserId;


                                DECLARE @OfferIdents table (Id int)
                                INSERT INTO @OfferIdents
	                                Select Id from Offer 
	                                where AdId in (Select * from @AdsIdents)
                                    or UserId = @UserId;


                                Delete from Payment
                                where OfferId in (Select * from @OfferIdents);

                                Delete from Shipping
                                where OfferId in (Select * from @OfferIdents);

                                Delete from Offer
                                where Id in (Select * from @OfferIdents);

                                Delete from Ads
                                where Id in (Select * from @AdsIdents);

                                Delete from Review 
                                where ReviewedUserId = @UserId
                                OR ReviewerUserId = @UserId;

                                Delete from EmailMessage 
                                where UserToSendId = @UserId;

                                Delete from Notification 
                                where UserId = @UserId;

                                Delete from [User]
                                where Id = @UserId;";

                    _ = await connection.QueryAsync(query, new { UserId = loggedUser.Id });
                    response.Success = true;
                    return response;
                }
                catch (Exception ex)
                {
                    ex.CatchIt();
                    response.Success = false;
                    response.ErrorMessage = ex.Message;
                }
                finally
                {
                    connection.Close();
                }
            }
            else
            {
                response.Success = false;
                response.ErrorMessage = $"You must be logged in!";
            }
            return response;
        }
        [HttpPost(Name = "WriteReview")]
        public async Task<ApiBaseResponse> WriteReview(ReviewRequest reviewRequest)
        {
            ApiBaseResponse response = new();
            UserIdAndProfile loggedUser = await GetUserIdAndProfileAsync();
            if (loggedUser.Id > 0)
            {
                using SqlConnection connection = AppConfiguration.GetConnection();
                connection.Open();
                try
                {
                    string query = $@"select Top 1 UserId from 
                                Offer where Id in (Select OfferId from Shipping where Id = @Id)";
                    int transporterId = await connection.QueryFirstOrDefaultAsync<int>(query, new { Id = reviewRequest.ShippingId });
                    query = $@"Select top 1 UserId from Ads where Id 
                                in (select Top 1 AdId from 
                                Offer where Id in (Select OfferId from Shipping where Id = @Id))";
                    int clientId = await connection.QueryFirstOrDefaultAsync<int>(query, new { Id = reviewRequest.ShippingId });

                    query = $@"select Top 1 UserProfile from 
                                Offer where Id in (Select OfferId from Shipping where Id = @Id)";
                    string transporterProfile = await connection.QueryFirstOrDefaultAsync<string>(query, new { Id = reviewRequest.ShippingId });
                    query = $@"Select top 1 UserProfile from Ads where Id 
                                in (select Top 1 AdId from 
                                Offer where Id in (Select OfferId from Shipping where Id = @Id))";
                    string clientProfile = await connection.QueryFirstOrDefaultAsync<string>(query, new { Id = reviewRequest.ShippingId });

                    int reviewedId = loggedUser.Id == transporterId ? clientId : transporterId;
                    string reviewdProfile = loggedUser.Id == transporterId ? clientProfile : transporterProfile;
                    Review review = new()
                    {
                        CreatedAt = DateTime.Now,
                        Message = reviewRequest.Message,
                        Rating = reviewRequest.Rating,
                        ReviewerUserId = loggedUser.Id,
                        ReviewerUserProfile = loggedUser.Profile.ToString(),
                        ReviewedUserId = reviewedId,
                        ReviewedUserProfile = reviewdProfile
                    };
                    _ = await connection.InsertAsync(review);
                    response.ErrorMessage = string.Empty;
                    response.Success = true;
                    return response;
                }
                catch (Exception ex)
                {
                    ex.CatchIt();
                    response.Success = false;
                    response.ErrorMessage = ex.Message;
                }
                finally
                {
                    connection.Close();
                }
            }
            else
            {
                response.Success = false;
                response.ErrorMessage = $"You must be logged in!";
            }
            return response;
        }
        private string NotificationMessage(OfferState state, string title)
        {
            return state switch
            {
                OfferState.Pending => string.Empty,
                OfferState.Sent => string.Empty,
                OfferState.Seen => $"Tu oferta por {title} ha sido vista.",
                OfferState.PaymentPending => $"Tu oferta por {title} fue aceptada. Esperar a que el cliente realice el pago.",
                OfferState.Accepted => $"Tu oferta por {title} fue aceptada. Ahora puedes ir tras el paquete",
                OfferState.Rejected => $"Tu oferta por {title} fue rechazada.",
                OfferState.Canceled => string.Empty,
                _ => string.Empty,
            };
        }
        private string GetEmailBody()
        {
            return
                    $@"<!DOCTYPE html>
                    <html>
                    <head>
                    <meta charset=""utf-8"" />
                        <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
	                    <title>PassingCar_Co</title>
	                    <link rel=""stylesheet"" href=""~/lib/bootstrap/dist/css/bootstrap.min.css"" />
                        <link rel=""stylesheet"" href=""~/css/site.css"" asp-append-version=""true"" />
                        <link rel=""stylesheet"" href=""~/PassingCar.styles.css"" asp-append-version=""true"" />
	                    <style>
	
	                    .column {{
		                    margin: 20px 20px 20px 20px;
	                    }}
	                    .header {{
		                    background-color: #1892df;
		                    text-align: center;
		
	                    }}
	                    .content{{
		                    background-color:lightgray;
	                    }}
	
	                    .header img {{
		                    max-width: 200px;
		                    text-align: center;
		                    margin-top: 20px;
	                    }}
	
	                    h2.title {{
		                    color: white;
		                    text-align: center;
		                    padding: 0px 0px 40px 0px;
	                    }}
	
	                    .subtitle{{
		                    text-align: center;
		                    padding: 30px 0px;
	                    }}
	
	                    .column_two {{
	                      flex: 50%;
	                      padding: 20px;
	                      height: 300px; /* Should be removed. Only for demonstration */
	                    }}

	                    .row {{
		                    display: flex;
	                    }}
	
	                    .content * {{
		                    box-sizing: border-box;
	                    }}
	
	                    .footer{{
		                    text-align: center;
		                    padding: 10px 0px;
		                    background-color:lightgray;
	                    }}
	                    </style>
                    </head>
                    <body>
	                    <div class=""column"">
		                    <div class=""header"">
			                    <img src=""~/Images/passingcar_white_logo.png"" alt=""PassingCar"" />
			                    <h2 class=""title"">PassingCar - Order Status/Confirm Order/Delivered/Account Created<h2>
		                    </div>
		                    <div class=""content"">
			                    <h3 class=""subtitle""><u>Subtitle</u></h3>
			                    <div class=""row"">
			                      <div class=""column_two"" style=""background-color:#aaa;"">
				                    <h2>Details 1</h2>
				                    <p>Some text..</p>
			                      </div>
			                      <div class=""column_two"" style=""background-color:#bbb;"">
				                    <h2>Details 2</h2>
				                    <p>Some text..</p>
			                      </div>
			  
			                    </div>			
		                    </div>	
		                    <div class=""footer"">
				                    <p>Extra Details - Uber can verify package. || © Copyright 2022 PassingCar Co</p>
		                    </div>		
	                    </div>
                    </body>
                    </html>";
        }

        private string GetOrderByClause(string? relevance)
        {
            return relevance switch
            {
                "El más reciente" => "order by a.CreatedAt desc",
                "El más antiguo" => "order by a.CreatedAt asc",
                "El más caro" => "order by a.Price desc",
                "El más barato" => "order by a.Price asc",
                _ => "order by a.Id desc" // Default: newest first
            };
        }
    }
}
