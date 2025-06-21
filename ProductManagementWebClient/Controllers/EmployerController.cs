using Microsoft.AspNetCore.Mvc;
using ProductManagementWebClient.Helpers;
using BusinessObjects.DTOs.Employer;
using BusinessObjects.DTOs.Common;
using BusinessObjects.DTOs.Authen;
using BusinessObjects.DTOs.Credit;

namespace ProductManagementWebClient.Controllers
{
    public class EmployerController : Controller
    {
        private readonly ApiHelper _apiHelper;

        public EmployerController(ApiHelper apiHelper)
        {
            _apiHelper = apiHelper;
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            // Kiểm tra session trước
            var role = HttpContext.Session.GetString("Role");
            if (string.IsNullOrEmpty(role) || role.ToLower() != "employer")
            {
                return RedirectToAction("Index", "Login");
            }


            try
            {
                var response = await _apiHelper.GetAsync<ApiResponse<EmployerProfileDto>>("api/EmployerApi/profile");

                if (response?.Success == true && response.Data != null)
                {
                    return View(response.Data);
                }

                TempData["Error"] = response?.Message ?? "Không thể lấy thông tin profile";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            // Kiểm tra session trước
            var role = HttpContext.Session.GetString("Role");
            if (string.IsNullOrEmpty(role) || role.ToLower() != "employer")
            {
                return RedirectToAction("Index", "Login");
            }
            try
            {
                var response = await _apiHelper.GetAsync<ApiResponse<EditEmployerProfileDto>>("api/EmployerApi/profile/edit");

                if (response?.Success == true && response.Data != null)
                {
                    return View(response.Data);
                }

                TempData["Error"] = response?.Message ?? "Không thể lấy thông tin chỉnh sửa";
                return RedirectToAction("Profile");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
                return RedirectToAction("Profile");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(EditEmployerProfileDto model, IFormFile? avatarFile)
        {
            // Kiểm tra session trước
            var role = HttpContext.Session.GetString("Role");
            if (string.IsNullOrEmpty(role) || role.ToLower() != "employer")
            {
                return RedirectToAction("Index", "Login");
            }

            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                // Handle avatar file upload first
                if (avatarFile != null && avatarFile.Length > 0)
                {
                    // Validate file size (max 5MB)
                    if (avatarFile.Length > 5 * 1024 * 1024)
                    {
                        TempData["Error"] = "Kích thước file không được vượt quá 5MB";
                        return View(model);
                    }

                    // Validate file type
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var extension = Path.GetExtension(avatarFile.FileName).ToLowerInvariant();
                    if (!allowedExtensions.Contains(extension))
                    {
                        TempData["Error"] = "Chỉ chấp nhận file JPG, PNG, hoặc GIF";
                        return View(model);
                    }

                    // Save file to wwwroot/images/avatars
                    var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "avatars");
                    if (!Directory.Exists(uploadsPath))
                    {
                        Directory.CreateDirectory(uploadsPath);
                    }

                    // Generate unique filename
                    var fileName = $"{Guid.NewGuid()}{extension}";
                    var filePath = Path.Combine(uploadsPath, fileName);

                    // Save file
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await avatarFile.CopyToAsync(stream);
                    }

                    // Set avatar path for model
                    model.Avatar = $"/images/avatars/{fileName}";
                }

                // Create multipart form data
                using var formData = new MultipartFormDataContent();

                // Add form fields
                formData.Add(new StringContent(model.Username ?? ""), "Username");
                formData.Add(new StringContent(model.Email ?? ""), "Email");

                if (!string.IsNullOrEmpty(model.FirstName))
                    formData.Add(new StringContent(model.FirstName), "FirstName");

                if (!string.IsNullOrEmpty(model.LastName))
                    formData.Add(new StringContent(model.LastName), "LastName");

                if (!string.IsNullOrEmpty(model.PhoneNumber))
                    formData.Add(new StringContent(model.PhoneNumber), "PhoneNumber");

                if (!string.IsNullOrEmpty(model.Address))
                    formData.Add(new StringContent(model.Address), "Address");

                // Add avatar path if available
                if (!string.IsNullOrEmpty(model.Avatar))
                    formData.Add(new StringContent(model.Avatar), "Avatar");

                // Add UserId to form data
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId.HasValue)
                {
                    formData.Add(new StringContent(userId.Value.ToString()), "UserId");
                }

                // Use HttpClient with proper configuration
                var httpClient = new HttpClient();
                var token = HttpContext.Session.GetString("Token");
                if (!string.IsNullOrEmpty(token))
                {
                    httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                var baseUrl = "https://localhost:7009";
                var response = await httpClient.PutAsync($"{baseUrl}/api/EmployerApi/profile", formData);

                // Read response content for detailed error information
                var responseContent = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"API Response Status: {response.StatusCode}");
                Console.WriteLine($"API Response Content: {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Cập nhật thông tin thành công!";
                    return RedirectToAction("Profile");
                }
                else
                {
                    // Try to parse error message from response
                    try
                    {
                        var errorResponse = System.Text.Json.JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        TempData["Error"] = errorResponse?.Message ?? $"Cập nhật thông tin thất bại. Status: {response.StatusCode}";
                    }
                    catch
                    {
                        TempData["Error"] = $"Cập nhật thông tin thất bại. Status: {response.StatusCode}. Response: {responseContent}";
                    }

                    return View(model);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in EditProfile: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                TempData["Error"] = $"Lỗi: {ex.Message}";
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> ChangePassword()
        {
            // Kiểm tra session trước
            var role = HttpContext.Session.GetString("Role");
            if (string.IsNullOrEmpty(role) || role.ToLower() != "employer")
            {
                return RedirectToAction("Index", "Login");
            }
            return View(new ChangePasswordDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto model)
        {
            // Kiểm tra session trước
            var role = HttpContext.Session.GetString("Role");
            if (string.IsNullOrEmpty(role) || role.ToLower() != "employer")
            {
                return RedirectToAction("Index", "Login");
            }

            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var response = await _apiHelper.PostAsync<ApiResponse<bool>>("api/EmployerApi/change-password", model);

                if (response?.Success == true)
                {
                    TempData["Success"] = "Đổi mật khẩu thành công!";
                    return RedirectToAction("Profile");
                }

                TempData["Error"] = response?.Message ?? "Đổi mật khẩu thất bại";
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> History()
        {
            // Kiểm tra session trước
            var role = HttpContext.Session.GetString("Role");
            if (string.IsNullOrEmpty(role) || role.ToLower() != "employer")
            {
                return RedirectToAction("Index", "Login");
            }
            try
            {
                var response = await _apiHelper.GetAsync<ApiResponse<EmployerHistoryDto>>("api/EmployerApi/history");

                if (response?.Success == true && response.Data != null)
                {
                    return View(response.Data);
                }

                TempData["Error"] = response?.Message ?? "Không thể lấy lịch sử";
                return RedirectToAction("Profile");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
                return RedirectToAction("Profile");
            }
        }
    }
}
