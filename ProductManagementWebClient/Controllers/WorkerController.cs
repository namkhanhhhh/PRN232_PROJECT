using BusinessObjects.DTOs;
using BusinessObjects.DTOs.Authen;
using BusinessObjects.DTOs.Worker;
using BusinessObjects.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductManagementWebClient.Helpers;
using System.Security.Claims;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using System.IO;
using System;

namespace ProductManagementWebClient.Controllers
{
    public class WorkerController : Controller
    {
        private readonly ApiHelper _apiHelper;
        private readonly int _pageSize = 10;
        private readonly IConfiguration _configuration;

        public WorkerController(ApiHelper apiHelper, IConfiguration configuration)
        {
            _apiHelper = apiHelper;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            string keyword = "",
            string location = "",
            string jobType = "",
            decimal? minSalary = null,
            decimal? maxSalary = null,
            int categoryId = 0,
            int page = 1,
            int allJobsPage = 1)
        {
            try
            {
                var request = new JobSearchRequestDto
                {
                    Keyword = keyword,
                    Location = location,
                    JobType = jobType,
                    MinSalary = minSalary,
                    MaxSalary = maxSalary,
                    CategoryId = categoryId > 0 ? categoryId : null,
                    Page = page,
                    PageSize = _pageSize,
                    UserId = GetCurrentUserId()
                };

                var response = await _apiHelper.PostAsync<ApiResponseDto<WorkerDashboardDto>>("api/WorkerApi/dashboard", request);

                // Get all jobs for pagination
                var allJobsRequest = new AllJobsRequestDto
                {
                    Page = allJobsPage,
                    PageSize = _pageSize,
                    UserId = GetCurrentUserId()
                };

                var allJobsResponse = await _apiHelper.PostAsync<PaginatedResponseDto<JobPostDto>>("api/WorkerApi/all-jobs", allJobsRequest);

                if (response?.Success == true && response.Data != null)
                {
                    ViewBag.Keyword = keyword;
                    ViewBag.Location = location;
                    ViewBag.JobType = jobType;
                    ViewBag.MinSalary = minSalary;
                    ViewBag.MaxSalary = maxSalary;
                    ViewBag.CategoryId = categoryId;
                    ViewBag.CurrentPage = page;
                    ViewBag.AllJobsPage = allJobsPage;
                    ViewBag.AllJobsData = allJobsResponse?.Success == true ? allJobsResponse : new PaginatedResponseDto<JobPostDto> { Items = new List<JobPostDto>() };

                    return View(response.Data);
                }

                TempData["ErrorMessage"] = response?.Message ?? "Không thể lấy dữ liệu";
                return View(new WorkerDashboardDto());
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi tải trang";
                return View(new WorkerDashboardDto());
            }
        }

        [HttpGet]
        public async Task<IActionResult> SearchResults(
            string keyword = "",
            string location = "",
            string jobType = "",
            decimal? minSalary = null,
            decimal? maxSalary = null,
            int categoryId = 0,
            int page = 1)
        {
            try
            {
                // Validate that at least one search parameter is provided
                bool hasSearchParameters =
                    !string.IsNullOrWhiteSpace(keyword) ||
                    !string.IsNullOrWhiteSpace(location) ||
                    !string.IsNullOrWhiteSpace(jobType) ||
                    minSalary.HasValue ||
                    maxSalary.HasValue ||
                    categoryId > 0;

                if (!hasSearchParameters)
                {
                    return RedirectToAction("Index");
                }

                var request = new JobSearchRequestDto
                {
                    Keyword = keyword,
                    Location = location,
                    JobType = jobType,
                    MinSalary = minSalary,
                    MaxSalary = maxSalary,
                    CategoryId = categoryId > 0 ? categoryId : null,
                    Page = page,
                    PageSize = _pageSize,
                    UserId = GetCurrentUserId()
                };

                var response = await _apiHelper.PostAsync<PaginatedResponseDto<JobPostDto>>("api/WorkerApi/search", request);

                if (response?.Success == true)
                {
                    ViewBag.Keyword = keyword;
                    ViewBag.Location = location;
                    ViewBag.JobType = jobType;
                    ViewBag.MinSalary = minSalary;
                    ViewBag.MaxSalary = maxSalary;
                    ViewBag.CategoryId = categoryId;

                    return View(response);
                }

                TempData["ErrorMessage"] = response?.Message ?? "Không thể tìm kiếm công việc";
                return View(new PaginatedResponseDto<JobPostDto> { Items = new List<JobPostDto>() });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi tìm kiếm";
                return View(new PaginatedResponseDto<JobPostDto> { Items = new List<JobPostDto>() });
            }
        }

