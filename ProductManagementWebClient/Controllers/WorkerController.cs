using BusinessObjects.DTOs;
using BusinessObjects.DTOs.Authen;
using BusinessObjects.DTOs.Worker;
using BusinessObjects.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductManagementWebClient.Helpers;
using System.Security.Claims;

namespace ProductManagementWebClient.Controllers
{
    public class WorkerController : Controller
    {
        private readonly ApiHelper _apiHelper;
        private readonly int _pageSize = 10;

        public WorkerController(ApiHelper apiHelper)
        {
            _apiHelper = apiHelper;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
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

                if (response?.Success == true && response.Data != null)
                {
                    ViewBag.Keyword = keyword;
                    ViewBag.Location = location;
                    ViewBag.JobType = jobType;
                    ViewBag.MinSalary = minSalary;
                    ViewBag.MaxSalary = maxSalary;
                    ViewBag.CategoryId = categoryId;
                    ViewBag.CurrentPage = page;

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
                    return Json(new { isInWishlist = response.Data, message = response.Message });
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
                    // Đảm bảo UserDetail không null
                    var userDetail = response.Data.UserDetail ?? new UserDetailDto();

                    // Map từ UserDto sang EditProfileViewModel
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
                        DesiredPosition = response.Data.UserDetail?.DesiredPosition,
                        DesiredSalary = response.Data.UserDetail?.DesiredSalary,
                        DesiredLocation = response.Data.UserDetail?.DesiredLocation,
                        Availability = response.Data.UserDetail?.Availability,
                        CurrentAvatar = response.Data.Avatar
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
        public async Task<IActionResult> EditProfile(EditProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState is invalid:");
                foreach (var error in ModelState)
                {
                    Console.WriteLine($"{error.Key}: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
                }
                return View(model);
            }

            try
            {
                Console.WriteLine($"Submitting profile update for user");

                // Tạo DTO để gửi đến API
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
                    Availability = model.Availability
                };

                Console.WriteLine($"DTO data: {System.Text.Json.JsonSerializer.Serialize(updateProfileDto)}");

                var response = await _apiHelper.PostAsync<ApiResponseDto>("api/WorkerApi/profile/update", updateProfileDto);

                Console.WriteLine($"API response: Success={response?.Success}, Message={response?.Message}");

                if (response?.Success == true)
                {
                    TempData["SuccessMessage"] = "Cập nhật hồ sơ thành công";
                    return RedirectToAction("Profile");
                }

                TempData["ErrorMessage"] = response?.Message ?? "Không thể cập nhật hồ sơ";
                return View(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in EditProfile: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
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
