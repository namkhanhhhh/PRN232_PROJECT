using BusinessObjects.DTOs.Credit;
using BusinessObjects.DTOs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using ProductManagementWebClient.Helpers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace MVCClient.Controllers
{
    public class LoginController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ApiHelper _apiHelper;


        public LoginController(IHttpClientFactory httpClientFactory, IConfiguration configuration, ApiHelper apiHelper)
        {
            _httpClient = httpClientFactory.CreateClient();
            _configuration = configuration;
            _apiHelper = apiHelper;
        }

        [HttpGet]
        public IActionResult Index()
        {
            if (TempData["SuccessMessage"] != null)
            {
                ViewBag.SuccessMessage = TempData["SuccessMessage"].ToString();
            }

            if (TempData["ErrorMessage"] != null)
            {
                ViewBag.ErrorMessage = TempData["ErrorMessage"].ToString();
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(string email, string password)
        {
            try
            {
                // Sử dụng anonymous object thay vì DTO
                var loginData = new
                {
                    Email = email,
                    Password = password
                };

                var json = JsonSerializer.Serialize(loginData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var baseUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7009";
                var response = await _httpClient.PostAsync($"{baseUrl}/api/Auth/login", content);

                var responseContent = await response.Content.ReadAsStringAsync();

                // Deserialize thành dynamic object
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var result = JsonSerializer.Deserialize<JsonElement>(responseContent);

                if (response.IsSuccessStatusCode)
                {
                    bool success = result.GetProperty("success").GetBoolean();

                    if (success)
                    {
                        // Lấy thông tin user
                        var user = result.GetProperty("user");
                        int userId = user.GetProperty("id").GetInt32();
                        string username = user.GetProperty("username").GetString() ?? "";
                        string roleName = user.GetProperty("roleName").GetString() ?? "";
                        var balanceResponse = await _apiHelper.GetAsync<ApiResponseDto<UserCreditDto>>($"api/EmployerApi/balance/{userId}");
                        int credit = (int)balanceResponse.Data.Balance;

                        // Lưu vào session
                        HttpContext.Session.SetInt32("UserId", userId);
                        HttpContext.Session.SetString("Username", username);
                        HttpContext.Session.SetString("Role", roleName);
                        HttpContext.Session.SetInt32("credit", credit);

                        // Lưu JWT token
                        if (result.TryGetProperty("token", out JsonElement tokenElement))
                        {
                            string token = tokenElement.GetString() ?? "";
                            HttpContext.Session.SetString("Token", token);

                            // Tạo authentication cookie
                            await SignInUserAsync(userId, username, email, roleName, token);
                        }

                        // Redirect dựa vào role - Updated logic
                        return roleName.ToLower() switch
                        {
                            "admin" => RedirectToAction("Index", "Account"),
                            "employer" => RedirectToAction("Index", "JobPostManagement"), // Thay đổi từ JobPostManagement sang Employer
                            "worker" => RedirectToAction("Index", "Worker"),
                            "customer" => RedirectToAction("SelectRole", "Customer"),
                            _ => RedirectToAction("Index", "Home") // Default to Home instead of SelectRole
                        };
                    }
                    else
                    {
                        string message = result.GetProperty("message").GetString() ?? "Login failed";
                        TempData["ErrorMessage"] = message;
                    }
                }
                else
                {
                    string message = "Login failed. Please try again.";
                    if (result.TryGetProperty("message", out JsonElement messageElement))
                    {
                        message = messageElement.GetString() ?? message;
                    }
                    TempData["ErrorMessage"] = message;
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred during login. Please try again.";
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult GoogleLogin()
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action("GoogleResponse")
            };

            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet]
        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = "Google authentication failed";
                return RedirectToAction("Index");
            }

            var claims = result.Principal.Identities.FirstOrDefault()?.Claims;

            var email = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var name = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            var googleId = claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(name) || string.IsNullOrEmpty(googleId))
            {
                TempData["ErrorMessage"] = "Could not retrieve information from Google";
                return RedirectToAction("Index");
            }

            var externalLoginData = new
            {
                Email = email,
                Name = name,
                Provider = "Google",
                ProviderId = googleId
            };

            var json = JsonSerializer.Serialize(externalLoginData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var baseUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7009";
            var response = await _httpClient.PostAsync($"{baseUrl}/api/Auth/external-login", content);

            var responseContent = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var apiResult = JsonSerializer.Deserialize<JsonElement>(responseContent);

            if (response.IsSuccessStatusCode && apiResult.TryGetProperty("success", out JsonElement successElement) && successElement.GetBoolean())
            {
                // Lấy thông tin user
                var user = apiResult.GetProperty("user");
                int userId = user.GetProperty("id").GetInt32();
                string username = user.GetProperty("username").GetString() ?? "";
                string roleName = user.GetProperty("roleName").GetString() ?? "";
                var balanceResponse = await _apiHelper.GetAsync<ApiResponseDto<UserCreditDto>>($"api/EmployerApi/balance/{userId}");
                int credit = (int)balanceResponse.Data.Balance;
                HttpContext.Session.SetInt32("credit", credit);

                // Lưu vào session
                HttpContext.Session.SetInt32("UserId", userId);
                HttpContext.Session.SetString("Username", username);
                HttpContext.Session.SetString("Role", roleName);

                // Lưu JWT token
                if (apiResult.TryGetProperty("token", out JsonElement tokenElement))
                {
                    string token = tokenElement.GetString() ?? "";
                    HttpContext.Session.SetString("Token", token);

                    // Tạo authentication cookie
                    await SignInUserAsync(userId, username, email, roleName, token);
                }

                // Redirect dựa vào role
                return roleName.ToLower() switch
                {
                    "admin" => RedirectToAction("Index", "Account"),
                    "employer" => RedirectToAction("Index", "JobPostManagement"), // Thay đổi từ JobPostManagement sang Employer
                    "worker" => RedirectToAction("Index", "Worker"),
                    "customer" => RedirectToAction("SelectRole", "Customer"),
                    _ => RedirectToAction("Index", "Home") // Default to Home instead of SelectRole
                };
            }
            else
            {
                string message = "Google login failed";
                if (apiResult.TryGetProperty("message", out JsonElement messageElement))
                {
                    message = messageElement.GetString() ?? message;
                }
                TempData["ErrorMessage"] = message;
                return RedirectToAction("Index");
            }
        }

        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index");
        }

        private async Task SignInUserAsync(int userId, string username, string email, string role, string token)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, role),
                new Claim("Token", token)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(24)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
        }
    }
}