        [HttpGet]
        public async Task<IActionResult> JobDetails(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var response = await _apiHelper.GetAsync<ApiResponseDto<JobPostDto>>($"api/WorkerApi/job/{id}?userId={userId}");

                if (response?.Success == true && response.Data != null)
                {
                    return View(response.Data);
                }

                TempData["ErrorMessage"] = response?.Message ?? "Không tìm thấy công việc";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi lấy chi tiết công việc";
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<IActionResult> CategoryJobs(int id, int page = 1)
        {
            try
            {
                var userId = GetCurrentUserId();
                var response = await _apiHelper.GetAsync<PaginatedResponseDto<JobPostDto>>($"api/WorkerApi/category/{id}?page={page}&pageSize={_pageSize}&userId={userId}");

                if (response?.Success == true)
                {
                    ViewBag.CategoryId = id;
                    ViewBag.CurrentPage = page;
                    return View(response);
                }

                TempData["ErrorMessage"] = response?.Message ?? "Không thể lấy danh sách công việc";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi lấy danh sách công việc theo danh mục";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Worker")]
        public async Task<IActionResult> ApplyJob(int jobId)
        {
            try
            {
                var response = await _apiHelper.PostAsync<ApiResponseDto>("api/WorkerApi/apply", jobId);

                if (response?.Success == true)
                {
                    TempData["SuccessMessage"] = response.Message;
                }
                else
                {
                    TempData["ErrorMessage"] = response?.Message ?? "Không thể ứng tuyển";
                }

                return RedirectToAction("JobDetails", new { id = jobId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi ứng tuyển";
                return RedirectToAction("JobDetails", new { id = jobId });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Worker")]
        public async Task<IActionResult> MyApplications()
        {
            try
            {
                var response = await _apiHelper.GetAsync<ApiResponseDto<List<ApplicationDto>>>("api/WorkerApi/applications");

                if (response?.Success == true && response.Data != null)
                {
                    return View(response.Data);
                }

                TempData["ErrorMessage"] = response?.Message ?? "Không thể lấy danh sách đơn ứng tuyển";
                return View(new List<ApplicationDto>());
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi lấy danh sách đơn ứng tuyển";
                return View(new List<ApplicationDto>());
            }
        }

        [HttpGet]
        [Authorize(Roles = "Worker")]
        public async Task<IActionResult> Wishlist()
        {
            try
            {
                var response = await _apiHelper.GetAsync<ApiResponseDto<List<WishlistDto>>>("api/WorkerApi/wishlist");

                if (response?.Success == true && response.Data != null)
                {
                    return View(response.Data);
                }

                TempData["ErrorMessage"] = response?.Message ?? "Không thể lấy danh sách yêu thích";
                return View(new List<WishlistDto>());
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi lấy danh sách yêu thích";
                return View(new List<WishlistDto>());
            }
        }

        [HttpPost]
        [Authorize(Roles = "Worker")]
        public async Task<IActionResult> ToggleWishlist(int jobId)
        {
            try
            {
                var response = await _apiHelper.PostAsync<ApiResponseDto<bool>>("api/WorkerApi/wishlist/toggle", jobId);

                if (response?.Success == true)
                {
                    return Json(new
                    {
                        isInWishlist = response.Data,
                        message = response.Message ?? (response.Data ? "Đã thêm vào danh sách yêu thích" : "Đã xóa khỏi danh sách yêu thích")
                    });
                }

                return Json(new { error = response?.Message ?? "Không thể cập nhật danh sách yêu thích" });
            }
            catch (Exception ex)
            {
                return Json(new { error = "Đã xảy ra lỗi khi cập nhật danh sách yêu thích" });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Worker")]
        public async Task<IActionResult> RemoveFromWishlist(int jobId)
        {
            try
            {
                var response = await _apiHelper.PostAsync<ApiResponseDto<bool>>("api/WorkerApi/wishlist/toggle", jobId);

                if (response?.Success == true)
                {
                    TempData["SuccessMessage"] = "Đã xóa khỏi danh sách yêu thích";
                }
                else
                {
                    TempData["ErrorMessage"] = response?.Message ?? "Không thể xóa khỏi danh sách yêu thích";
                }

                return RedirectToAction("Wishlist");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi xóa khỏi danh sách yêu thích";
                return RedirectToAction("Wishlist");
            }
        }

        [HttpGet]
        public async Task<IActionResult> EmployerProfile(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var response = await _apiHelper.GetAsync<ApiResponseDto<UserDto>>($"api/WorkerApi/employer/{id}?userId={userId}");

                if (response?.Success == true && response.Data != null)
                {
                    // Map UserDto to EmployerProfileViewModel
                    var viewModel = new EmployerProfileViewModel
                    {
                        User = response.Data,
                        UserDetail = response.Data.UserDetail ?? new UserDetailDto(),
                        JobPosts = response.Data.JobPosts ?? new List<JobPostDto>()
                    };

                    return View(viewModel);
                }

                TempData["ErrorMessage"] = response?.Message ?? "Không tìm thấy thông tin nhà tuyển dụng";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi lấy thông tin nhà tuyển dụng";
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        [Route("/HomePage")]
        public IActionResult HomePage()
        {
            return RedirectToAction("Index");
        }

        [HttpGet]
        [Authorize(Roles = "Worker")]
        public async Task<IActionResult> GetWishlistCount()
        {
            try
            {
                var response = await _apiHelper.GetAsync<ApiResponseDto<int>>("api/WorkerApi/wishlist/count");

                if (response?.Success == true)
                {
                    return Json(new { count = response.Data });
                }

                return Json(new { count = 0 });
            }
            catch (Exception ex)
            {
                return Json(new { count = 0 });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Worker")]
        public async Task<IActionResult> GetApplicationsCount()
        {
            try
            {
                var response = await _apiHelper.GetAsync<ApiResponseDto<int>>("api/WorkerApi/applications/count");

                if (response?.Success == true)
                {
                    return Json(new { count = response.Data });
                }

                return Json(new { count = 0 });
            }
            catch (Exception ex)
            {
                return Json(new { count = 0 });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Worker")]
        public async Task<IActionResult> Profile()
        {
            try
            {
                var response = await _apiHelper.GetAsync<ApiResponseDto<UserDto>>("api/WorkerApi/profile");

                if (response?.Success == true && response.Data != null)
                {
                    return View(response.Data);
                }

                TempData["ErrorMessage"] = response?.Message ?? "Không thể lấy thông tin hồ sơ";
                return View(new UserDto());
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi lấy thông tin hồ sơ";
                return View(new UserDto());
            }
        }

        [HttpGet]
        [Authorize(Roles = "Worker")]
        public async Task<IActionResult> EditProfile()
        {
            try
            {
                var response = await _apiHelper.GetAsync<ApiResponseDto<UserDto>>("api/WorkerApi/profile");

                if (response?.Success == true && response.Data != null)
                {
                    var userDetail = response.Data.UserDetail ?? new UserDetailDto();

                    var viewModel = new EditProfileViewModel
                    {
                        FirstName = response.Data.FirstName,
                        LastName = response.Data.LastName,
                        Email = response.Data.Email,
                        PhoneNumber = response.Data.PhoneNumber,
                        Address = userDetail.Address,
                        Headline = userDetail.Headline,
                        Bio = userDetail.Bio,
                        ExperienceYears = userDetail.ExperienceYears,
                        Skills = userDetail.Skills,
                        Education = userDetail.Education,
                        DesiredPosition = userDetail.DesiredPosition,
                        DesiredSalary = userDetail.DesiredSalary,
                        DesiredLocation = userDetail.DesiredLocation,
                        Availability = userDetail.Availability,
                        CurrentAvatar = response.Data.Avatar // Pass current avatar path
                    };

                    return View(viewModel);
                }

                TempData["ErrorMessage"] = response?.Message ?? "Không thể lấy thông tin hồ sơ";
                return View(new EditProfileViewModel());
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi lấy thông tin hồ sơ";
                return View(new EditProfileViewModel());
            }
        }

        [HttpPost]
        [Authorize(Roles = "Worker")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(EditProfileViewModel model, IFormFile? avatarFile) // IFormFile is a parameter, not in model
        {
            // Kiểm tra session trước
            var role = HttpContext.Session.GetString("Role");
            if (string.IsNullOrEmpty(role) || role.ToLower() != "worker")
            {
                return RedirectToAction("Index", "Login");
            }

            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                // Handle avatar file upload
                string? avatarPath = model.CurrentAvatar; // Start with current avatar path

                if (avatarFile != null && avatarFile.Length > 0)
                {
                    // Validate file size (max 5MB)
                    if (avatarFile.Length > 5 * 1024 * 1024)
                    {
                        TempData["ErrorMessage"] = "Kích thước file không được vượt quá 5MB";
                        return View(model);
                    }

                    // Validate file type
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var extension = Path.GetExtension(avatarFile.FileName).ToLowerInvariant();
                    if (!allowedExtensions.Contains(extension))
                    {
                        TempData["ErrorMessage"] = "Chỉ chấp nhận file JPG, PNG, hoặc GIF";
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

                    // Set avatar path for DTO
                    avatarPath = $"/images/avatars/{fileName}";
                }

                // Create DTO to send to API
                var updateProfileDto = new UpdateProfileDto
                {
                    FirstName = model.FirstName ?? "",
                    LastName = model.LastName ?? "",
                    Email = model.Email ?? "",
                    PhoneNumber = model.PhoneNumber,
                    Address = model.Address,
                    Headline = model.Headline,
                    Bio = model.Bio,
                    ExperienceYears = model.ExperienceYears,
                    Skills = model.Skills,
                    Education = model.Education,
                    DesiredPosition = model.DesiredPosition,
                    DesiredSalary = model.DesiredSalary,
                    DesiredLocation = model.DesiredLocation,
                    Availability = model.Availability,
                    Avatar = avatarPath // Pass the new or existing avatar path
                };

                // Use HttpClient with proper configuration
                var httpClient = new HttpClient();
                var token = HttpContext.Session.GetString("Token");
                if (!string.IsNullOrEmpty(token))
                {
                    httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);
                }

                var baseUrl = _configuration["ApiSettings:BaseUrl"];
                if (string.IsNullOrEmpty(baseUrl))
                {
                    throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
                }

                // Use MultipartFormDataContent to send string data and potentially file data (though file is saved locally)
                // This is to match the [FromForm] expectation on the API side for consistency,
                // even if the actual file bytes aren't sent in this specific request.
                // We are sending the path as a string.
                using var formData = new MultipartFormDataContent();
                formData.Add(new StringContent(updateProfileDto.FirstName), "FirstName");
                formData.Add(new StringContent(updateProfileDto.LastName), "LastName");
                formData.Add(new StringContent(updateProfileDto.Email), "Email");

                if (!string.IsNullOrEmpty(updateProfileDto.PhoneNumber))
                    formData.Add(new StringContent(updateProfileDto.PhoneNumber), "PhoneNumber");
                if (!string.IsNullOrEmpty(updateProfileDto.Address))
                    formData.Add(new StringContent(updateProfileDto.Address), "Address");
                if (!string.IsNullOrEmpty(updateProfileDto.Headline))
                    formData.Add(new StringContent(updateProfileDto.Headline), "Headline");
                if (!string.IsNullOrEmpty(updateProfileDto.Bio))
                    formData.Add(new StringContent(updateProfileDto.Bio), "Bio");
                if (updateProfileDto.ExperienceYears.HasValue)
                    formData.Add(new StringContent(updateProfileDto.ExperienceYears.Value.ToString()), "ExperienceYears");
                if (!string.IsNullOrEmpty(updateProfileDto.Skills))
                    formData.Add(new StringContent(updateProfileDto.Skills), "Skills");
                if (!string.IsNullOrEmpty(updateProfileDto.Education))
                    formData.Add(new StringContent(updateProfileDto.Education), "Education");
                if (!string.IsNullOrEmpty(updateProfileDto.DesiredPosition))
                    formData.Add(new StringContent(updateProfileDto.DesiredPosition), "DesiredPosition");
                if (updateProfileDto.DesiredSalary.HasValue)
                    formData.Add(new StringContent(updateProfileDto.DesiredSalary.Value.ToString()), "DesiredSalary");
                if (!string.IsNullOrEmpty(updateProfileDto.DesiredLocation))
                    formData.Add(new StringContent(updateProfileDto.DesiredLocation), "DesiredLocation");
                if (!string.IsNullOrEmpty(updateProfileDto.Availability))
                    formData.Add(new StringContent(updateProfileDto.Availability), "Availability");
                if (!string.IsNullOrEmpty(updateProfileDto.Avatar))
                    formData.Add(new StringContent(updateProfileDto.Avatar), "Avatar"); // Send the avatar path

                var response = await httpClient.PostAsync($"{baseUrl}/api/WorkerApi/profile/update", formData);

                var responseContent = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"API Response Status: {response.StatusCode}");
                Console.WriteLine($"API Response Content: {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Cập nhật hồ sơ thành công";
                    return RedirectToAction("Profile");
                }
                else
                {
                    try
                    {
                        var errorResponse = JsonSerializer.Deserialize<ApiResponseDto>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        TempData["ErrorMessage"] = errorResponse?.Message ?? $"Cập nhật hồ sơ thất bại. Status: {response.StatusCode}";
                    }
                    catch
                    {
                        TempData["ErrorMessage"] = $"Cập nhật hồ sơ thất bại. Status: {response.StatusCode}. Response: {responseContent}";
                    }

                    return View(model);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in EditProfile: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                TempData["ErrorMessage"] = $"Đã xảy ra lỗi khi cập nhật hồ sơ: {ex.Message}";
                return View(model);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Worker")]
        public IActionResult ChangePassword()
        {
            return View(new ChangePasswordViewModel());
        }

        [HttpPost]
        [Authorize(Roles = "Worker")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var changePasswordDto = new ChangePasswordDto
                {
                    CurrentPassword = model.CurrentPassword,
                    NewPassword = model.NewPassword,
                    ConfirmPassword = model.ConfirmPassword
                };

                var response = await _apiHelper.PostAsync<ApiResponseDto>("api/WorkerApi/change-password", changePasswordDto);

                if (response?.Success == true)
                {
                    TempData["SuccessMessage"] = "Đổi mật khẩu thành công";
                    return RedirectToAction("Profile");
                }

                TempData["ErrorMessage"] = response?.Message ?? "Không thể đổi mật khẩu";
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi đổi mật khẩu";
                return View(model);
            }
        }

        private int? GetCurrentUserId()
        {
            if (!User.Identity.IsAuthenticated)
                return null;

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : null;
        }
    }
}
