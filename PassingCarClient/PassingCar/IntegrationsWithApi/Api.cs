using Newtonsoft.Json;

using PassingCar.Interfaces;
using PassingCar.LocalDatabase;
using PassingCar.Models;
using PassingCar.Models.API;
using PassingCar.Models.API.Ads;
using PassingCar.Models.API.Chat;
using PassingCar.Models.API.NotificationNS;
using PassingCar.Models.API.Payment;
using PassingCar.Models.API.Photo;
using PassingCar.Models.API.User;
using PassingCar.Models.API.Validation;
using PassingCar.Utils;
using PassingCar.ViewModels;

using RestSharp;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;


namespace PassingCar.IntegrationsWithApi
{
    public static class Api
    {

        //public static readonly string ApiBaseUrl = "https://polecat-unified-wrongly.ngrok-free.app/";
        public static readonly string ApiBaseUrl = "https://passingcar.discountlagbe.com/";
        //public static readonly string ApiBaseUrl = "https://n4h3nzns-7160.inc1.devtunnels.ms/";
        private static readonly ViewModels.ICustomNotification notification;
        public static bool HasMorePofiles = false;

        // PERFORMANCE FIX: Add call deduplication to prevent multiple simultaneous API calls
        private static readonly Dictionary<string, Task<GetNextAdsResponse>> _activeGetNextAdsCalls = new();
        private static readonly object _callLock = new object();
        
        // CRITICAL FIX: Add connection validation to prevent first-attempt failures
        private static RestClient _cachedClient = null;
        private static DateTime _clientCacheTime = DateTime.MinValue;
        private static readonly TimeSpan ClientCacheTimeout = TimeSpan.FromMinutes(5);
        
        private static RestClient GetCachedClient()
        {
            if (_cachedClient == null || DateTime.Now - _clientCacheTime > ClientCacheTimeout)
            {
                _cachedClient = CreateClientForWS();
                _clientCacheTime = DateTime.Now;
                System.Diagnostics.Debug.WriteLine("[Api] Created new cached RestClient");
                
                // Start keep-alive mechanism for the new client
                _ = Task.Run(async () => await KeepConnectionAlive());
            }
            return _cachedClient;
        }
        
        private static async Task KeepConnectionAlive()
        {
            try
            {
                while (_cachedClient != null && DateTime.Now - _clientCacheTime < ClientCacheTimeout)
                {
                    await Task.Delay(TimeSpan.FromMinutes(2)); // Keep alive every 2 minutes
                    
                    if (_cachedClient != null)
                    {
                        try
                        {
                            // CRITICAL FIX: Use base URL instead of non-existent /health endpoint
                            var keepAliveRequest = new RestRequest("/", Method.Get);
                            keepAliveRequest.Timeout = TimeSpan.FromSeconds(5);
                            _ = await _cachedClient.ExecuteAsync(keepAliveRequest);
                            System.Diagnostics.Debug.WriteLine("[Api] Connection keep-alive ping sent");
                        }
                        catch
                        {
                            // Ignore keep-alive failures
                        }
                    }
                }
            }
            catch
            {
                // Ignore keep-alive task failures
            }
        }
        
