using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace MVCClient.Controllers
{
    public class RegisterController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RegisterController> _logger;

        public RegisterController(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<RegisterController> logger)
        {
            _httpClient = httpClientFactory.CreateClient();
            _configuration = configuration;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            if (TempData["ErrorMessage"] != null)
            {
                ViewBag.ErrorMessage = TempData["ErrorMessage"].ToString();
            }

            if (TempData["SuccessMessage"] != null)
            {
                ViewBag.SuccessMessage = TempData["SuccessMessage"].ToString();
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(string username, string email, string password,
            string confirmPassword, string firstName, string lastName)
        {
            try
            {
                _logger.LogInformation("Starting registration process for email: {Email}", email);

                // Validate input
                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) ||
                    string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword) ||
                    string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName))
                {
                    TempData["ErrorMessage"] = "Tất cả các trường đều bắt buộc";
                    return RedirectToAction("Index");
                }

                if (password != confirmPassword)
                {
                    TempData["ErrorMessage"] = "Mật khẩu xác nhận không khớp";
                    return RedirectToAction("Index");
                }

                // Tạo object đăng ký
                var registerData = new
                {
                    Username = username,
                    Email = email,
                    Password = password,
                    ConfirmPassword = confirmPassword,
                    FirstName = firstName,
                    LastName = lastName
                };

                var json = JsonSerializer.Serialize(registerData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var baseUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7009";
                var apiUrl = $"{baseUrl}/api/Auth/register";

                _logger.LogInformation("Calling API: {ApiUrl}", apiUrl);
                _logger.LogInformation("Request data: {RequestData}", json);

                var response = await _httpClient.PostAsync(apiUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("API Response Status: {StatusCode}", response.StatusCode);
                _logger.LogInformation("API Response Content: {ResponseContent}", responseContent);

                if (response.IsSuccessStatusCode)
                {
                    // Xử lý response thành công (200-299)
                    try
                    {
                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var result = JsonSerializer.Deserialize<JsonElement>(responseContent);

                        if (result.TryGetProperty("success", out JsonElement successElement))
                        {
                            bool success = successElement.GetBoolean();
                            string message = result.TryGetProperty("message", out JsonElement msgElement)
                                ? msgElement.GetString() ?? "Đăng ký thành công"
                                : "Đăng ký thành công";

                            if (success)
                            {
                                TempData["SuccessMessage"] = message;
                                return RedirectToAction("Index", "Login");
                            }
                            else
                            {
                                TempData["ErrorMessage"] = message;
                            }
                        }
                        else
                        {
                            TempData["SuccessMessage"] = "Đăng ký thành công";
                            return RedirectToAction("Index", "Login");
                        }
                    }
                    catch (JsonException jsonEx)
                    {
                        _logger.LogError(jsonEx, "Failed to parse success response: {Response}", responseContent);
                        TempData["SuccessMessage"] = "Đăng ký thành công";
                        return RedirectToAction("Index", "Login");
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    // Xử lý validation errors (400)
                    try
                    {
                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var errorResult = JsonSerializer.Deserialize<JsonElement>(responseContent);

                        // Kiểm tra có errors property không (ASP.NET Core validation format)
                        if (errorResult.TryGetProperty("errors", out JsonElement errorsElement))
                        {
                            var errorMessages = new List<string>();

                            foreach (var errorProperty in errorsElement.EnumerateObject())
                            {
                                string fieldName = errorProperty.Name;
                                if (errorProperty.Value.ValueKind == JsonValueKind.Array)
                                {
                                    foreach (var errorMessage in errorProperty.Value.EnumerateArray())
                                    {
                                        string message = errorMessage.GetString() ?? "";
                                        if (!string.IsNullOrEmpty(message))
                                        {
                                            // Dịch một số lỗi phổ biến
                                            message = TranslateValidationError(message);
                                            errorMessages.Add($"{GetFieldDisplayName(fieldName)}: {message}");
                                        }
                                    }
                                }
                            }

                            if (errorMessages.Any())
                            {
                                TempData["ErrorMessage"] = string.Join("<br/>", errorMessages);
                            }
                            else
                            {
                                TempData["ErrorMessage"] = "Dữ liệu không hợp lệ";
                            }
                        }
                        else if (errorResult.TryGetProperty("message", out JsonElement msgElement))
                        {
                            TempData["ErrorMessage"] = msgElement.GetString() ?? "Đăng ký thất bại";
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "Dữ liệu không hợp lệ";
                        }
                    }
                    catch (JsonException)
                    {
                        TempData["ErrorMessage"] = "Dữ liệu không hợp lệ";
                    }
                }
                else
                {
                    // Xử lý các lỗi khác (401, 500, etc.)
                    try
                    {
                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var errorResult = JsonSerializer.Deserialize<JsonElement>(responseContent);

                        string errorMessage = "Đăng ký thất bại";
                        if (errorResult.TryGetProperty("message", out JsonElement msgElement))
                        {
                            errorMessage = msgElement.GetString() ?? errorMessage;
                        }

                        TempData["ErrorMessage"] = errorMessage;
                    }
                    catch (JsonException)
                    {
                        TempData["ErrorMessage"] = $"Đăng ký thất bại. Mã lỗi: {response.StatusCode}";
                    }
                }
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "HTTP request failed");
                TempData["ErrorMessage"] = "Không thể kết nối đến server. Vui lòng kiểm tra API có đang chạy không.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during registration");
                TempData["ErrorMessage"] = $"Lỗi không mong muốn: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        private string TranslateValidationError(string message)
        {
            // Dịch một số lỗi validation phổ biến
            var translations = new Dictionary<string, string>
            {
                { "Password must be at least 6 characters", "Mật khẩu phải có ít nhất 6 ký tự" },
                { "The Email field is not a valid e-mail address", "Email không hợp lệ" },
                { "The Username field is required", "Tên đăng nhập là bắt buộc" },
                { "The Password field is required", "Mật khẩu là bắt buộc" },
                { "The Email field is required", "Email là bắt buộc" },
                { "Username already exists", "Tên đăng nhập đã tồn tại" },
                { "Email already exists", "Email đã tồn tại" }
            };

            return translations.ContainsKey(message) ? translations[message] : message;
        }

        private string GetFieldDisplayName(string fieldName)
        {
            // Chuyển đổi tên field thành tiếng Việt
            var fieldNames = new Dictionary<string, string>
            {
                { "Username", "Tên đăng nhập" },
                { "Password", "Mật khẩu" },
                { "Email", "Email" },
                { "FirstName", "Tên" },
                { "LastName", "Họ" },
                { "ConfirmPassword", "Xác nhận mật khẩu" }
            };

            return fieldNames.ContainsKey(fieldName) ? fieldNames[fieldName] : fieldName;
        }
    }
}
