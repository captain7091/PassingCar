using AuthenticationServices;
using Foundation;
using Newtonsoft.Json.Linq;
using PassingCar.Models.Apple;
using PassingCar.Services;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using UIKit;

namespace PassingCar.CustomServices
{
    public class AppleSignInService : NSObject, IAppleSignIn, IASAuthorizationControllerDelegate, IASAuthorizationControllerPresentationContextProviding
    {
        public bool IsAvailable => UIDevice.CurrentDevice.CheckSystemVersion(13, 0);

        TaskCompletionSource<ASAuthorizationAppleIdCredential> tcsCredential;

        public async Task<AppleSignInCredentialState> GetCredentialStateAsync(string userId)
        {
            var appleIdProvider = new ASAuthorizationAppleIdProvider();
            var credentialState = await appleIdProvider.GetCredentialStateAsync(userId);
            return credentialState switch
            {
                ASAuthorizationAppleIdProviderCredentialState.Authorized => AppleSignInCredentialState.Authorized,
                ASAuthorizationAppleIdProviderCredentialState.Revoked => AppleSignInCredentialState.Revoked,
                ASAuthorizationAppleIdProviderCredentialState.NotFound => AppleSignInCredentialState.NotFound,
                _ => AppleSignInCredentialState.Unknown,
            };
        }

        #region IASAuthorizationController Delegate

        [Export("authorizationController:didCompleteWithAuthorization:")]
        public void DidComplete(ASAuthorizationController controller, ASAuthorization authorization)
        {
            var creds = authorization.GetCredential<ASAuthorizationAppleIdCredential>();
            tcsCredential?.TrySetResult(creds);
        }

        [Export("authorizationController:didCompleteWithError:")]
        public void DidComplete(ASAuthorizationController controller, NSError error)
        {
            // Handle error
            tcsCredential?.TrySetResult(null);
            Console.WriteLine(error);
        }

        #endregion

        #region IASAuthorizationControllerPresentationContextProviding

        public UIWindow GetPresentationAnchor(ASAuthorizationController controller)
        {
            return UIApplication.SharedApplication.KeyWindow;
        }

        #endregion

#if __IOS__13
        AuthManager authManager;
#endif
        bool Is13 => UIDevice.CurrentDevice.CheckSystemVersion(13, 0);
        WebAppleSignInService webSignInService;

        public AppleSignInService()
        {
            if (!Is13)
                webSignInService = new WebAppleSignInService();
        }