        private static async Task<bool> ValidateConnectionAsync()
        {
            try
            {
                var client = GetCachedClient();
                if (client == null) return false;
                
                // CRITICAL FIX: Use base URL GET request instead of non-existent /health endpoint
                var request = new RestRequest("/", Method.Get);
                request.Timeout = TimeSpan.FromSeconds(3); // Quick validation timeout
                
                var response = await client.ExecuteAsync(request);
                // Accept any response that indicates server is reachable (200, 404, etc.)
                bool isValid = response != null && response.ResponseStatus == ResponseStatus.Completed;
                System.Diagnostics.Debug.WriteLine($"[Api] Connection validation result: {isValid}, StatusCode: {response?.StatusCode}");
                return isValid;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Api] Connection validation exception: {ex.Message}");
                // If health check fails, assume connection is available to avoid blocking login
                return true;
            }
        }
        public static async Task<LoginResponse> Login(LoginRequest request, bool firstLogin = true)
        {
            LoginResponse response;
            if (request != null)
            {
                if (string.IsNullOrEmpty(request.Email) && string.IsNullOrEmpty(request.PhoneNumber) && string.IsNullOrEmpty(request.GoogleId) && string.IsNullOrEmpty(request.FacebookId) && string.IsNullOrEmpty(request.AppleId))
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
                    bool loginViaExternalId = !string.IsNullOrEmpty(request.GoogleId) || !string.IsNullOrEmpty(request.FacebookId) || !string.IsNullOrEmpty(request.AppleId);

                    if (!string.IsNullOrEmpty(request.Password) || loginViaExternalId)
                    {
                        try
                        {
                            // CRITICAL FIX: Ensure client is fully warmed up before login attempt
                            System.Diagnostics.Debug.WriteLine("[Api.Login] Starting login request with client pre-warming...");
                            
                            RestClient client = GetCachedClient();
                            
                            // COMMENTED OUT: Connection pre-warming - will be re-enabled later
                            /*
                            // CRITICAL FIX: Pre-warm the connection if this is likely a first attempt
                            if (_cachedClient == null || DateTime.Now - _clientCacheTime < TimeSpan.FromSeconds(30))
                            {
                                System.Diagnostics.Debug.WriteLine("[Api.Login] Pre-warming connection for first-attempt reliability...");
                                try
                                {
                                    var warmupRequest = new RestRequest("/", Method.Get);
                                    warmupRequest.Timeout = TimeSpan.FromSeconds(3);
                                    _ = await client.ExecuteAsync(warmupRequest);
                                    System.Diagnostics.Debug.WriteLine("[Api.Login] Connection pre-warming completed");
                                }
                                catch (Exception warmupEx)
                                {
                                    System.Diagnostics.Debug.WriteLine($"[Api.Login] Connection pre-warming failed (continuing anyway): {warmupEx.Message}");
                                }
                            }
                            */
                            
                            RestRequest apiRquest = new RestRequest("/User/Login", Method.Post);
                            // CRITICAL FIX: Increase timeout for login requests to handle first-attempt issues
                            apiRquest.Timeout = TimeSpan.FromSeconds(15);
                            _ = apiRquest.AddJsonBody(request);
                            
                            // CRITICAL FIX: Single fast attempt for optimal first-time login
                            System.Diagnostics.Debug.WriteLine("[Api.Login] Executing login request...");
                            
                            RestResponse<LoginResponse> apiResponse = await client.ExecuteAsync<LoginResponse>(apiRquest);
                            System.Diagnostics.Debug.WriteLine($"[Api.Login] Response received - StatusCode: {apiResponse?.StatusCode}, ResponseStatus: {apiResponse?.ResponseStatus}");
                            
                            if (apiResponse != null)
                            {
                                if (apiResponse.StatusCode != HttpStatusCode.OK || apiResponse.ResponseStatus != ResponseStatus.Completed)
                                {
                                    response = new LoginResponse()
                                    {
                                        Success = false,
                                        ErrorMessage = $"Eroare apelare api. StatusCode {apiResponse.StatusCode} ResponseStatus {apiResponse.ResponseStatus}",
                                        UserData = null
                                    };
                                }
                                else
                                {
                                    response = apiResponse.Data ?? new LoginResponse()
                                    {
                                        Success = false,
                                        ErrorMessage = "Response data is null",
                                        UserData = null
                                    };

                                    if (response.Success && firstLogin)
                                    {
                                        // Run storage operations in background to not block login
                                        _ = Task.Run(async () =>
                                        {
                                            try
                                            {
                                                await SetUserValues(request, true, response);
                                            }
                                            catch (Exception)
                                            {
                                                // Ignore storage errors during login
                                            }
                                        });
                                    }
                                }
                            }
                            else
                            {
                                response = new LoginResponse()
                                {
                                    Success = false,
                                    ErrorMessage = $"Eroare apelare api. apiResponse is  NULL",
                                    UserData = null
                                };
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"[Api.Login] Login request exception: {ex.Message}");
                            response = new LoginResponse()
                            {
                                Success = false,
                                ErrorMessage = $"Eroare apelare api. {ex.Message}",
                                UserData = null
                            };
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
        public static async Task<GetNotificationsResponse> GetNotifications()
        {
            GetNotificationsResponse response;
            AuthorizeClientForWS client = await CreateAuthorizedClientForWS();
            if (client.Success && client.Client != null)
            {
                RestRequest apiRquest = new RestRequest("/Ads/GetMyNotifications", Method.Post);
                RestResponse apiResponse = await client.Client.ExecuteAsync(apiRquest);
                response = apiResponse != null
                    ? apiResponse.StatusCode != HttpStatusCode.OK || apiResponse.ResponseStatus != ResponseStatus.Completed
                        ? new GetNotificationsResponse()
                        {
                            Success = false,
                            ErrorMessage = $"Eroare apelare api. StatusCode {apiResponse.StatusCode} ResponseStatus {apiResponse.ResponseStatus}",
                            Notifications = new List<Notification>()
                        }
                        : JsonConvert.DeserializeObject<GetNotificationsResponse>(apiResponse.Content)
                    : new GetNotificationsResponse()
                    {
                        Success = false,
                        ErrorMessage = $"Eroare apelare api. apiResponse is  NULL",
                        Notifications = new List<Notification>()
                    };
            }
            else
            {
                response = new GetNotificationsResponse()
                {
                    Success = false,
                    ErrorMessage = client.ErrorMessage,
                    Notifications = new List<Notification>()
                };
            }
            return response;
        }
        public static async Task<GetChatsResponse> GetChats()
        {

            GetChatsResponse response;
            AuthorizeClientForWS client = await CreateAuthorizedClientForWS();
            if (client.Success && client.Client != null)
            {
                RestRequest apiRquest = new RestRequest("/Chat/GetChats", Method.Get);
                RestResponse apiResponse = await client.Client.ExecuteAsync(apiRquest);
                response = apiResponse != null
                    ? apiResponse.StatusCode != HttpStatusCode.OK || apiResponse.ResponseStatus != ResponseStatus.Completed
                        ? new GetChatsResponse()
                        {
                            Success = false,
                            ErrorMessage = $"Eroare apelare api. StatusCode {apiResponse.StatusCode} ResponseStatus {apiResponse.ResponseStatus}",
                            Result = new List<ChatDetails>()
                        }
                        : JsonConvert.DeserializeObject<GetChatsResponse>(apiResponse.Content)
                    : new GetChatsResponse()
                    {
                        Success = false,
                        ErrorMessage = $"Eroare apelare api. apiResponse is  NULL",
                        Result = new List<ChatDetails>()
                    };
            }
            else
            {
                response = new GetChatsResponse()
                {
                    Success = false,
                    ErrorMessage = client.ErrorMessage,
                    Result = new List<ChatDetails>()
                };
            }
            return response;
        }
        public static async Task<GetOffersResponse> GetMyOffers()
        {

            GetOffersResponse response;
            AuthorizeClientForWS client = await CreateAuthorizedClientForWS();
            if (client.Success && client.Client != null)
            {
                RestRequest apiRquest = new RestRequest("/Ads/GetMyOffers", Method.Post);
                RestResponse apiResponse = await client.Client.ExecuteAsync(apiRquest);
                response = apiResponse != null
                    ? apiResponse.StatusCode != HttpStatusCode.OK || apiResponse.ResponseStatus != ResponseStatus.Completed
                        ? new GetOffersResponse()
                        {
                            Success = false,
                            ErrorMessage = $"Eroare apelare api. StatusCode {apiResponse.StatusCode} ResponseStatus {apiResponse.ResponseStatus}"
                        }
                        : JsonConvert.DeserializeObject<GetOffersResponse>(apiResponse.Content)
                    : new GetOffersResponse()
                    {
                        Success = false,
                        ErrorMessage = $"Eroare apelare api. apiResponse is  NULL"
                    };
            }
            else
            {
                response = new GetOffersResponse()
                {
                    Success = false,
                    ErrorMessage = client.ErrorMessage
                };
            }
            return response;
        }
        public static async Task<GetChatDetailsResponse> GetChatDetails(GetChatDetailsInput request)
        {

            GetChatDetailsResponse response;
            AuthorizeClientForWS client = await CreateAuthorizedClientForWS();
            if (client.Success && client.Client != null)
            {
                RestRequest apiRquest = new RestRequest("/Chat/GetChatDetails", Method.Post);
                _ = apiRquest.AddJsonBody(request);
                RestResponse apiResponse = await client.Client.ExecuteAsync(apiRquest);
                response = apiResponse != null
                    ? apiResponse.StatusCode != HttpStatusCode.OK || apiResponse.ResponseStatus != ResponseStatus.Completed
                        ? new GetChatDetailsResponse()
                        {
                            Success = false,
                            ErrorMessage = $"Eroare apelare api. StatusCode {apiResponse.StatusCode} ResponseStatus {apiResponse.ResponseStatus}",
                            Result = new ChatDetails()
                        }
                        : JsonConvert.DeserializeObject<GetChatDetailsResponse>(apiResponse.Content)
                    : new GetChatDetailsResponse()
                    {
                        Success = false,
                        ErrorMessage = $"Eroare apelare api. apiResponse is  NULL",
                        Result = new ChatDetails()
                    };
            }
            else
            {
                response = new GetChatDetailsResponse()
                {
                    Success = false,
                    ErrorMessage = client.ErrorMessage,
                    Result = new ChatDetails()
                };
            }
            return response;
        }
        public static async Task<ApiBaseResponse> Logout()
        {
            try
            {
                await SetUserValues(null, false);
                SecureStorage.RemoveAll();
                
                // CRITICAL FIX: Clear static app variables to prevent profile mixing between users
                App.CurrentUser = null;
                App.HeaderContext = null;
                
                // FIXED: Clear only session data during logout, preserve user ads
                try
                {
                    await App.LocalDatabase.ClearCache(); // Only clear session data, preserve user ads
                    System.Diagnostics.Debug.WriteLine("[Api.Logout] Cleared session cache (user ads preserved)");
                }
                catch (Exception cacheEx)
                {
                    System.Diagnostics.Debug.WriteLine($"[Api.Logout] Error clearing session cache: {cacheEx.Message}");
                }
                
                System.Diagnostics.Debug.WriteLine("[Api.Logout] Successfully cleared all user session data");
                
                return new ApiBaseResponse()
                {
                    ErrorMessage = string.Empty,
                    Success = true,
                };
            }
            catch (Exception ex)
            {
                return new ApiBaseResponse()
                {
                    ErrorMessage = ex.Message,
                    Success = false,
                };
            }
        }
        public static async Task<GetAdsDetailsResponse> GetAdsDetailsAds(GetAdsDetailsInput input)
        {

            GetAdsDetailsResponse response;
            AuthorizeClientForWS client = await CreateAuthorizedClientForWS();
            if (client.Success && client.Client != null)
            {
                RestRequest apiRquest = new RestRequest("/Ads/GetAdsDetails", Method.Post);
                _ = apiRquest.AddJsonBody(input);
                RestResponse apiResponse = await client.Client.ExecuteAsync(apiRquest);
                response = apiResponse != null
                    ? apiResponse.StatusCode != HttpStatusCode.OK || apiResponse.ResponseStatus != ResponseStatus.Completed
                        ? new GetAdsDetailsResponse()
                        {
                            Success = false,
                            ErrorMessage = $"Eroare apelare api. StatusCode {apiResponse.StatusCode} ResponseStatus {apiResponse.ResponseStatus}"
                        }
                        : JsonConvert.DeserializeObject<GetAdsDetailsResponse>(apiResponse.Content)
                    : new GetAdsDetailsResponse()
                    {
                        Success = false,
                        ErrorMessage = $"Eroare apelare api. apiResponse is  NULL"
                    };
            }
            else
            {
                response = new GetAdsDetailsResponse()
                {
                    Success = false,
                    ErrorMessage = client.ErrorMessage
                };
            }
            return response;
        }
        private static async Task SetUserValues(LoginRequest loginRequest, bool logged, LoginResponse loginResponse = null)
        {
            // Optimize: Run all storage operations in parallel
            var tasks = new List<Task>();

            if (loginRequest != null)
                tasks.Add(SecureStorage.SetAsync("LoginRequest", JsonConvert.SerializeObject(loginRequest)));

            tasks.Add(SecureStorage.SetAsync("LoggedIn", JsonConvert.SerializeObject(logged)));

            if (loginResponse != null && !string.IsNullOrEmpty(loginResponse.Token))
            {
                tasks.Add(SecureStorage.SetAsync("Token", JsonConvert.SerializeObject(loginResponse.Token)));
                tasks.Add(SecureStorage.SetAsync("LoggedTime", JsonConvert.SerializeObject(DateTime.Now)));

                if (loginRequest != null && !loginRequest.OnlyToken && loginResponse.UserData != null)
                    tasks.Add(SecureStorage.SetAsync("UserLogged", JsonConvert.SerializeObject(loginResponse.UserData)));
            }

            // Execute all storage operations in parallel
            await Task.WhenAll(tasks);
        }
        public static async Task SetProfile(ProfileType profileType)
        {
            await SecureStorage.SetAsync("ProfileType", profileType.ToString());
            LoginRequest loginRequest = JsonConvert.DeserializeObject<LoginRequest>(await SecureStorage.GetAsync("LoginRequest"));
            if (loginRequest != null)
            {
                loginRequest.OnlyToken = true;
                loginRequest.Profile = profileType;
                await SecureStorage.SetAsync("LoginRequest", JsonConvert.SerializeObject(loginRequest));
                LoginResponse loginResponse = await Login(loginRequest, false);
                if (loginResponse != null && loginResponse.Success && !string.IsNullOrEmpty(loginResponse.Token))
                {
                    await SecureStorage.SetAsync("Token", JsonConvert.SerializeObject(loginResponse.Token));
                }
            }
        }
        public static async Task<ProfileType> GetProfile()
        {
            string profile = await SecureStorage.GetAsync("ProfileType");
            Enum.TryParse(profile, out ProfileType profileType);
            return profileType;
        }
        public static async Task<int> GetUserId()
        {
            try
            {
                string tokenSerialized = await SecureStorage.GetAsync("Token");
                if (string.IsNullOrEmpty(tokenSerialized))
                {
                    return 0;
                }

                string token = JsonConvert.DeserializeObject<string>(tokenSerialized);
                if (string.IsNullOrEmpty(token))
                {
                    return 0;
                }

                JwtSecurityToken jwtTokenDetails = new JwtSecurityTokenHandler().ReadJwtToken(token);
                IEnumerable<Claim> claimsWithIdVal = jwtTokenDetails.Claims.Where(c => c.Type == "Id");
                if (claimsWithIdVal != null && claimsWithIdVal.Count() > 0)
                {
                    Claim userClaim = claimsWithIdVal.FirstOrDefault();
                    if (userClaim != null)
                    {
                        bool parsed = int.TryParse(userClaim.Value, out int userId);
                        if (parsed)
                        {
                            return userId;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var Error = ex.Message;
            }
            return 0;
        }
        public static async Task<User> GetUserData()
        {
            try
            {
                string userSerialized = await SecureStorage.GetAsync("UserLogged");
                if (string.IsNullOrEmpty(userSerialized))
                {
                    return null;
                }

                User user = JsonConvert.DeserializeObject<User>(userSerialized);
                return user;
            }
            catch (Exception ex)
            {
             
            }
            return null;
        }
        public static async Task<InsertResponse> Register(User request)
        {
            InsertResponse response;
            if (request != null)
            {
                if (string.IsNullOrEmpty(request.PhoneNumber) && string.IsNullOrEmpty(request.GoogleId) && string.IsNullOrEmpty(request.FacebookId) && string.IsNullOrEmpty(request.AppleId))
                {
                    response = new InsertResponse()
                    {
                        Success = false,
                        ErrorMessage = $"PhoneNumber is Required",
                        Id = 0
                    };
                }
                else
                {

                    {
                        RestClient client = CreateClientForWS();
                        RestRequest apiRquest = new RestRequest("/User/Register", Method.Post);
                        _ = apiRquest.AddJsonBody(request);
                        RestResponse apiResponse = await client.ExecuteAsync(apiRquest);
                        response = apiResponse != null
                            ? apiResponse.StatusCode != HttpStatusCode.OK || apiResponse.ResponseStatus != ResponseStatus.Completed
                                ? new InsertResponse()
                                {
                                    Success = false,
                                    ErrorMessage = $"Eroare apelare api. StatusCode {apiResponse.StatusCode} ResponseStatus {apiResponse.ResponseStatus}",
                                    Id = 0
                                }
                                : JsonConvert.DeserializeObject<InsertResponse>(apiResponse.Content)
                            : new InsertResponse()
                            {
                                Success = false,
                                ErrorMessage = $"Eroare apelare api. apiResponse is  NULL",
                                Id = 0
                            };

                    }
                }
            }
            else
            {
                response = new InsertResponse()
                {
                    Success = false,
                    ErrorMessage = $"User Request is NULL",
                    Id = 0
                };
            }
            return response;
        }
        public static async Task<ApiBaseResponse> UpdateUser(User request)
        {
            ApiBaseResponse response;
            if (request != null)
            {
                if (string.IsNullOrEmpty(request.PhoneNumber) && string.IsNullOrEmpty(request.GoogleId) && string.IsNullOrEmpty(request.FacebookId) && string.IsNullOrEmpty(request.AppleId))
                {
                    response = new InsertResponse()
                    {
                        Success = false,
                        ErrorMessage = $"PhoneNumber is Required",
                        Id = 0
                    };
                }
                else
                {
                    {
                        RestClient client = CreateClientForWS();
                        RestRequest apiRquest = new RestRequest("/User/Update", Method.Post);
                        _ = apiRquest.AddJsonBody(request);
                        RestResponse apiResponse = await client.ExecuteAsync(apiRquest);
                        response = apiResponse != null
                            ? apiResponse.StatusCode != HttpStatusCode.OK || apiResponse.ResponseStatus != ResponseStatus.Completed
                                ? new ApiBaseResponse()
                                {
                                    Success = false,
                                    ErrorMessage = $"Eroare apelare api. StatusCode {apiResponse.StatusCode} ResponseStatus {apiResponse.ResponseStatus}",
                                }
                                : JsonConvert.DeserializeObject<ApiBaseResponse>(apiResponse.Content)
                            : new ApiBaseResponse()
                            {
                                Success = false,
                                ErrorMessage = $"Eroare apelare api. apiResponse is  NULL"
                            };

                    }
                }
            }
            else
            {
                response = new ApiBaseResponse()
                {
                    Success = false,
                    ErrorMessage = $"User Request is NULL"
                };
            }
            if (response.Success)
                await SecureStorage.SetAsync("UserLogged", JsonConvert.SerializeObject(request));
            return response;
        }
        public static async Task<ApiBaseResponse> UploadNoFactura(UploadNoFacturaRequest request)
        {
            ApiBaseResponse response;
            if (request != null)
            {
                if (string.IsNullOrEmpty(request.NoFactura))
                {
                    response = new InsertResponse()
                    {
                        Success = false,
                        ErrorMessage = $"NoFactura is Required",
                        Id = 0
                    };
                }
                else
                {
                    {
                        AuthorizeClientForWS client = await CreateAuthorizedClientForWS();
                        RestRequest apiRquest = new RestRequest("/Payment/UploadNoFactura", Method.Post);
                        _ = apiRquest.AddJsonBody(request);
                        RestResponse apiResponse = await client.Client.ExecuteAsync(apiRquest);
                        response = apiResponse != null
                            ? apiResponse.StatusCode != HttpStatusCode.OK || apiResponse.ResponseStatus != ResponseStatus.Completed
                                ? new ApiBaseResponse()
                                {
                                    Success = false,
                                    ErrorMessage = $"Eroare apelare api. StatusCode {apiResponse.StatusCode} ResponseStatus {apiResponse.ResponseStatus}",
                                }
                                : JsonConvert.DeserializeObject<ApiBaseResponse>(apiResponse.Content)
                            : new ApiBaseResponse()
                            {
                                Success = false,
                                ErrorMessage = $"Eroare apelare api. apiResponse is  NULL"
                            };

                    }
                }
            }
            else
            {
                response = new ApiBaseResponse()
                {
                    Success = false,
                    ErrorMessage = $"Request is NULL"
                };
            }
            return response;
        }
        public static async Task<UserPublicData> GetUserPublicData(int userID)
        {
            UserPublicData response;
            GetUserProfile request = new GetUserProfile()
            {
                UserId = userID,
            };
            AuthorizeClientForWS client = await CreateAuthorizedClientForWS();
            RestRequest apiRquest = new RestRequest("/User/GetUserPublicData", Method.Post);
            _ = apiRquest.AddJsonBody(request);
            RestResponse apiResponse = await client.Client.ExecuteAsync(apiRquest);
            response = apiResponse != null
                ? apiResponse.StatusCode != HttpStatusCode.OK || apiResponse.ResponseStatus != ResponseStatus.Completed
                    ? new UserPublicData()
                    {
                        Success = false,
                        ErrorMessage = $"Eroare apelare api. StatusCode {apiResponse.StatusCode} ResponseStatus {apiResponse.ResponseStatus}",
                    }
                    : JsonConvert.DeserializeObject<UserPublicData>(apiResponse.Content)
                : new UserPublicData()
                {
                    Success = false,
                    ErrorMessage = $"Eroare apelare api. apiResponse is  NULL"
                };
            return response;
        }
        public static async Task<User> GetUserFromApiValid(string phonenumber)
        {
            User? response;

            RestClient client = CreateClientForWS();
            RestRequest apiRquest = new RestRequest("/User/GetUserFromApiValid", Method.Post);
            _ = apiRquest.AddJsonBody(new User { PhoneNumber = phonenumber });
            RestResponse apiResponse = await client.ExecuteAsync(apiRquest);
            response = apiResponse != null
                ? apiResponse.StatusCode != HttpStatusCode.OK || apiResponse.ResponseStatus != ResponseStatus.Completed
                    ? null
                    : JsonConvert.DeserializeObject<User>(apiResponse.Content)
                : null;
            return response;
        }
        public static async Task<bool> ResetSold(decimal sold)
        {
            bool response;
            AuthorizeClientForWS client = await CreateAuthorizedClientForWS();
            RestRequest apiRquest = new RestRequest("/Ads/ResetSold", Method.Post);
            _ = apiRquest.AddJsonBody(new
            {
                Sold = sold,
            });
            RestResponse apiResponse = await client.Client.ExecuteAsync(apiRquest);
            response = apiResponse != null
                ? apiResponse.StatusCode != HttpStatusCode.OK || apiResponse.ResponseStatus != ResponseStatus.Completed
                    ? false
                    : JsonConvert.DeserializeObject<bool>(apiResponse.Content)
                : false;
            return response;
        }
        public static async Task<bool> HandleException(HandleExceptionInput ex)
        {
            bool response;
            AuthorizeClientForWS client = await CreateAuthorizedClientForWS();
            RestRequest apiRquest = new RestRequest("/Ads/HandleException", Method.Post);
            _ = apiRquest.AddJsonBody(ex);
            RestResponse apiResponse = await client.Client.ExecuteAsync(apiRquest);
            if (apiResponse != null)
            {
                if (apiResponse.StatusCode != HttpStatusCode.OK || apiResponse.ResponseStatus != ResponseStatus.Completed)
                {
                    return true;
                }
                else
                {
                    response = JsonConvert.DeserializeObject<bool>(apiResponse.Content);
                }
            }
            else
            {
                return false;
            }
            return response;
        }
        public static async Task<GetReviewsResponse> GetReviews(GetReviewsInput request)
        {
            GetReviewsResponse response;
            if (request != null)
            {
                {
                    AuthorizeClientForWS client = await CreateAuthorizedClientForWS();
                    RestRequest apiRquest = new RestRequest("/Ads/GetReviews", Method.Post);
                    _ = apiRquest.AddJsonBody(request);
                    RestResponse apiResponse = await client.Client.ExecuteAsync(apiRquest);
                    response = apiResponse != null
                        ? apiResponse.StatusCode != HttpStatusCode.OK || apiResponse.ResponseStatus != ResponseStatus.Completed
                            ? new GetReviewsResponse()
                            {
                                Success = false,
                                ErrorMessage = $"Eroare apelare api. StatusCode {apiResponse.StatusCode} ResponseStatus {apiResponse.ResponseStatus}",
                            }
                            : JsonConvert.DeserializeObject<GetReviewsResponse>(apiResponse.Content)
                        : new GetReviewsResponse()
                        {
                            Success = false,
                            ErrorMessage = $"Eroare apelare api. apiResponse is  NULL"
                        };
                }
            }
            else
            {
                response = new GetReviewsResponse()
                {
                    Success = false,
                    ErrorMessage = $"Request is NULL"
                };
            }
            return response;
        }
        public static async Task<GetOtherUserNameResult> GetOtherUserName(GetOtherUserNameInput request)
        {
            GetOtherUserNameResult response;
            if (request != null)
            {
                {
                    AuthorizeClientForWS client = await CreateAuthorizedClientForWS();
                    RestRequest apiRquest = new RestRequest("/Ads/GetOtherUserName", Method.Post);
                    _ = apiRquest.AddJsonBody(request);
                    RestResponse apiResponse = await client.Client.ExecuteAsync(apiRquest);
                    response = apiResponse != null
                        ? apiResponse.StatusCode != HttpStatusCode.OK || apiResponse.ResponseStatus != ResponseStatus.Completed
                            ? new GetOtherUserNameResult()
                            {
                                Success = false,
                                ErrorMessage = $"Eroare apelare api. StatusCode {apiResponse.StatusCode} ResponseStatus {apiResponse.ResponseStatus}",
                            }
                            : JsonConvert.DeserializeObject<GetOtherUserNameResult>(apiResponse.Content)
                        : new GetOtherUserNameResult()
                        {
                            Success = false,
                            ErrorMessage = $"Eroare apelare api. apiResponse is  NULL"
                        };
                }
            }
            else
            {
                response = new GetOtherUserNameResult()
                {
                    Success = false,
                    ErrorMessage = $"Request is NULL"
                };
            }
            return response;
        }
        public static async Task<ApiBaseResponse> WriteReview(ReviewRequest request)
        {
            ApiBaseResponse response;
            if (request != null)
            {
                AuthorizeClientForWS client = await CreateAuthorizedClientForWS();
                RestRequest apiRquest = new RestRequest("/Ads/WriteReview", Method.Post);
                _ = apiRquest.AddJsonBody(request);
                RestResponse apiResponse = await client.Client.ExecuteAsync(apiRquest);
                response = apiResponse != null
                    ? apiResponse.StatusCode != HttpStatusCode.OK || apiResponse.ResponseStatus != ResponseStatus.Completed
                        ? new ApiBaseResponse()
                        {
                            Success = false,
                            ErrorMessage = $"Eroare apelare api. StatusCode {apiResponse.StatusCode} ResponseStatus {apiResponse.ResponseStatus}",
                        }
                        : JsonConvert.DeserializeObject<ApiBaseResponse>(apiResponse.Content)
                    : new ApiBaseResponse()
                    {
                        Success = false,
                        ErrorMessage = $"Eroare apelare api. apiResponse is  NULL"
                    };
            }
            else
            {
                response = new ApiBaseResponse()
                {
                    Success = false,
                    ErrorMessage = $"Request is NULL"
                };
            }
            return response;
        }
        public static async Task<ApiBaseResponse> CheckConnection()
        {
            ApiBaseResponse response;
            try
            {
                RestClient client = CreateClientForWS();

                RestRequest apiRquest = new RestRequest("/User/CheckConnection", Method.Post);
                // Optimize: Set request timeout using TimeSpan
                apiRquest.Timeout = TimeSpan.FromSeconds(3);

                RestResponse apiResponse = await client.ExecuteAsync(apiRquest);
                response = apiResponse != null
                    ? apiResponse.StatusCode != HttpStatusCode.OK || apiResponse.ResponseStatus != ResponseStatus.Completed
                        ? new ApiBaseResponse()
                        {
                            Success = false,
                            ErrorMessage = $"Eroare apelare api. StatusCode {apiResponse.StatusCode} ResponseStatus {apiResponse.ResponseStatus}",
                        }
                        : JsonConvert.DeserializeObject<ApiBaseResponse>(apiResponse.Content)
                    : new ApiBaseResponse()
                    {
                        Success = false,
                        ErrorMessage = $"Eroare apelare api. apiResponse is  NULL"
                    };
            }
            catch (Exception ex)
            {
                response = new ApiBaseResponse()
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
            return response;
        }
        public static async Task<SendValidationCodeOnPhoneResponse> SendValidationCodeToPhone(SendValidationCodeOnPhoneRequest request)
        {
            SendValidationCodeOnPhoneResponse response;
            if (request != null)
            {
                if (string.IsNullOrEmpty(request.PhoneNumber))
                {
                    response = new SendValidationCodeOnPhoneResponse()
                    {
                        Sent = false,
                        ErrorMessage = $"PhoneNumber is Required"
                    };
                }
                else
                {
                    {
                        RestClient client = CreateClientForWS();
                        RestRequest apiRquest = new RestRequest("/Validation/SendCodeToPhone", Method.Post);
                        _ = apiRquest.AddJsonBody(request);
                        RestResponse apiResponse = await client.ExecuteAsync(apiRquest);
                        response = apiResponse != null
                            ? apiResponse.StatusCode != HttpStatusCode.OK || apiResponse.ResponseStatus != ResponseStatus.Completed
                                ? new SendValidationCodeOnPhoneResponse()
                                {
                                    Sent = false,
                                    ErrorMessage = $"Eroare apelare api. StatusCode {apiResponse.StatusCode} ResponseStatus {apiResponse.ResponseStatus}",
                                }
                                : JsonConvert.DeserializeObject<SendValidationCodeOnPhoneResponse>(apiResponse.Content)
                            : new SendValidationCodeOnPhoneResponse()
                            {
                                Sent = false,
                                ErrorMessage = $"Eroare apelare api. apiResponse is  NULL",
                            };
                    }
                }
            }
            else
            {
                response = new SendValidationCodeOnPhoneResponse()
                {
                    Sent = false,
                    ErrorMessage = $"Request is NULL",
                };
            }
            return response;
        }
        public static async Task<CheckPhoneValidationCodeResponse> CheckPhoneValidationCode(CheckPhoneValidationCodeRequest request)
        {
            CheckPhoneValidationCodeResponse response;
            if (request != null)
            {
                if (string.IsNullOrEmpty(request.PhoneNumber))
                {
                    response = new CheckPhoneValidationCodeResponse()
                    {
                        IsValid = false,
                        ErrorMessage = $"PhoneNumber is required"
                    };
                }
                else
                {
                    {
                        RestClient client = CreateClientForWS();
                        RestRequest apiRquest = new RestRequest("/Validation/CheckPhoneValidationCode", Method.Post);
                        _ = apiRquest.AddJsonBody(request);
                        RestResponse apiResponse = await client.ExecuteAsync(apiRquest);
                        response = apiResponse != null
                            ? apiResponse.StatusCode != HttpStatusCode.OK || apiResponse.ResponseStatus != ResponseStatus.Completed
                                ? new CheckPhoneValidationCodeResponse()
                                {
                                    IsValid = false,
                                    ErrorMessage = $"Eroare apelare api. StatusCode {apiResponse.StatusCode} ResponseStatus {apiResponse.ResponseStatus}",
                                }
                                : JsonConvert.DeserializeObject<CheckPhoneValidationCodeResponse>(apiResponse.Content)
                            : new CheckPhoneValidationCodeResponse()
                            {
                                IsValid = false,
                                ErrorMessage = $"Eroare apelare api. apiResponse is  NULL",
                            };
                    }
                }
            }
            else
            {
                response = new CheckPhoneValidationCodeResponse()
                {
                    IsValid = false,
                    ErrorMessage = $"Request is NULL",
                };
            }
            return response;
        }
        public static async Task<MonthlyStatus> GetMonthlyStatus(MonthlyStatusInput input)
        {

            MonthlyStatus response;
            AuthorizeClientForWS client = await CreateAuthorizedClientForWS();
            if (client.Success && client.Client != null)
            {
                RestRequest apiRquest = new RestRequest("/Ads/GetMonthlyStatus", Method.Post);
                _ = apiRquest.AddJsonBody(input);
                RestResponse apiResponse = await client.Client.ExecuteAsync(apiRquest);
                response = apiResponse != null
                    ? apiResponse.StatusCode != HttpStatusCode.OK || apiResponse.ResponseStatus != ResponseStatus.Completed
                        ? new MonthlyStatus()
                        {
                            Success = false,
                            ErrorMessage = $"Eroare apelare api. StatusCode {apiResponse.StatusCode} ResponseStatus {apiResponse.ResponseStatus}"
                        }
                        : JsonConvert.DeserializeObject<MonthlyStatus>(apiResponse.Content)
                    : new MonthlyStatus()
                    {
                        Success = false,
                        ErrorMessage = $"Eroare apelare api. apiResponse is  NULL"
                    };
            }
            else
            {
                response = new MonthlyStatus()
                {
                    Success = false,
                    ErrorMessage = client.ErrorMessage
                };
            }
            return response;
        }
        public static async Task<InsertResponse> AddAds(AddAdsModel ads)
        {
            System.Diagnostics.Debug.WriteLine("[API] AddAds called");
            System.Diagnostics.Debug.WriteLine($"[API] Ad Title: '{ads?.Title}', Price: {ads?.Price}");

            // Validate input
            if (ads == null)
            {
                return new InsertResponse()
                {
                    Success = false,
                    ErrorMessage = "Error de validación: Los datos del anuncio son requeridos.",
                    Id = 0
                };
            }

            const int maxRetries = 3;
            const int timeoutSeconds = 120; // Increased timeout for image uploads
            
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"[API] AddAds attempt {attempt}/{maxRetries}");
                    
                    AuthorizeClientForWS client = await CreateAuthorizedClientForWS();
                    System.Diagnostics.Debug.WriteLine($"[API] Client authorization: Success={client.Success}, ErrorMessage='{client.ErrorMessage}'");

                    if (client.Success && client.Client != null)
                    {
                        System.Diagnostics.Debug.WriteLine("[API] Creating API request to /Ads/Add");
                        RestRequest apiRquest = new RestRequest("/Ads/Add", Method.Post);
                        
                        // Add timeout to prevent hanging
                        apiRquest.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
                        
                        _ = apiRquest.AddJsonBody(ads);

                        System.Diagnostics.Debug.WriteLine($"[API] Executing API request with {timeoutSeconds}s timeout...");
                        System.Diagnostics.Debug.WriteLine($"[API] Request payload size: {System.Text.Json.JsonSerializer.Serialize(ads).Length} characters");
                        
                        // Use cancellation token for additional timeout control
                        using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds + 5)))
                        {
                            RestResponse apiResponse = await client.Client.ExecuteAsync(apiRquest, cts.Token);

                            System.Diagnostics.Debug.WriteLine($"[API] Response received:");
                            System.Diagnostics.Debug.WriteLine($"[API] - StatusCode: {apiResponse?.StatusCode}");
                            System.Diagnostics.Debug.WriteLine($"[API] - ResponseStatus: {apiResponse?.ResponseStatus}");
                            System.Diagnostics.Debug.WriteLine($"[API] - Content: {apiResponse?.Content}");
                            System.Diagnostics.Debug.WriteLine($"[API] - ErrorMessage: {apiResponse?.ErrorMessage}");

                            if (apiResponse != null)
                            {
                                if (apiResponse.StatusCode == HttpStatusCode.OK && apiResponse.ResponseStatus == ResponseStatus.Completed)
                                {
                                    var response = JsonConvert.DeserializeObject<InsertResponse>(apiResponse.Content);
                                    System.Diagnostics.Debug.WriteLine($"[API] Final response: Success={response?.Success}, Id={response?.Id}, Error='{response?.ErrorMessage}'");
                                    return response;
                                }
                                else if (apiResponse.ResponseStatus == ResponseStatus.TimedOut)
                                {
                                    System.Diagnostics.Debug.WriteLine($"[API] Request timed out on attempt {attempt}");
                                    if (attempt == maxRetries)
                                    {
                                        return new InsertResponse()
                                        {
                                            Success = false,
                                            ErrorMessage = "Tiempo de espera agotado. Por favor, verifique su conexión a internet e intente nuevamente.",
                                            Id = 0
                                        };
                                    }
                                    // Wait before retry
                                    await Task.Delay(2000 * attempt);
                                    continue;
                                }
                                else
                                {
                                    return new InsertResponse()
                                    {
                                        Success = false,
                                        ErrorMessage = $"Error del servidor: {apiResponse.StatusCode}. {apiResponse.ErrorMessage ?? "Error desconocido"}",
                                        Id = 0
                                    };
                                }
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"[API] No response received on attempt {attempt}");
                                if (attempt == maxRetries)
                                {
                                    return new InsertResponse()
                                    {
                                        Success = false,
                                        ErrorMessage = "No se pudo conectar con el servidor. Por favor, verifique su conexión a internet.",
                                        Id = 0
                                    };
                                }
                                // Wait before retry
                                await Task.Delay(2000 * attempt);
                                continue;
                            }
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[API] Client authorization failed: {client.ErrorMessage}");
                        return new InsertResponse()
                        {
                            Success = false,
                            ErrorMessage = client.ErrorMessage ?? "Error de autorización",
                            Id = 0
                        };
                    }
                }
                catch (OperationCanceledException)
                {
                    System.Diagnostics.Debug.WriteLine($"[API] Operation cancelled on attempt {attempt}");
                    if (attempt == maxRetries)
                    {
                        return new InsertResponse()
                        {
                            Success = false,
                            ErrorMessage = "Operación cancelada por tiempo de espera. Por favor, intente nuevamente.",
                            Id = 0
                        };
                    }
                    await Task.Delay(2000 * attempt);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[API] Exception on attempt {attempt}: {ex.Message}");
                    if (attempt == maxRetries)
                    {
                        return new InsertResponse()
                        {
                            Success = false,
                            ErrorMessage = $"Error inesperado: {ex.Message}",
                            Id = 0
                        };
                    }
                    await Task.Delay(2000 * attempt);
                }
            }

            // This should never be reached, but just in case
            return new InsertResponse()
            {
                Success = false,
                ErrorMessage = "Error inesperado durante el envío del anuncio.",
                Id = 0
            };
        }
        public static async Task<GetNextAdsResponse> GetNextAds(GetNextAdsInput input)
        {
            System.Diagnostics.Debug.WriteLine("[API] GetNextAds called");
            System.Diagnostics.Debug.WriteLine($"[API] Current User ID: {App.CurrentUser?.Id ?? 0}");

            // Enhanced logging to debug serialization issues
            try
            {
                var filterJson = input?.Filter != null ? JsonConvert.SerializeObject(input.Filter) : "null";
                System.Diagnostics.Debug.WriteLine($"[API] Input: LastAdsLoaded={input?.LastAdsLoaded}");
                System.Diagnostics.Debug.WriteLine($"[API] Filter JSON: {filterJson}");
            }
            catch (Exception logEx)
            {
                System.Diagnostics.Debug.WriteLine($"[API] Failed to serialize filter for logging: {logEx.Message}");
            }

            // PERFORMANCE FIX: Prevent multiple simultaneous identical calls
            string callKey = $"GetNextAds_{input?.LastAdsLoaded}_{JsonConvert.SerializeObject(input?.Filter ?? new AdsFilter())}";

            Task<GetNextAdsResponse> activeCall = null;
            lock (_callLock)
            {
                if (_activeGetNextAdsCalls.TryGetValue(callKey, out activeCall))
                {
                    System.Diagnostics.Debug.WriteLine($"[API] Deduplicating GetNextAds call - returning existing task");
                }
            }

            // If we found an active call, return it (outside the lock)
            if (activeCall != null)
            {
                return await activeCall;
            }

            // Create and store the task for deduplication
            var task = ExecuteGetNextAdsInternal(input, callKey);

            lock (_callLock)
            {
                _activeGetNextAdsCalls[callKey] = task;
            }

            try
            {
                return await task;
            }
            finally
            {
                // Clean up the active call
                lock (_callLock)
                {
                    _activeGetNextAdsCalls.Remove(callKey);
                }
            }
        }

        private static async Task<GetNextAdsResponse> ExecuteGetNextAdsInternal(GetNextAdsInput input, string callKey)
        {
            GetNextAdsResponse response;
            AuthorizeClientForWS client = await CreateAuthorizedClientForWS();

            System.Diagnostics.Debug.WriteLine($"[API] GetNextAds authorization: Success={client.Success}, ErrorMessage='{client.ErrorMessage}'");

            if (client.Success && client.Client != null)
            {
                System.Diagnostics.Debug.WriteLine("[API] Creating GetNextAds request to /Ads/GetNextAds");
                RestRequest apiRquest = new RestRequest("/Ads/GetNextAds", Method.Post);

                // BALANCED FIX: 15s timeout - compromise between UX and reliability
                apiRquest.Timeout = TimeSpan.FromSeconds(15);

                _ = apiRquest.AddJsonBody(input);

                System.Diagnostics.Debug.WriteLine("[API] Executing GetNextAds request with 15s timeout...");
                RestResponse apiResponse = await client.Client.ExecuteAsync(apiRquest);

                System.Diagnostics.Debug.WriteLine($"[API] GetNextAds response received:");
                System.Diagnostics.Debug.WriteLine($"[API] - StatusCode: {apiResponse?.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"[API] - ResponseStatus: {apiResponse?.ResponseStatus}");
                System.Diagnostics.Debug.WriteLine($"[API] - Content: {apiResponse?.Content}");
                System.Diagnostics.Debug.WriteLine($"[API] - ErrorMessage: {apiResponse?.ErrorMessage}");

                response = apiResponse != null
                    ? apiResponse.StatusCode != HttpStatusCode.OK || apiResponse.ResponseStatus != ResponseStatus.Completed
                        ? new GetNextAdsResponse()
                        {
                            Success = false,
                            ErrorMessage = apiResponse.ResponseStatus == ResponseStatus.TimedOut
                                ? "Network timeout - please check your internet connection and try again"
                                : $"API Error: StatusCode {apiResponse.StatusCode}, Status {apiResponse.ResponseStatus}. Content: {apiResponse.Content}"
                        }
                        : JsonConvert.DeserializeObject<GetNextAdsResponse>(apiResponse.Content)
                    : new GetNextAdsResponse()
                    {
                        Success = false,
                        ErrorMessage = $"Network error - no response received from server"
                    };

                System.Diagnostics.Debug.WriteLine($"[API] GetNextAds final response: Success={response?.Success}, AdsCount={response?.AdsItem?.Count() ?? 0}, Error='{response?.ErrorMessage}'");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[API] GetNextAds authorization failed: {client.ErrorMessage}");
                response = new GetNextAdsResponse()
                {
                    Success = false,
                    ErrorMessage = client.ErrorMessage
                };
            }
            return response;
        }
        public static async Task<GetNextAdsResponse> CheckAdsUpdates(List<LocalAd> localAdsToUpdate)
        {
            CheckAdsUpdatesInput input = new CheckAdsUpdatesInput() { AdsFromLocal = new List<AdsUpdateDetails>() };
            foreach (LocalAd item in localAdsToUpdate)
            {
                input.AdsFromLocal.Add(new AdsUpdateDetails()
                {
                    AdId = item.Id,
                    ModifiedAt = item.ModifiedAt
                });
            }
            GetNextAdsResponse response;
            AuthorizeClientForWS client = await CreateAuthorizedClientForWS();
            if (client.Success && client.Client != null)
            {
                RestRequest apiRquest = new RestRequest("/Ads/CheckAdsUpdates", Method.Post);
                _ = apiRquest.AddJsonBody(input);
                RestResponse apiResponse = await client.Client.ExecuteAsync(apiRquest);
                response = apiResponse != null
                    ? apiResponse.StatusCode != HttpStatusCode.OK || apiResponse.ResponseStatus != ResponseStatus.Completed
                        ? new GetNextAdsResponse()
                        {
                            Success = false,
                            ErrorMessage = $"Eroare apelare api. StatusCode {apiResponse.StatusCode} ResponseStatus {apiResponse.ResponseStatus}"
                        }
                        : JsonConvert.DeserializeObject<GetNextAdsResponse>(apiResponse.Content)
                    : new GetNextAdsResponse()
                    {
                        Success = false,
                        ErrorMessage = $"Eroare apelare api. apiResponse is  NULL"
                    };
            }
            else
            {
                response = new GetNextAdsResponse()
                {
                    Success = false,
                    ErrorMessage = client.ErrorMessage
                };
            }
            return response;
        }
        public static async Task<GetNextShippingResponse> GetNextShipping(GetNextShippingInput input)
        {

            GetNextShippingResponse response;
            AuthorizeClientForWS client = await CreateAuthorizedClientForWS();
            if (client.Success && client.Client != null)
            {
                RestRequest apiRquest = new RestRequest("/Ads/GetNextShipping", Method.Post);
                _ = apiRquest.AddJsonBody(input);
                RestResponse apiResponse = await client.Client.ExecuteAsync(apiRquest);
                response = apiResponse != null
                    ? apiResponse.StatusCode != HttpStatusCode.OK || apiResponse.ResponseStatus != ResponseStatus.Completed
                        ? new GetNextShippingResponse()
                        {
                            Success = false,
                            ErrorMessage = $"Eroare apelare api. StatusCode {apiResponse.StatusCode} ResponseStatus {apiResponse.ResponseStatus}"
                        }
                        : JsonConvert.DeserializeObject<GetNextShippingResponse>(apiResponse.Content)
                    : new GetNextShippingResponse()
                    {
                        Success = false,
                        ErrorMessage = $"Eroare apelare api. apiResponse is  NULL"
                    };
            }
            else
            {
                response = new GetNextShippingResponse()
                {
                    Success = false,
                    ErrorMessage = client.ErrorMessage
                };
            }
            return response;
        }



        public static async Task<GetUserRatingAndSoldResponse> GetUserRatingAndSol()
        {
            GetUserRatingAndSoldResponse response;
            AuthorizeClientForWS client = await CreateAuthorizedClientForWS();
            if (client.Success && client.Client != null)
            {
                RestRequest apiRquest = new RestRequest("/Ads/GetUserRatingAndSold", Method.Post);
                RestResponse apiResponse = await client.Client.ExecuteAsync(apiRquest);
                response = apiResponse != null
                    ? apiResponse.StatusCode != HttpStatusCode.OK || apiResponse.ResponseStatus != ResponseStatus.Completed
                        ? new GetUserRatingAndSoldResponse()
                        {
                            Success = false,
                            ErrorMessage = $"Eroare apelare api. StatusCode {apiResponse.StatusCode} ResponseStatus {apiResponse.ResponseStatus}"
                        }
                        : JsonConvert.DeserializeObject<GetUserRatingAndSoldResponse>(apiResponse.Content)
                    : new GetUserRatingAndSoldResponse()
                    {
                        Success = false,
                        ErrorMessage = $"Eroare apelare api. apiResponse is  NULL"
                    };
            }
            else
            {
                response = new GetUserRatingAndSoldResponse()
                {
                    Success = false,
                    ErrorMessage = client.ErrorMessage
                };
            }
            return response;
        }

        public static async Task<ApiBaseResponse> DeleteAccount()
        {
            ApiBaseResponse response;
            AuthorizeClientForWS client = await CreateAuthorizedClientForWS();
            if (client.Success && client.Client != null)
            {
                RestRequest apiRquest = new RestRequest("/Ads/DeleteAccount", Method.Post);
                RestResponse apiResponse = await client.Client.ExecuteAsync(apiRquest);
                response = apiResponse != null
                    ? apiResponse.StatusCode != HttpStatusCode.OK || apiResponse.ResponseStatus != ResponseStatus.Completed
                        ? new ApiBaseResponse()
                        {
                            Success = false,
                            ErrorMessage = $"Eroare apelare api. StatusCode {apiResponse.StatusCode} ResponseStatus {apiResponse.ResponseStatus}"
                        }
                        : JsonConvert.DeserializeObject<ApiBaseResponse>(apiResponse.Content)
                    : new ApiBaseResponse()
                    {
                        Success = false,
                        ErrorMessage = $"Eroare apelare api. apiResponse is  NULL"
                    };
            }
            else
            {
                response = new ApiBaseResponse()
                {
                    Success = false,
                    ErrorMessage = client.ErrorMessage
                };
            }
            return response;
        }
        public static async Task<GetChatMessagesResponse> GetChatMessages(GetChatMessagesInput input)
        {
            GetChatMessagesResponse response;
            AuthorizeClientForWS client = await CreateAuthorizedClientForWS();
            if (client.Success && client.Client != null)
            {
                RestRequest apiRquest = new RestRequest("/Chat/GetMessages", Method.Post);
                _ = apiRquest.AddJsonBody(input);
                RestResponse apiResponse = await client.Client.ExecuteAsync(apiRquest);
                response = apiResponse != null
                    ? apiResponse.StatusCode != HttpStatusCode.OK || apiResponse.ResponseStatus != ResponseStatus.Completed
                        ? new GetChatMessagesResponse()
                        {
                            Success = false,
                            ErrorMessage = $"Eroare apelare api. StatusCode {apiResponse.StatusCode} ResponseStatus {apiResponse.ResponseStatus}",
                            Result = new List<ChatMessage>()
                        }
                        : JsonConvert.DeserializeObject<GetChatMessagesResponse>(apiResponse.Content)
                    : new GetChatMessagesResponse()
                    {
                        Success = false,
                        ErrorMessage = $"Eroare apelare api. apiResponse is  NULL",
                        Result = new List<ChatMessage>()
                    };
            }
            else
            {
                response = new GetChatMessagesResponse()
                {
                    Success = false,
                    ErrorMessage = client.ErrorMessage,
                    Result = new List<ChatMessage>()
                };
            }
            return response;
        }
        public static async Task<ApiBaseResponse> SendMessage(SendMessageInput input)
        {
            ApiBaseResponse response;
            AuthorizeClientForWS client = await CreateAuthorizedClientForWS();
            if (client.Success && client.Client != null)
            {
                RestRequest apiRquest = new RestRequest("/Chat/SendMessage", Method.Post);
                _ = apiRquest.AddJsonBody(input);
                RestResponse apiResponse = await client.Client.ExecuteAsync(apiRquest);
                response = apiResponse != null
                    ? apiResponse.StatusCode != HttpStatusCode.OK || apiResponse.ResponseStatus != ResponseStatus.Completed
                        ? new ApiBaseResponse()
                        {
                            Success = false,
                            ErrorMessage = $"Eroare apelare api. StatusCode {apiResponse.StatusCode} ResponseStatus {apiResponse.ResponseStatus}",
                        }
                        : JsonConvert.DeserializeObject<ApiBaseResponse>(apiResponse.Content)
                    : new ApiBaseResponse()
                    {
                        Success = false,
                        ErrorMessage = $"Eroare apelare api. apiResponse is  NULL",
                    };
            }
            else
            {
                response = new ApiBaseResponse()
                {
                    Success = false,
                    ErrorMessage = client.ErrorMessage,
                };
            }
            return response;
        }
        public static async Task<ApiBaseResponse> DeleteChat(GetChatMessagesInput input)
        {
            ApiBaseResponse response;
            AuthorizeClientForWS client = await CreateAuthorizedClientForWS();
            if (client.Success && client.Client != null)
            {
                RestRequest apiRquest = new RestRequest("/Chat/DeleteChat", Method.Post);
                _ = apiRquest.AddJsonBody(input);
                RestResponse apiResponse = await client.Client.ExecuteAsync(apiRquest);
                response = apiResponse != null
                    ? apiResponse.StatusCode != HttpStatusCode.OK || apiResponse.ResponseStatus != ResponseStatus.Completed
                        ? new ApiBaseResponse()
                        {
                            Success = false,
                            ErrorMessage = $"Eroare apelare api. StatusCode {apiResponse.StatusCode} ResponseStatus {apiResponse.ResponseStatus}",
                        }
                        : JsonConvert.DeserializeObject<ApiBaseResponse>(apiResponse.Content)
                    : new ApiBaseResponse()
                    {
                        Success = false,
                        ErrorMessage = $"Eroare apelare api. apiResponse is  NULL",
                    };
            }
            else
            {
                response = new ApiBaseResponse()
                {
                    Success = false,
                    ErrorMessage = client.ErrorMessage,
                };
            }
            return response;
        }
        public static async Task<GetChatStateResult> GetChatState(GetChatDetailsInput input)
        {
            GetChatStateResult response;
            AuthorizeClientForWS client = await CreateAuthorizedClientForWS();
            if (client.Success && client.Client != null)
            {
                RestRequest apiRquest = new RestRequest("/Chat/GetChatState", Method.Post);
                _ = apiRquest.AddJsonBody(input);
                RestResponse apiResponse = await client.Client.ExecuteAsync(apiRquest);
                response = apiResponse != null
                    ? apiResponse.StatusCode != HttpStatusCode.OK || apiResponse.ResponseStatus != ResponseStatus.Completed
                        ? new GetChatStateResult()
                        {
                            Success = false,
                            ErrorMessage = $"Eroare apelare api. StatusCode {apiResponse.StatusCode} ResponseStatus {apiResponse.ResponseStatus}",
                        }
                        : JsonConvert.DeserializeObject<GetChatStateResult>(apiResponse.Content)
                    : new GetChatStateResult()
                    {
                        Success = false,
                        ErrorMessage = $"Eroare apelare api. apiResponse is  NULL",
                    };
            }
            else
            {
                response = new GetChatStateResult()
                {
                    Success = false,
                    ErrorMessage = client.ErrorMessage,
                };
            }
            return response;
        }
        public static async Task<ApiBaseResponse> AddToFavorite(AddToFavoriteInput input)
        {
            ApiBaseResponse response;
            AuthorizeClientForWS client = await CreateAuthorizedClientForWS();
            if (client.Success && client.Client != null)
            {
                RestRequest apiRquest = new RestRequest("/Ads/AddToFavorite", Method.Post);
                _ = apiRquest.AddJsonBody(input);
                RestResponse apiResponse = await client.Client.ExecuteAsync(apiRquest);
                response = apiResponse != null
                    ? apiResponse.StatusCode != HttpStatusCode.OK || apiResponse.ResponseStatus != ResponseStatus.Completed
                        ? new ApiBaseResponse()
                        {
                            Success = false,
                            ErrorMessage = $"Eroare apelare api. StatusCode {apiResponse.StatusCode} ResponseStatus {apiResponse.ResponseStatus}",
                        }
                        : JsonConvert.DeserializeObject<ApiBaseResponse>(apiResponse.Content)
                    : new ApiBaseResponse()
                    {
                        Success = false,
                        ErrorMessage = $"Eroare apelare api. apiResponse is  NULL",
                    };
            }
            else
            {
                response = new ApiBaseResponse()
                {
                    Success = false,
                    ErrorMessage = client.ErrorMessage,
                };
            }
            return response;
        }
        public static async Task<ApiBaseResponse> RemoveFromFavorite(AddToFavoriteInput input)
        {
            ApiBaseResponse response;
            AuthorizeClientForWS client = await CreateAuthorizedClientForWS();
            if (client.Success && client.Client != null)
            {
                RestRequest apiRquest = new RestRequest("/Ads/RemoveFromFavorite", Method.Post);
                _ = apiRquest.AddJsonBody(input);
                RestResponse apiResponse = await client.Client.ExecuteAsync(apiRquest);
                response = apiResponse != null
                    ? apiResponse.StatusCode != HttpStatusCode.OK || apiResponse.ResponseStatus != ResponseStatus.Completed
                        ? new ApiBaseResponse()
                        {
                            Success = false,
                            ErrorMessage = $"Eroare apelare api. StatusCode {apiResponse.StatusCode} ResponseStatus {apiResponse.ResponseStatus}",
                        }
                        : JsonConvert.DeserializeObject<ApiBaseResponse>(apiResponse.Content)
                    : new ApiBaseResponse()
                    {
                        Success = false,
                        ErrorMessage = $"Eroare apelare api. apiResponse is  NULL",
                    };
            }
            else
            {
                response = new ApiBaseResponse()
                {
                    Success = false,
                    ErrorMessage = client.ErrorMessage,
                };
            }
            return response;
        }
        public static async Task<SendOfferResult> SendOffer(SendOfferInput input)
        {
            SendOfferResult response;
            AuthorizeClientForWS client = await CreateAuthorizedClientForWS();
            if (client.Success && client.Client != null)
            {
                RestRequest apiRquest = new RestRequest("/Ads/SendOffer", Method.Post);
                _ = apiRquest.AddJsonBody(input);
                RestResponse apiResponse = await client.Client.ExecuteAsync(apiRquest);
                response = apiResponse != null
                    ? apiResponse.StatusCode != HttpStatusCode.OK || apiResponse.ResponseStatus != ResponseStatus.Completed
                        ? new SendOfferResult()
                        {
                            Success = false,
                            ErrorMessage = $"Eroare apelare api. StatusCode {apiResponse.StatusCode} ResponseStatus {apiResponse.ResponseStatus}",
                        }
                        : JsonConvert.DeserializeObject<SendOfferResult>(apiResponse.Content)
                    : new SendOfferResult()
                    {
                        Success = false,
                        ErrorMessage = $"Eroare apelare api. apiResponse is  NULL",
                    };
            }
            else
            {
                response = new SendOfferResult()
                {
                    Success = false,
                    ErrorMessage = client.ErrorMessage,
                };
            }
            return response;
        }
        public static async Task<ApiBaseResponse> ChangeOfferState(ChangeOfferStateInput input)
        {
            ApiBaseResponse response;
            AuthorizeClientForWS client = await CreateAuthorizedClientForWS();
            if (client.Success && client.Client != null)
            {
                RestRequest apiRquest = new RestRequest("/Ads/ChangeOfferState", Method.Post);
                _ = apiRquest.AddJsonBody(input);
                RestResponse apiResponse = await client.Client.ExecuteAsync(apiRquest);
                response = apiResponse != null
                    ? apiResponse.StatusCode != HttpStatusCode.OK || apiResponse.ResponseStatus != ResponseStatus.Completed
                        ? new ApiBaseResponse()
                        {
                            Success = false,
                            ErrorMessage = $"Eroare apelare api. StatusCode {apiResponse.StatusCode} ResponseStatus {apiResponse.ResponseStatus}",
                        }
                        : JsonConvert.DeserializeObject<ApiBaseResponse>(apiResponse.Content)
                    : new ApiBaseResponse()
                    {
                        Success = false,
                        ErrorMessage = $"Eroare apelare api. apiResponse is  NULL",
                    };
            }
            else
            {
                response = new ApiBaseResponse()
                {
                    Success = false,
                    ErrorMessage = client.ErrorMessage,
                };
            }
            return response;
        }
        public static async Task<ApiBaseResponse> ChangeAdsState(ChangeAdsStateInput input)
        {
            ApiBaseResponse response;
            AuthorizeClientForWS client = await CreateAuthorizedClientForWS();
            if (client.Success && client.Client != null)
            {
                RestRequest apiRquest = new RestRequest("/Ads/ChangeAdsState", Method.Post);
                _ = apiRquest.AddJsonBody(input);
                RestResponse apiResponse = await client.Client.ExecuteAsync(apiRquest);
                response = apiResponse != null
                    ? apiResponse.StatusCode != HttpStatusCode.OK || apiResponse.ResponseStatus != ResponseStatus.Completed
                        ? new ApiBaseResponse()
                        {
                            Success = false,
                            ErrorMessage = $"Eroare apelare api. StatusCode {apiResponse.StatusCode} ResponseStatus {apiResponse.ResponseStatus}",
                        }
                        : JsonConvert.DeserializeObject<ApiBaseResponse>(apiResponse.Content)
                    : new ApiBaseResponse()
                    {
                        Success = false,
                        ErrorMessage = $"Eroare apelare api. apiResponse is  NULL",
                    };
            }
            else
            {
                response = new ApiBaseResponse()
                {
                    Success = false,
                    ErrorMessage = client.ErrorMessage,
                };
            }
            return response;
        }
        public static async Task<ChangeShippingStateResponse> ChangeShippingState(ChangesShippingStateInput input)
        {
            ChangeShippingStateResponse response;
            AuthorizeClientForWS client = await CreateAuthorizedClientForWS();
            if (client.Success && client.Client != null)
            {
                RestRequest apiRquest = new RestRequest("/Ads/ChangeShippingState", Method.Post);
                _ = apiRquest.AddJsonBody(input);
                RestResponse apiResponse = await client.Client.ExecuteAsync(apiRquest);
                response = apiResponse != null
                    ? apiResponse.StatusCode != HttpStatusCode.OK || apiResponse.ResponseStatus != ResponseStatus.Completed
                        ? new ChangeShippingStateResponse()
                        {
                            Success = false,
                            ErrorMessage = $"Eroare apelare api. StatusCode {apiResponse.StatusCode} ResponseStatus {apiResponse.ResponseStatus}",
                        }
                        : JsonConvert.DeserializeObject<ChangeShippingStateResponse>(apiResponse.Content)
                    : new ChangeShippingStateResponse()
                    {
                        Success = false,
                        ErrorMessage = $"Eroare apelare api. apiResponse is  NULL",
                    };
            }
            else
            {
                response = new ChangeShippingStateResponse()
                {
                    Success = false,
                    ErrorMessage = client.ErrorMessage,
                };
            }
            return response;
        }
        public static async Task<GetPhotoResponse> GetUserPhoto(GetPhotoRequest input)
        {
            GetPhotoResponse response;
            AuthorizeClientForWS client = await CreateAuthorizedClientForWS();
            if (client.Success && client.Client != null)
            {
                RestRequest apiRquest = new RestRequest("/Ads/GetUserPhoto", Method.Post);
                _ = apiRquest.AddJsonBody(input);
                RestResponse apiResponse = await client.Client.ExecuteAsync(apiRquest);
                response = apiResponse != null
                    ? apiResponse.StatusCode != HttpStatusCode.OK || apiResponse.ResponseStatus != ResponseStatus.Completed
                        ? new GetPhotoResponse()
                        {
                            Success = false,
                            ErrorMessage = $"Eroare apelare api. StatusCode {apiResponse.StatusCode} ResponseStatus {apiResponse.ResponseStatus}",
                        }
                        : JsonConvert.DeserializeObject<GetPhotoResponse>(apiResponse.Content)
                    : new GetPhotoResponse()
                    {
                        Success = false,
                        ErrorMessage = $"Eroare apelare api. apiResponse is  NULL",
                    };
            }
            else
            {
                response = new GetPhotoResponse()
                {
                    Success = false,
                    ErrorMessage = client.ErrorMessage,
                };
            }
            return response;
        }
        public static async Task<GetPhotoResponse> GetAdsPhoto(GetPhotoRequest input)
        {
            GetPhotoResponse response;
            AuthorizeClientForWS client = await CreateAuthorizedClientForWS();
            if (client.Success && client.Client != null)
            {
                RestRequest apiRquest = new RestRequest("/Ads/GetAdsPhoto", Method.Post);
                _ = apiRquest.AddJsonBody(input);
                RestResponse apiResponse = await client.Client.ExecuteAsync(apiRquest);
                response = apiResponse != null
                    ? apiResponse.StatusCode != HttpStatusCode.OK || apiResponse.ResponseStatus != ResponseStatus.Completed
                        ? new GetPhotoResponse()
                        {
                            Success = false,
                            ErrorMessage = $"Eroare apelare api. StatusCode {apiResponse.StatusCode} ResponseStatus {apiResponse.ResponseStatus}",
                        }
                        : JsonConvert.DeserializeObject<GetPhotoResponse>(apiResponse.Content)
                    : new GetPhotoResponse()
                    {
                        Success = false,
                        ErrorMessage = $"Eroare apelare api. apiResponse is  NULL",
                    };
            }
            else
            {
                response = new GetPhotoResponse()
                {
                    Success = false,
                    ErrorMessage = client.ErrorMessage,
                };
            }
            return response;
        }
        public static async Task<ApiBaseResponse> IncrementViewCounter(IncrementViewCounterInput input)
        {
            ApiBaseResponse response;
            AuthorizeClientForWS client = await CreateAuthorizedClientForWS();
            if (client.Success && client.Client != null)
            {
                RestRequest apiRquest = new RestRequest("/Ads/IncrementViewCounter", Method.Post);
                _ = apiRquest.AddJsonBody(input);
                RestResponse apiResponse = await client.Client.ExecuteAsync(apiRquest);
                response = apiResponse != null
                    ? apiResponse.StatusCode != HttpStatusCode.OK || apiResponse.ResponseStatus != ResponseStatus.Completed
                        ? new ApiBaseResponse()
                        {
                            Success = false,
                            ErrorMessage = $"Eroare apelare api. StatusCode {apiResponse.StatusCode} ResponseStatus {apiResponse.ResponseStatus}",
                        }
                        : JsonConvert.DeserializeObject<ApiBaseResponse>(apiResponse.Content)
                    : new ApiBaseResponse()
                    {
                        Success = false,
                        ErrorMessage = $"Eroare apelare api. apiResponse is  NULL",
                    };
            }
            else
            {
                response = new ApiBaseResponse()
                {
                    Success = false,
                    ErrorMessage = client.ErrorMessage,
                };
            }
            return response;
        }

        public static async Task<ApiBaseResponse> UpdateMessagesStatus(UpdateMessageStatusInput input)
        {
            ApiBaseResponse response;
            AuthorizeClientForWS client = await CreateAuthorizedClientForWS();
            if (client.Success && client.Client != null)
            {
                RestRequest apiRquest = new RestRequest("/Chat/UpdateMessagesStatus", Method.Post);
                _ = apiRquest.AddJsonBody(input);
                RestResponse apiResponse = await client.Client.ExecuteAsync(apiRquest);
                response = apiResponse != null
                    ? apiResponse.StatusCode != HttpStatusCode.OK || apiResponse.ResponseStatus != ResponseStatus.Completed
                        ? new ApiBaseResponse()
                        {
                            Success = false,
                            ErrorMessage = $"Eroare apelare api. StatusCode {apiResponse.StatusCode} ResponseStatus {apiResponse.ResponseStatus}",
                        }
                        : JsonConvert.DeserializeObject<ApiBaseResponse>(apiResponse.Content)
                    : new ApiBaseResponse()
                    {
                        Success = false,
                        ErrorMessage = $"Eroare apelare api. apiResponse is  NULL",
                    };
            }
            else
            {
                response = new ApiBaseResponse()
                {
                    Success = false,
                    ErrorMessage = client.ErrorMessage,
                };
            }
            return response;
        }
        public static async Task<GetMessageStateResponse> GetMessageStatus(GetMessageStateInput input)
        {
            GetMessageStateResponse response;
            AuthorizeClientForWS client = await CreateAuthorizedClientForWS();
            if (client.Success && client.Client != null)
            {
                RestRequest apiRquest = new RestRequest("/Chat/GetMessageStatus", Method.Post);
                _ = apiRquest.AddJsonBody(input);
                RestResponse apiResponse = await client.Client.ExecuteAsync(apiRquest);
                response = apiResponse != null
                    ? apiResponse.StatusCode != HttpStatusCode.OK || apiResponse.ResponseStatus != ResponseStatus.Completed
                        ? new GetMessageStateResponse()
                        {
                            Success = false,
                            ErrorMessage = $"Eroare apelare api. StatusCode {apiResponse.StatusCode} ResponseStatus {apiResponse.ResponseStatus}",
                        }
                        : JsonConvert.DeserializeObject<GetMessageStateResponse>(apiResponse.Content)
                    : new GetMessageStateResponse()
                    {
                        Success = false,
                        ErrorMessage = $"Eroare apelare api. apiResponse is  NULL",
                    };
            }
            else
            {
                response = new GetMessageStateResponse()
                {
                    Success = false,
                    ErrorMessage = client.ErrorMessage,
                };
            }
            return response;
        }
        public static async Task<InsertPaymentResponse> AddPayment(InsertPaymentInput input)
        {
            InsertPaymentResponse response;
            AuthorizeClientForWS client = await CreateAuthorizedClientForWS();
            if (client.Success && client.Client != null)
            {
                RestRequest apiRquest = new RestRequest("/Payment/Add", Method.Post);
                _ = apiRquest.AddJsonBody(input);
                RestResponse apiResponse = await client.Client.ExecuteAsync(apiRquest);
                response = apiResponse != null
                    ? apiResponse.StatusCode != HttpStatusCode.OK || apiResponse.ResponseStatus != ResponseStatus.Completed
                        ? new InsertPaymentResponse()
                        {
                            Success = false,
                            ErrorMessage = $"Eroare apelare api. StatusCode {apiResponse.StatusCode} ResponseStatus {apiResponse.ResponseStatus}",
                            PaymentId = 0
                        }
                        : JsonConvert.DeserializeObject<InsertPaymentResponse>(apiResponse.Content)
                    : new InsertPaymentResponse()
                    {
                        Success = false,
                        ErrorMessage = $"Eroare apelare api. apiResponse is  NULL",
                        PaymentId = 0
                    };
            }
            else
            {
                response = new InsertPaymentResponse()
                {
                    Success = false,
                    ErrorMessage = client.ErrorMessage,
                    PaymentId = 0
                };
            }
            return response;
        }
        public static async Task<ApiBaseResponse> UpdatePayResponse(UpdatePayResponseRequest input)
        {
            ApiBaseResponse response;
            AuthorizeClientForWS client = await CreateAuthorizedClientForWS();
            if (client.Success && client.Client != null)
            {
                RestRequest apiRquest = new RestRequest("/Payment/UpdatePayResponse", Method.Post);
                _ = apiRquest.AddJsonBody(input);
                RestResponse apiResponse = await client.Client.ExecuteAsync(apiRquest);
                response = apiResponse != null
                    ? apiResponse.StatusCode != HttpStatusCode.OK || apiResponse.ResponseStatus != ResponseStatus.Completed
                        ? new ApiBaseResponse()
                        {
                            Success = false,
                            ErrorMessage = $"Eroare apelare api. StatusCode {apiResponse.StatusCode} ResponseStatus {apiResponse.ResponseStatus}"
                        }
                        : JsonConvert.DeserializeObject<ApiBaseResponse>(apiResponse.Content)
                    : new ApiBaseResponse()
                    {
                        Success = false,
                        ErrorMessage = $"Eroare apelare api. apiResponse is  NULL"
                    };
            }
            else
            {
                response = new ApiBaseResponse()
                {
                    Success = false,
                    ErrorMessage = client.ErrorMessage
                };
            }
            return response;
        }
        public static async Task<ApiBaseResponse> UpdateRefund(UpdateRefundRequest input)
        {
            ApiBaseResponse response;
            AuthorizeClientForWS client = await CreateAuthorizedClientForWS();
            if (client.Success && client.Client != null)
            {
                RestRequest apiRquest = new RestRequest("/Payment/UpdateRefund", Method.Post);
                _ = apiRquest.AddJsonBody(input);
                RestResponse apiResponse = await client.Client.ExecuteAsync(apiRquest);
                response = apiResponse != null
                    ? apiResponse.StatusCode != HttpStatusCode.OK || apiResponse.ResponseStatus != ResponseStatus.Completed
                        ? new ApiBaseResponse()
                        {
                            Success = false,
                            ErrorMessage = $"Eroare apelare api. StatusCode {apiResponse.StatusCode} ResponseStatus {apiResponse.ResponseStatus}"
                        }
                        : JsonConvert.DeserializeObject<ApiBaseResponse>(apiResponse.Content)
                    : new ApiBaseResponse()
                    {
                        Success = false,
                        ErrorMessage = $"Eroare apelare api. apiResponse is  NULL"
                    };
            }
            else
            {
                response = new ApiBaseResponse()
                {
                    Success = false,
                    ErrorMessage = client.ErrorMessage
                };
            }
            return response;
        }
        public static async Task<ApiBaseResponse> UpdateRefundResponse(UpdateRefundResponseRequest input)
        {
            ApiBaseResponse response;
            AuthorizeClientForWS client = await CreateAuthorizedClientForWS();
            if (client.Success && client.Client != null)
            {
                RestRequest apiRquest = new RestRequest("/Payment/UpdateRefundResponse", Method.Post);
                _ = apiRquest.AddJsonBody(input);
                RestResponse apiResponse = await client.Client.ExecuteAsync(apiRquest);
                response = apiResponse != null
                    ? apiResponse.StatusCode != HttpStatusCode.OK || apiResponse.ResponseStatus != ResponseStatus.Completed
                        ? new ApiBaseResponse()
                        {
                            Success = false,
                            ErrorMessage = $"Eroare apelare api. StatusCode {apiResponse.StatusCode} ResponseStatus {apiResponse.ResponseStatus}"
                        }
                        : JsonConvert.DeserializeObject<ApiBaseResponse>(apiResponse.Content)
                    : new ApiBaseResponse()
                    {
                        Success = false,
                        ErrorMessage = $"Eroare apelare api. apiResponse is  NULL"
                    };
            }
            else
            {
                response = new ApiBaseResponse()
                {
                    Success = false,
                    ErrorMessage = client.ErrorMessage
                };
            }
            return response;
        }
        public static async Task<GetPaymentDetailsResponse> GetPaymentDetails(int OfferId)
        {
            GetPaymentDetailsResponse response;
            AuthorizeClientForWS client = await CreateAuthorizedClientForWS();
            if (client.Success && client.Client != null)
            {
                RestRequest apiRquest = new RestRequest("/Payment/GetPaymentDetails", Method.Post);
                _ = apiRquest.AddJsonBody(new GetPaymentDetailsInput()
                {
                    OfferId = OfferId,
                });
                RestResponse apiResponse = await client.Client.ExecuteAsync(apiRquest);
                response = apiResponse != null
                    ? apiResponse.StatusCode != HttpStatusCode.OK || apiResponse.ResponseStatus != ResponseStatus.Completed
                        ? new GetPaymentDetailsResponse()
                        {
                            Success = false,
                            ErrorMessage = $"Eroare apelare api. StatusCode {apiResponse.StatusCode} ResponseStatus {apiResponse.ResponseStatus}"
                        }
                        : JsonConvert.DeserializeObject<GetPaymentDetailsResponse>(apiResponse.Content)
                    : new GetPaymentDetailsResponse()
                    {
                        Success = false,
                        ErrorMessage = $"Eroare apelare api. apiResponse is  NULL"
                    };
            }
            else
            {
                response = new GetPaymentDetailsResponse()
                {
                    Success = false,
                    ErrorMessage = client.ErrorMessage
                };
            }
            return response;
        }
        public static async Task<GetPaymentProofResponse> DownloadPaymentProof(int OfferId)
        {
            GetPaymentDetailsResponse getPaymentDetails = await GetPaymentDetails(OfferId);
            if (getPaymentDetails == null)
            {
                return new GetPaymentProofResponse()
                {
                    Success = false,
                    ErrorMessage = $"Unable to receive payment details"
                };
            }
            if (getPaymentDetails.Success == false)
            {
                return new GetPaymentProofResponse()
                {
                    Success = false,
                    ErrorMessage = getPaymentDetails.ErrorMessage
                };
            }
            if (getPaymentDetails.PaymentDetails == null)
            {
                return new GetPaymentProofResponse()
                {
                    Success = false,
                    ErrorMessage = $"No payment details"
                };
            }
            if (getPaymentDetails.PaymentDetails.PaymentProofModel == null)
            {
                return new GetPaymentProofResponse()
                {
                    Success = false,
                    ErrorMessage = $"No payment proof details"
                };
            }

            AuthorizeClientForWS client = await CreateAuthorizedClientForWS();
            if (client.Success && client.Client != null)
            {
                RestRequest apiRquest = new RestRequest("/Home/PaymentProof", Method.Post);
                _ = apiRquest.AddJsonBody(getPaymentDetails.PaymentDetails.PaymentProofModel);

                byte[] pdfBytes = client.Client.DownloadData(apiRquest);
                return pdfBytes == null || pdfBytes.Length == 0
                    ? new GetPaymentProofResponse()
                    {
                        Success = false,
                        ErrorMessage = $"No content for payment proof"
                    }
                    : new GetPaymentProofResponse()
                    {
                        Success = true,
                        ErrorMessage = string.Empty,
                        PaymentProofBytes = pdfBytes
                    };
            }
            else
            {
                return new GetPaymentProofResponse()
                {
                    Success = false,
                    ErrorMessage = $"Cannot generate PaymentProof"
                };
            }
        }
        private static RestClient CreateClientForWS()
        {
            try
            {
                if (!string.IsNullOrEmpty(ApiBaseUrl))
                {
                    var options = new RestClientOptions(ApiBaseUrl)
                    {
                        // CRITICAL FIX: Maximum timeout for first-attempt login reliability and image uploads
                        Timeout = TimeSpan.FromSeconds(60),
                        // Enable aggressive connection pooling with retry logic
                        ConfigureMessageHandler = handler =>
                        {
                            if (handler is HttpClientHandler httpHandler)
                            {
                                // CRITICAL FIX: Aggressive optimization for first-attempt success
                                httpHandler.MaxConnectionsPerServer = 25; // Maximum connections
                                httpHandler.UseCookies = false; // Disable cookies for faster requests
                                httpHandler.UseDefaultCredentials = false;
                                httpHandler.PreAuthenticate = false;
                                
                                // Enable automatic decompression for better performance
                                httpHandler.AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate;
                                
                                // Additional mobile network optimizations
                                httpHandler.MaxRequestContentBufferSize = 2 * 1024 * 1024; // 2MB buffer
                                
                                // Connection keep-alive settings
                                if (httpHandler.SupportsRedirectConfiguration)
                                {
                                    httpHandler.AllowAutoRedirect = true;
                                    httpHandler.MaxAutomaticRedirections = 5;
                                }
                                
                                // Note: PooledConnectionLifetime and PooledConnectionIdleTimeout are not available in this .NET version
                                // Connection pooling is handled automatically by the framework
                            }
                            return handler;
                        }
                    };

                    return new RestClient(options);
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }
        
        // CRITICAL FIX: Add method to create test client for connection warmup
        public static RestClient CreateTestClient()
        {
            return CreateClientForWS();
        }
        private static async Task<AuthorizeClientForWS> CreateAuthorizedClientForWS()
        {
            try
            {
                if (!string.IsNullOrEmpty(ApiBaseUrl))
                {
                    try
                    {
                        DateTime loggedTime = JsonConvert.DeserializeObject<DateTime>(await SecureStorage.GetAsync("LoggedTime"));
                        string token = JsonConvert.DeserializeObject<string>(await SecureStorage.GetAsync("Token"));
                        if (loggedTime != null && !string.IsNullOrEmpty(token) && (DateTime.Now - loggedTime <= TimeSpan.FromHours(11)))
                        {
                            RestClient client = CreateClientForWS();
                            _ = client.AddDefaultHeader("Authorization", $"Bearer {token}");
                            return new AuthorizeClientForWS()
                            {
                                Success = true,
                                ErrorMessage = string.Empty,
                                Client = client,
                                Token = token
                            };
                        }
                    }
                    catch (Exception)
                    {
                    }
                    try
                    {
                        LoginRequest loginRequest = JsonConvert.DeserializeObject<LoginRequest>(await SecureStorage.GetAsync("LoginRequest"));
                        if (loginRequest != null)
                        {
                            loginRequest.OnlyToken = true;
                            LoginResponse loginResponse = await Login(loginRequest, false);
                            if (loginResponse != null)
                            {
                                if (loginResponse.Success && !string.IsNullOrEmpty(loginResponse.Token))
                                {
                                    RestClient client = CreateClientForWS();
                                    _ = client.AddDefaultHeader("Authorization", $"Bearer {loginResponse.Token}");
                                    await SecureStorage.SetAsync("Token", JsonConvert.SerializeObject(loginResponse.Token));
                                    await SecureStorage.SetAsync("LoggedTime", JsonConvert.SerializeObject(DateTime.Now.AddMinutes(-5)));
                                    // await PassingCarHubs.ReconnectHubs();
                                    return new AuthorizeClientForWS()
                                    {
                                        Success = true,
                                        ErrorMessage = string.Empty,
                                        Client = client
                                    };

                                }
                                else
                                {
                                    return new AuthorizeClientForWS()
                                    {
                                        Success = false,
                                        ErrorMessage = $"Failed to login.{loginResponse.ErrorMessage}",
                                        Client = null
                                    };
                                }
                            }
                            else
                            {
                                return new AuthorizeClientForWS()
                                {
                                    Success = false,
                                    ErrorMessage = $"Failed to login. loginResponse is Null. Please login again",
                                    Client = null
                                };
                            }
                        }
                        else
                        {
                            return new AuthorizeClientForWS()
                            {
                                Success = false,
                                ErrorMessage = $"Failed to login. loginRequest is Null. Please login again",
                                Client = null
                            };
                        }

                    }
                    catch (Exception ex)
                    {
                        return new AuthorizeClientForWS()
                        {
                            Success = false,
                            ErrorMessage = $"{ex.Message}",
                            Client = null
                        };
                    }
                }
                else
                {
                    return new AuthorizeClientForWS()
                    {
                        Success = false,
                        ErrorMessage = $"Api BASE URL is null",
                        Client = null
                    };
                }
            }
            catch (Exception ex)
            {
                return new AuthorizeClientForWS()
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    Client = null
                };
            }
        }
    }
    public class AuthorizeClientForWS : ApiBaseResponse
    {
        public RestClient Client { get; set; }
        public string Token { get; set; }
    }
}
