using ECommerceSystem.GUI.Apis;
using ECommerceSystem.Shared.DTOs.Models;
using ECommerceSystem.Shared.DTOs.User;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Refit;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace ECommerceSystem.GUI.Services
{
    public class AuthService
    {
        private readonly IAuthApi _authApi;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private const string TokenCookieName = "AuthToken";

        public AuthService(IAuthApi authApi, IHttpContextAccessor httpContextAccessor)
        {
            _authApi = authApi;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<(bool Success, string Role)> LoginAsync(LoginModel model)
        {
            try
            {
                var response = await _authApi.Login(model);

                if (!string.IsNullOrWhiteSpace(response.Token))
                {
                    SaveTokenToCookie(response.Token);

                    var handler = new JwtSecurityTokenHandler();
                    var jwtToken = handler.ReadToken(response.Token) as JwtSecurityToken;

                    var role = jwtToken?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

                    if (jwtToken != null)
                    {
                        var claimsIdentity = new ClaimsIdentity(jwtToken.Claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        foreach (var claim in jwtToken.Claims)
                        {
                            Console.WriteLine($"Claim: {claim.Type} = {claim.Value}");
                        }

                        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                        await _httpContextAccessor.HttpContext.SignInAsync(
                            "MyCookieAuth",
                            claimsPrincipal,
                            new AuthenticationProperties
                            {
                                IsPersistent = true,
                                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1)
                            });
                    }

                    return (true, role);
                }

                return (false, null);
            }
            catch (ApiException ex)
            {
                return (false, null);
            }
        }

        public async Task<bool> RegisterAsync(RegisterModel model)
        {
            try
            {
                var response = await _authApi.Register(model);
                return response != null;
            }
            catch (ApiException ex)
            {
                return false;
            }
        }

        public async Task<string> GetCurrentRoleAsync()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null || !user.Identity.IsAuthenticated)
            {
                return null;
            }

            var roleClaim = user.FindFirst(ClaimTypes.Role)?.Value;
            return roleClaim ?? string.Empty;
        }

        public void Logout()
        {
            var response = _httpContextAccessor.HttpContext?.Response;
            response?.Cookies.Delete(TokenCookieName);
        }

        public void SaveTokenToCookie(string token)
        {
            var response = _httpContextAccessor.HttpContext?.Response;
            response?.Cookies.Append(TokenCookieName, token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddHours(1)
            });
        }

        public string GetTokenFromCookie()
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            return request?.Cookies[TokenCookieName];
        }

        public UserInfo GetCurrentUser()
        {
            var token = GetTokenFromCookie();
            if (string.IsNullOrEmpty(token)) return null;

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(token) as JwtSecurityToken;

            if (jwtToken == null) return null;

            var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var email = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var role = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            var name = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

            return new UserInfo
            {
                Id = userId,
                Email = email,
                Role = role,
                Name = name
            };
        }

        public async Task<bool> RefreshTokenAsync()
        {
            var refreshToken = GetRefreshTokenFromCookie();
            if (string.IsNullOrEmpty(refreshToken)) return false;

            try
            {
                var response = await _authApi.Refresh(new RefreshTokenRequest
                {
                    RefreshToken = refreshToken
                });

                SaveTokenToCookie(response.AccessToken);
                SaveRefreshTokenToCookie(response.RefreshToken);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void SaveRefreshTokenToCookie(string refreshToken)
        {
            var response = _httpContextAccessor.HttpContext?.Response;
            response?.Cookies.Append("RefreshToken", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });
        }

        public string GetRefreshTokenFromCookie()
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            return request?.Cookies["RefreshToken"];
        }
    }
}