        public async Task<AppleAccount> SignInWithAppleAsync()
        {
            // Fallback to web for older iOS versions
            if (!Is13)
                return await webSignInService.SignInWithAppleAsync();

            try
            {
                AppleAccountToken appleAccount = default;

                var provider = new ASAuthorizationAppleIdProvider();
                var req = provider.CreateRequest();

                var authManager = new AuthManager(UIApplication.SharedApplication.KeyWindow);

                req.RequestedScopes = new[] { ASAuthorizationScope.FullName, ASAuthorizationScope.Email };
                var controller = new ASAuthorizationController(new[] { req });

                controller.Delegate = authManager;
                controller.PresentationContextProvider = authManager;

                controller.PerformRequests();

                var creds = await authManager.Credentials;

                if (creds == null)
                    return null;

                appleAccount = new AppleAccountToken
                {
                    IdToken = JwtToken.Decode(new NSString(creds.IdentityToken, NSStringEncoding.UTF8).ToString()),
                    Email = creds.Email,
                    UserId = creds.User,
                    Name = NSPersonNameComponentsFormatter.GetLocalizedString(creds.FullName, NSPersonNameComponentsFormatterStyle.Default, NSPersonNameComponentsFormatterOptions.Phonetic),
                    RealUserStatus = creds.RealUserStatus.ToString()
                };

                return new AppleAccount
                {
                    Email = appleAccount.Email,
                    Name = appleAccount.Name,
                    Token = appleAccount.IdToken.AccessTokenHash,
                    RealUserStatus = appleAccount.RealUserStatus,
                    UserId = appleAccount.UserId
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return null;
        }

        public bool Callback(string url) => true;
    }

    class AuthManager : NSObject, IASAuthorizationControllerDelegate, IASAuthorizationControllerPresentationContextProviding
    {
        public Task<ASAuthorizationAppleIdCredential> Credentials => tcsCredential?.Task;

        TaskCompletionSource<ASAuthorizationAppleIdCredential> tcsCredential;
        UIWindow presentingAnchor;

        public AuthManager(UIWindow presentingWindow)
        {
            tcsCredential = new TaskCompletionSource<ASAuthorizationAppleIdCredential>();
            presentingAnchor = presentingWindow;
        }

        public UIWindow GetPresentationAnchor(ASAuthorizationController controller) => presentingAnchor;

        [Export("authorizationController:didCompleteWithAuthorization:")]
        public void DidComplete(ASAuthorizationController controller, ASAuthorization authorization)
        {
            var creds = authorization.GetCredential<ASAuthorizationAppleIdCredential>();
            tcsCredential?.TrySetResult(creds);
        }

        [Export("authorizationController:didCompleteWithError:")]
        public void DidComplete(ASAuthorizationController controller, NSError error)
        {
            tcsCredential?.TrySetException(new Exception(error.LocalizedDescription));
        }
    }

    public class WebAppleSignInService : IAppleSignIn
    {
        public const string CallbackUriScheme = "xamarinformsapplesignin";
        public const string InitialAuthUrl = "http://local.test:7071/api/applesignin_auth";

        string currentState;
        string currentNonce;

        TaskCompletionSource<AppleAccount> tcsAccount = null;

        public bool Callback(string url)
        {
            if (!url.StartsWith(CallbackUriScheme + "://"))
                return false;

            if (tcsAccount != null && !tcsAccount.Task.IsCompleted)
            {
                try
                {
                    var account = AppleAccountToken.FromUrl(url);

                    if (!account.IdToken.Nonce.Equals(currentNonce))
                        tcsAccount.TrySetException(new InvalidOperationException("Invalid or non-matching nonce returned"));

                    tcsAccount.TrySetResult(new AppleAccount
                    {
                        Email = account.Email,
                        Name = account.Name,
                        Token = account.IdToken.AccessTokenHash,
                        RealUserStatus = account.RealUserStatus,
                        UserId = account.UserId
                    });
                }
                catch (Exception ex)
                {
                    tcsAccount.TrySetException(ex);
                }
            }

            tcsAccount.TrySetResult(null);
            return false;
        }

        public async Task<AppleAccount> SignInWithAppleAsync()
        {
            tcsAccount = new TaskCompletionSource<AppleAccount>();

            currentState = Util.GenerateState();
            currentNonce = Util.GenerateNonce();

            await Browser.OpenAsync($"{InitialAuthUrl}?&state={currentState}&nonce={currentNonce}", BrowserLaunchMode.SystemPreferred);

            return await tcsAccount.Task;
        }
    }

    public static class Util
    {
        public static string GenerateNonce() => GenerateRandom(32);
        public static string GenerateState() => GenerateRandom(12);

        static string GenerateRandom(int len)
        {
            var random = RandomNumberGenerator.Create();
            var data = new byte[len];
            random.GetNonZeroBytes(data);
            return Base64UrlEncode(data);
        }

        internal static string Sha256AtHash(string token)
        {
            var crypt = SHA256.Create();
            var hashBytes = crypt.ComputeHash(Encoding.UTF8.GetBytes(token));
            return Base64UrlEncode(hashBytes, 0, 16);
        }

        internal static string Base64UrlEncode(byte[] data) => Base64UrlEncode(data, 0, data.Length);

        internal static string Base64UrlEncode(byte[] data, int offset, int length)
        {
            var base64 = Convert.ToBase64String(data, offset, length);
            var base64Url = new StringBuilder();

            foreach (var c in base64)
            {
                if (c == '+') base64Url.Append('-');
                else if (c == '/') base64Url.Append('_');
                else if (c == '=') break;
                else base64Url.Append(c);
            }

            return base64Url.ToString();
        }

        internal static byte[] Base64UrlDecode(string encoded)
        {
            var decoded = encoded.Replace('_', '/').Replace('-', '+');

            switch (decoded.Length % 4)
            {
                case 0: break;
                case 2: decoded += "=="; break;
                case 3: decoded += "="; break;
            }

            return Convert.FromBase64String(decoded);
        }

        public static IDictionary<string, string> ParseUrlParameters(string url)
        {
            var d = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(url))
                return d;

            var queryString = url.Contains("#") ? url.Substring(url.IndexOf('#') + 1) :
                               url.Contains("?") ? url.Substring(url.IndexOf('?') + 1) : url;

            foreach (var kvp in queryString.Split('&'))
            {
                var pair = kvp.Split('=');
                if (pair.Length == 2) d[pair[0]] = pair[1];
            }

            return d;
        }
    }

    public class AppleAccountToken
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public JwtToken IdToken { get; set; }
        public string RealUserStatus { get; set; }
        public string UserId { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }

        public string ToQueryParameters() => $"access_token={AccessToken}&refresh_token={RefreshToken}&id_token={IdToken.Raw}";

        public static AppleAccountToken FromUrl(string url)
        {
            var p = Util.ParseUrlParameters(url);
            return new AppleAccountToken
            {
                AccessToken = p["access_token"],
                IdToken = JwtToken.Decode(p["id_token"]),
                RefreshToken = p["refresh_token"]
            };
        }
    }
    public class JwtToken
    {
        public string Raw { get; }
        public string AccessTokenHash { get; }
        public string Nonce { get; }

        public JwtToken(string rawToken)
        {
            Raw = rawToken;

            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(rawToken) as JwtSecurityToken;

            if (jsonToken != null)
            {
                AccessTokenHash = jsonToken.Claims.FirstOrDefault(c => c.Type == "at_hash")?.Value;
                Nonce = jsonToken.Claims.FirstOrDefault(c => c.Type == "nonce")?.Value;
            }
        }

        public static JwtToken Decode(string token)
        {
            return new JwtToken(token);
        }
    }
}
