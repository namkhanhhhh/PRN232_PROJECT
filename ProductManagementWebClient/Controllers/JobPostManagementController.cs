using BusinessObjects.DTOs.JobPost;
using BusinessObjects.DTOs.Authen;
using BusinessObjects.DTOs.Employer;
using Microsoft.AspNetCore.Mvc;
using ProductManagementWebClient.Helpers;
using BusinessObjects.DTOs;
using BusinessObjects.DTOs.Credit;

namespace ProductManagementWebClient.Controllers
{
    public class JobPostManagementController : Controller
    {
        private readonly ApiHelper _apiHelper;

        public JobPostManagementController(ApiHelper apiHelper)
        {
            _apiHelper = apiHelper;
        }

        [HttpGet]
        public async Task<IActionResult> GetCurrentUserBalance()
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                {
                    return Json(new { success = false, message = "User not logged in" });
                }

                var balanceResponse = await _apiHelper.GetAsync<ApiResponseDto<BusinessObjects.DTOs.Credit.UserCreditDto>>($"api/EmployerApi/balance/{userId}");

                if (balanceResponse?.Success == true && balanceResponse.Data != null)
                {
                    return Json(new
                    {
                        success = true,
                        balance = balanceResponse.Data.Balance,
                        userId = userId
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        balance = 0,
                        message = balanceResponse?.Message ?? "Failed to load balance"
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    balance = 0,
                    message = ex.Message
                });
            }
        }

        public async Task<IActionResult> Index(string status = "all", string search = "", DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                {
                    return RedirectToAction("Index", "Login");
                }

                // Get user post credits
                var creditsResponse = await _apiHelper.GetAsync<ApiResponseDto<UserPostCreditsDto>>($"api/JobPostManagementApi/user-credits/{userId}");
                if (creditsResponse?.Success == true && creditsResponse.Data != null)
                {
                    ViewBag.SilverCredits = creditsResponse.Data.SilverPostsAvailable;
                    ViewBag.GoldCredits = creditsResponse.Data.GoldPostsAvailable;
                    ViewBag.DiamondCredits = creditsResponse.Data.DiamondPostsAvailable;
                }
                else
                {
                    ViewBag.SilverCredits = 0;
                    ViewBag.GoldCredits = 0;
                    ViewBag.DiamondCredits = 0;
                }

                var response = await _apiHelper.GetAsync<ApiResponseDto<List<JobPostListDto>>>($"api/JobPostManagementApi/employer/{userId}");

                if (response != null && response.Success && response.Data != null)
                {
                    var jobPosts = response.Data;

                    // Apply filters
                    if (!string.IsNullOrEmpty(search))
                    {
                        jobPosts = jobPosts.Where(jp => jp.Title.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
                    }

                    if (fromDate.HasValue)
                    {
                        jobPosts = jobPosts.Where(jp => jp.CreatedAt >= fromDate.Value).ToList();
                    }

                    if (toDate.HasValue)
                    {
                        jobPosts = jobPosts.Where(jp => jp.CreatedAt <= toDate.Value.AddDays(1)).ToList();
                    }

                    // Filter by status
                    var today = DateOnly.FromDateTime(DateTime.Today);
                    switch (status.ToLower())
                    {
                        case "active":
                            jobPosts = jobPosts.Where(jp => jp.Status?.ToLower() == "active" &&
                                                           (jp.Deadline == null || jp.Deadline >= today)).ToList();
                            break;
                        case "inactive":
                            jobPosts = jobPosts.Where(jp => jp.Status?.ToLower() == "inactive").ToList();
                            break;
                        case "expired":
                            jobPosts = jobPosts.Where(jp => jp.Deadline.HasValue && jp.Deadline < today).ToList();
                            break;
                            // "all" - no additional filtering
                    }

                    // Calculate counts for filter tabs
                    var allPosts = response.Data;
                    ViewBag.AllCount = allPosts.Count;
                    ViewBag.ActiveCount = allPosts.Count(jp => jp.Status?.ToLower() == "active" &&
                                                              (jp.Deadline == null || jp.Deadline >= today));
                    ViewBag.InactiveCount = allPosts.Count(jp => jp.Status?.ToLower() == "inactive");
                    ViewBag.ExpiredCount = allPosts.Count(jp => jp.Deadline.HasValue && jp.Deadline < today);

                    // Pass filter values to view
                    ViewBag.CurrentStatus = status;
                    ViewBag.CurrentSearch = search;
                    ViewBag.CurrentFromDate = fromDate?.ToString("yyyy-MM-dd");
                    ViewBag.CurrentToDate = toDate?.ToString("yyyy-MM-dd");
                    await LoadUserBalanceForAllActions();
                    return View(jobPosts);
                }

                ViewBag.ErrorMessage = response?.Message ?? "Failed to load job posts";
                return View(new List<JobPostListDto>());
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Error loading job posts: {ex.Message}";

                return View(new List<JobPostListDto>());
            }
        }

        public async Task<IActionResult> Create()
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                {
                    return RedirectToAction("Index", "Login");
                }

                // Get user post credits
                var creditsResponse = await _apiHelper.GetAsync<ApiResponseDto<UserPostCreditsDto>>($"api/JobPostManagementApi/user-credits/{userId}");
                if (creditsResponse?.Success == true && creditsResponse.Data != null)
                {
                    ViewBag.SilverCredits = creditsResponse.Data.SilverPostsAvailable;
                    ViewBag.GoldCredits = creditsResponse.Data.GoldPostsAvailable;
                    ViewBag.DiamondCredits = creditsResponse.Data.DiamondPostsAvailable;
                }
                else
                {
                    ViewBag.SilverCredits = 0;
                    ViewBag.GoldCredits = 0;
                    ViewBag.DiamondCredits = 0;
                }
                await LoadUserBalanceForAllActions();

                return View(new JobPostCreateDto());
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Error loading form: {ex.Message}";
                return View(new JobPostCreateDto());
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(JobPostCreateDto model, IFormFile? ImageMainFile, IFormFile? Image2File, IFormFile? Image3File, IFormFile? Image4File, string? selectedCategoryIds, string? specificAddress, string? selectedProvinceName, string? selectedDistrictName, string? selectedWardName)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                {
                    return RedirectToAction("Index", "Login");
                }

                model.UserId = userId.Value;

                // Parse selected category IDs
                if (!string.IsNullOrEmpty(selectedCategoryIds))
                {
                    try
                    {
                        model.CategoryIds = selectedCategoryIds.Split(',')
                            .Where(id => !string.IsNullOrWhiteSpace(id))
                            .Select(int.Parse)
                            .ToList();
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("CategoryIds", "Invalid category selection");
                    }
                }

                // Construct Location string from specific address, selected province, district, ward
                var locationParts = new List<string>();
                if (!string.IsNullOrEmpty(specificAddress))
                {
                    locationParts.Add(specificAddress);
                }
                if (!string.IsNullOrEmpty(selectedWardName) && selectedWardName != "-- Chọn Phường/Xã --")
                {
                    locationParts.Add(selectedWardName);
                }
                if (!string.IsNullOrEmpty(selectedDistrictName) && selectedDistrictName != "-- Chọn Quận/Huyện --")
                {
                    locationParts.Add(selectedDistrictName);
                }
                if (!string.IsNullOrEmpty(selectedProvinceName) && selectedProvinceName != "-- Chọn Tỉnh/Thành phố --")
                {
                    locationParts.Add(selectedProvinceName);
                }
                model.Location = string.Join(", ", locationParts);

                // Handle image uploads
                var imageFiles = new[] {
                  new { File = ImageMainFile, PropertyName = "ImageMain" },
                  new { File = Image2File, PropertyName = "Image2" },
                  new { File = Image3File, PropertyName = "Image3" },
                  new { File = Image4File, PropertyName = "Image4" }
              };

                foreach (var imageFile in imageFiles)
                {
                    if (imageFile.File != null && imageFile.File.Length > 0)
                    {
                        var uploadResult = await UploadImageAsync(imageFile.File);
                        if (uploadResult.Success)
                        {
                            switch (imageFile.PropertyName)
                            {
                                case "ImageMain":
                                    model.ImageMain = uploadResult.FilePath;
                                    break;
                                case "Image2":
                                    model.Image2 = uploadResult.FilePath;
                                    break;
                                case "Image3":
                                    model.Image3 = uploadResult.FilePath;
                                    break;
                                case "Image4":
                                    model.Image4 = uploadResult.FilePath;
                                    break;
                            }
                        }
                        else
                        {
                            ModelState.AddModelError($"{imageFile.PropertyName}File", uploadResult.ErrorMessage);
                        }
                    }
                }

                // If no main image uploaded, set a default placeholder
                if (string.IsNullOrEmpty(model.ImageMain))
                {
                    model.ImageMain = "/images/default-job-post.jpg";
                }

                // Manual validation for required fields
                if (string.IsNullOrWhiteSpace(model.Title))
                {
                    ModelState.AddModelError("Title", "Tiêu đề là bắt buộc");
                }
                else if (model.Title.Length > 255)
                {
                    ModelState.AddModelError("Title", "Tiêu đề không được vượt quá 255 ký tự");
                }

                if (string.IsNullOrWhiteSpace(model.Description))
                {
                    ModelState.AddModelError("Description", "Mô tả công việc là bắt buộc");
                }

                if (string.IsNullOrWhiteSpace(model.Location))
                {
                    ModelState.AddModelError("Location", "Địa điểm là bắt buộc");
                }
                else if (model.Location.Length > 255)
                {
                    ModelState.AddModelError("Location", "Địa điểm không được vượt quá 255 ký tự");
                }

                if (string.IsNullOrWhiteSpace(model.PostType))
                {
                    ModelState.AddModelError("PostType", "Vui lòng chọn loại tin");
                }

                // Validate string lengths
                if (!string.IsNullOrEmpty(model.Requirements) && model.Requirements.Length > 500)
                {
                    ModelState.AddModelError("Requirements", "Yêu cầu ứng viên không được vượt quá 500 ký tự");
                }

                if (!string.IsNullOrEmpty(model.Benefits) && model.Benefits.Length > 500)
                {
                    ModelState.AddModelError("Benefits", "Phúc lợi không được vượt quá 500 ký tự");
                }

                if (!string.IsNullOrEmpty(model.JobType) && model.JobType.Length > 255)
                {
                    ModelState.AddModelError("JobType", "Hình thức làm việc không được vượt quá 255 ký tự");
                }

                if (!string.IsNullOrEmpty(model.ExperienceLevel) && model.ExperienceLevel.Length > 255)
                {
                    ModelState.AddModelError("ExperienceLevel", "Kinh nghiệm không được vượt quá 255 ký tự");
                }

                // Validate salary range
                if (model.SalaryMin.HasValue && model.SalaryMin < 0)
                {
                    ModelState.AddModelError("SalaryMin", "Lương tối thiểu phải là số dương");
                }

                if (model.SalaryMax.HasValue && model.SalaryMax < 0)
                {
                    ModelState.AddModelError("SalaryMax", "Lương tối đa phải là số dương");
                }

                if (model.SalaryMin.HasValue && model.SalaryMax.HasValue && model.SalaryMin > model.SalaryMax)
                {
                    ModelState.AddModelError("SalaryMax", "Lương tối đa phải lớn hơn hoặc bằng lương tối thiểu");
                }

                if (!ModelState.IsValid)
                {
                    // Get credits again for view
                    var creditsResponse = await _apiHelper.GetAsync<ApiResponseDto<UserPostCreditsDto>>($"api/JobPostManagementApi/user-credits/{userId}");
                    if (creditsResponse?.Success == true && creditsResponse.Data != null)
                    {
                        ViewBag.SilverCredits = creditsResponse.Data.SilverPostsAvailable;
                        ViewBag.GoldCredits = creditsResponse.Data.GoldPostsAvailable;
                        ViewBag.DiamondCredits = creditsResponse.Data.DiamondPostsAvailable;
                    }
                    else
                    {
                        ViewBag.SilverCredits = 0;
                        ViewBag.GoldCredits = 0;
                        ViewBag.DiamondCredits = 0;
                    }
                    ViewBag.ErrorMessage = "Vui lòng kiểm tra lại thông tin đã nhập";
                    return View(model);
                }

                var response = await _apiHelper.PostAsync<ApiResponseDto<bool>>("api/JobPostManagementApi", model);

                if (response?.Success == true)
                {
                    TempData["SuccessMessage"] = "Tạo bài đăng thành công!";
                    return RedirectToAction("Index");
                }

                // Handle API errors - simplified approach
                if (!string.IsNullOrEmpty(response?.Message))
                {
                    ModelState.AddModelError("", response.Message);
                }

                ViewBag.ErrorMessage = response?.Message ?? "Không thể tạo bài đăng. Vui lòng thử lại.";

                // Get credits again for view
                var creditsResponse2 = await _apiHelper.GetAsync<ApiResponseDto<UserPostCreditsDto>>($"api/JobPostManagementApi/user-credits/{userId}");
                if (creditsResponse2?.Success == true && creditsResponse2.Data != null)
                {
                    ViewBag.SilverCredits = creditsResponse2.Data.SilverPostsAvailable;
                    ViewBag.GoldCredits = creditsResponse2.Data.GoldPostsAvailable;
                    ViewBag.DiamondCredits = creditsResponse2.Data.DiamondPostsAvailable;
                }
                else
                {
                    ViewBag.SilverCredits = 0;
                    ViewBag.GoldCredits = 0;
                    ViewBag.DiamondCredits = 0;
                }
                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Lỗi hệ thống: {ex.Message}";
                ViewBag.SilverCredits = 0;
                ViewBag.GoldCredits = 0;
                ViewBag.DiamondCredits = 0;
                return View(model);
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            try
            {
                var response = await _apiHelper.GetAsync<ApiResponseDto<JobPostListDto>>($"api/JobPostManagementApi/{id}");

                if (response?.Success == true && response.Data != null)
                {
                    return View(response.Data);
                }
                await LoadUserBalanceForAllActions();
                return NotFound();
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Error loading job post: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var response = await _apiHelper.GetAsync<ApiResponseDto<JobPostListDto>>($"api/JobPostManagementApi/{id}");

                if (response?.Success == true && response.Data != null)
                {
                    // Parse the full location string to separate specific address and geographic parts
                    string fullLocation = response.Data.Location;
                    string parsedSpecificAddress = "";
                    string parsedLocationForDropdowns = "";

                    var parts = fullLocation.Split(new[] { ", " }, StringSplitOptions.None).ToList();

                    // Heuristic: Assume the last 3 parts are Ward, District, Province if they exist.
                    // Everything before that is the specific address.
                    if (parts.Count >= 3)
                    {
                        parsedLocationForDropdowns = string.Join(", ", parts.Skip(parts.Count - 3));
                        parsedSpecificAddress = string.Join(", ", parts.Take(parts.Count - 3));
                    }
                    else if (parts.Count > 0)
                    {
                        // If less than 3 parts, it's either an incomplete location or just a specific address.
                        // For simplicity, if it's not a full "Ward, District, Province" structure,
                        // assume the entire string is the specific address.
                        parsedSpecificAddress = fullLocation;
                        parsedLocationForDropdowns = ""; // No geographic parts to pre-select
                    }


                    var updateDto = new JobPostUpdateDto
                    {
                        Id = response.Data.Id,
                        Title = response.Data.Title,
                        Description = response.Data.Description,
                        Requirements = response.Data.Requirements,
                        Benefits = response.Data.Benefits,
                        Location = parsedLocationForDropdowns, // This will be used by JS to pre-select dropdowns
                        SpecificAddress = parsedSpecificAddress, // New property
                        SalaryMin = response.Data.SalaryMin,
                        SalaryMax = response.Data.SalaryMax,
                        JobType = response.Data.JobType,
                        ExperienceLevel = response.Data.ExperienceLevel,
                        Deadline = response.Data.Deadline,
                        ImageMain = response.Data.ImageMain,
                        Image2 = response.Data.Image2,
                        Image3 = response.Data.Image3,
                        Image4 = response.Data.Image4,
                        PostType = response.Data.PostType,
                        Status = response.Data.Status
                    };

                    // Get current categories for this job post
                    var currentCategoriesResponse = await _apiHelper.GetAsync<ApiResponseDto<List<int>>>($"api/JobPostManagementApi/{id}/categories");
                    if (currentCategoriesResponse?.Success == true && currentCategoriesResponse.Data != null)
                    {
                        updateDto.CategoryIds = currentCategoriesResponse.Data;
                        ViewBag.CurrentCategoryIds = string.Join(",", currentCategoriesResponse.Data);
                    }
                    else
                    {
                        updateDto.CategoryIds = new List<int>();
                        ViewBag.CurrentCategoryIds = "";
                    }

                    // Pass current location parts to the view for pre-selection
                    ViewBag.CurrentLocation = response.Data.Location; // Pass the original full string for JS parsing
                    ViewBag.CurrentSpecificAddress = parsedSpecificAddress; // Pass the parsed specific address

                    var userId = HttpContext.Session.GetInt32("UserId");
                    await LoadUserBalanceForAllActions();
                    return View(updateDto);
                }

                return NotFound();
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Error loading job post: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(JobPostUpdateDto model, IFormFile? ImageMainFile, IFormFile? Image2File, IFormFile? Image3File, IFormFile? Image4File, string? selectedCategoryIds, string? specificAddress, string? selectedProvinceName, string? selectedDistrictName, string? selectedWardName)
        {
            try
            {
                // Parse selected category IDs
                if (!string.IsNullOrEmpty(selectedCategoryIds))
                {
                    try
                    {
                        model.CategoryIds = selectedCategoryIds.Split(',')
                            .Where(id => !string.IsNullOrWhiteSpace(id))
                            .Select(int.Parse)
                            .ToList();
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("CategoryIds", "Invalid category selection");
                    }
                }

                // Construct Location string from specific address, selected province, district, ward
                var locationParts = new List<string>();
                if (!string.IsNullOrEmpty(specificAddress))
                {
                    locationParts.Add(specificAddress);
                }
                if (!string.IsNullOrEmpty(selectedWardName) && selectedWardName != "-- Chọn Phường/Xã --")
                {
                    locationParts.Add(selectedWardName);
                }
                if (!string.IsNullOrEmpty(selectedDistrictName) && selectedDistrictName != "-- Chọn Quận/Huyện --")
                {
                    locationParts.Add(selectedDistrictName);
                }
                if (!string.IsNullOrEmpty(selectedProvinceName) && selectedProvinceName != "-- Chọn Tỉnh/Thành phố --")
                {
                    locationParts.Add(selectedProvinceName);
                }
                model.Location = string.Join(", ", locationParts);

                // Get current job post to preserve existing images if no new images are uploaded
                var currentJobPostResponse = await _apiHelper.GetAsync<ApiResponseDto<JobPostListDto>>($"api/JobPostManagementApi/{model.Id}");

                if (currentJobPostResponse?.Success == true && currentJobPostResponse.Data != null)
                {
                    // Preserve existing images if no new ones uploaded
                    if (ImageMainFile == null)
                        model.ImageMain = currentJobPostResponse.Data.ImageMain;
                    if (Image2File == null)
                        model.Image2 = currentJobPostResponse.Data.Image2;
                    if (Image3File == null)
                        model.Image3 = currentJobPostResponse.Data.Image3;
                    if (Image4File == null)
                        model.Image4 = currentJobPostResponse.Data.Image4;
                }

                // Handle image uploads
                var imageFiles = new[] {
                  new { File = ImageMainFile, PropertyName = "ImageMain", CurrentPath = model.ImageMain },
                  new { File = Image2File, PropertyName = "Image2", CurrentPath = model.Image2 },
                  new { File = Image3File, PropertyName = "Image3", CurrentPath = model.Image3 },
                  new { File = Image4File, PropertyName = "Image4", CurrentPath = model.Image4 }
              };

                foreach (var imageFile in imageFiles)
                {
                    if (imageFile.File != null && imageFile.File.Length > 0)
                    {
                        var uploadResult = await UploadImageAsync(imageFile.File);
                        if (uploadResult.Success)
                        {
                            // Delete old image if it exists and is not default
                            if (!string.IsNullOrEmpty(imageFile.CurrentPath) &&
                                imageFile.CurrentPath.StartsWith("/Uploads/") &&
                                !imageFile.CurrentPath.Contains("default"))
                            {
                                var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", imageFile.CurrentPath.TrimStart('/'));
                                if (System.IO.File.Exists(oldImagePath))
                                {
                                    System.IO.File.Delete(oldImagePath);
                                }
                            }

                            // Set new image path
                            switch (imageFile.PropertyName)
                            {
                                case "ImageMain":
                                    model.ImageMain = uploadResult.FilePath;
                                    break;
                                case "Image2":
                                    model.Image2 = uploadResult.FilePath;
                                    break;
                                case "Image3":
                                    model.Image3 = uploadResult.FilePath;
                                    break;
                                case "Image4":
                                    model.Image4 = uploadResult.FilePath;
                                    break;
                            }
                        }
                        else
                        {
                            ViewBag.ErrorMessage = uploadResult.ErrorMessage;
                            return View(model);
                        }
                    }
                }

                // Manual validation for required fields (re-add for edit)
                if (string.IsNullOrWhiteSpace(model.Title))
                {
                    ModelState.AddModelError("Title", "Tiêu đề là bắt buộc");
                }
                else if (model.Title.Length > 255)
                {
                    ModelState.AddModelError("Title", "Tiêu đề không được vượt quá 255 ký tự");
                }

                if (string.IsNullOrWhiteSpace(model.Description))
                {
                    ModelState.AddModelError("Description", "Mô tả công việc là bắt buộc");
                }

                if (string.IsNullOrWhiteSpace(model.Location))
                {
                    ModelState.AddModelError("Location", "Địa điểm là bắt buộc");
                }
                else if (model.Location.Length > 255)
                {
                    ModelState.AddModelError("Location", "Địa điểm không được vượt quá 255 ký tự");
                }

                if (!ModelState.IsValid)
                {
                    ViewBag.ErrorMessage = "Vui lòng kiểm tra lại thông tin đã nhập";
                    // Re-populate current categories for view if validation fails
                    ViewBag.CurrentCategoryIds = string.Join(",", model.CategoryIds ?? new List<int>());
                    // Re-parse location for view if validation fails
                    string fullLocation = model.Location;
                    string parsedSpecificAddress = "";
                    string parsedLocationForDropdowns = "";
                    var parts = fullLocation.Split(new[] { ", " }, StringSplitOptions.None).ToList();
                    if (parts.Count >= 3)
                    {
                        parsedLocationForDropdowns = string.Join(", ", parts.Skip(parts.Count - 3));
                        parsedSpecificAddress = string.Join(", ", parts.Take(parts.Count - 3));
                    }
                    else if (parts.Count > 0)
                    {
                        parsedSpecificAddress = fullLocation;
                        parsedLocationForDropdowns = "";
                    }
                    ViewBag.CurrentLocation = parsedLocationForDropdowns; // This will be used by JS to pre-select dropdowns
                    ViewBag.CurrentSpecificAddress = parsedSpecificAddress; // New property
                    return View(model);
                }

                // Call API to update
                var response = await _apiHelper.PutAsync<ApiResponseDto<bool>>($"api/JobPostManagementApi/{model.Id}", model);

                if (response?.Success == true)
                {
                    TempData["SuccessMessage"] = "Cập nhật bài đăng thành công!";
                    return RedirectToAction("Index");
                }

                ViewBag.ErrorMessage = response?.Message ?? "Không thể cập nhật bài đăng. Vui lòng thử lại.";
                // Re-populate current categories for view if API call fails
                ViewBag.CurrentCategoryIds = string.Join(",", model.CategoryIds ?? new List<int>());
                // Re-parse location for view if API call fails
                string fullLocationAfterFail = model.Location;
                string parsedSpecificAddressAfterFail = "";
                string parsedLocationForDropdownsAfterFail = "";
                var partsAfterFail = fullLocationAfterFail.Split(new[] { ", " }, StringSplitOptions.None).ToList();
                if (partsAfterFail.Count >= 3)
                {
                    parsedLocationForDropdownsAfterFail = string.Join(", ", partsAfterFail.Skip(partsAfterFail.Count - 3));
                    parsedSpecificAddressAfterFail = string.Join(", ", partsAfterFail.Take(partsAfterFail.Count - 3));
                }
                else if (partsAfterFail.Count > 0)
                {
                    parsedSpecificAddressAfterFail = fullLocationAfterFail;
                    parsedLocationForDropdownsAfterFail = "";
                }
                ViewBag.CurrentLocation = parsedLocationForDropdownsAfterFail; // This will be used by JS to pre-select dropdowns
                ViewBag.CurrentSpecificAddress = parsedSpecificAddressAfterFail; // New property
                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Lỗi hệ thống: {ex.Message}";
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var response = await _apiHelper.DeleteAsync($"api/JobPostManagementApi/{id}");

                TempData["SuccessMessage"] = "Job post deactivated successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deactivating job post: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            try
            {
                var response = await _apiHelper.PostAsync<ApiResponseDto<bool>>($"api/JobPostManagementApi/{id}/toggle-status", null);

                if (response?.Success == true)
                {
                    TempData["SuccessMessage"] = "Cập nhật trạng thái bài đăng thành công!";
                }
                else
                {
                    TempData["ErrorMessage"] = response?.Message ?? "Không thể cập nhật trạng thái bài đăng";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi cập nhật trạng thái: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Activate(int id)
        {
            try
            {
                var response = await _apiHelper.PostAsync<ApiResponseDto<bool>>($"api/JobPostManagementApi/{id}/activate", null);

                if (response?.Success == true)
                {
                    TempData["SuccessMessage"] = "Kích hoạt bài đăng thành công!";
                }
                else
                {
                    TempData["ErrorMessage"] = response?.Message ?? "Không thể kích hoạt bài đăng";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi kích hoạt bài đăng: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Deactivate(int id)
        {
            try
            {
                var response = await _apiHelper.PostAsync<ApiResponseDto<bool>>($"api/JobPostManagementApi/{id}/deactivate", null);

                if (response?.Success == true)
                {
                    TempData["SuccessMessage"] = "Ẩn bài đăng thành công!";
                }
                else
                {
                    TempData["ErrorMessage"] = response?.Message ?? "Không thể ẩn bài đăng";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi ẩn bài đăng: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetJobCategories()
        {
            try
            {
                var response = await _apiHelper.GetAsync<ApiResponseDto<List<JobCategoryHierarchyDto>>>("api/JobPostManagementApi/categories");
                if (response?.Success == true && response.Data != null)
                {
                    return Json(response.Data);
                }
                return Json(new List<JobCategoryHierarchyDto>());
            }
            catch (Exception ex)
            {
                return Json(new List<JobCategoryHierarchyDto>());
            }
        }

        private async Task<(bool Success, string FilePath, string ErrorMessage)> UploadImageAsync(IFormFile file)
        {
            try
            {
                // Validate file size (max 5MB)
                if (file.Length > 5 * 1024 * 1024)
                {
                    return (false, "", "Kích thước file không được vượt quá 5MB.");
                }

                // Validate file type
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(extension))
                {
                    return (false, "", "Chỉ chấp nhận file JPG, PNG, hoặc GIF.");
                }

                // Create uploads directory if it doesn't exist
                var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads", "jobposts");
                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                }

                // Generate unique file name
                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                var filePath = Path.Combine(uploadsPath, fileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return (true, $"/Uploads/jobposts/{fileName}", "");
            }
            catch (Exception ex)
            {
                return (false, "", $"Lỗi khi tải ảnh lên: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> TestBalance()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Json(new { error = "User not logged in" });
            }

            try
            {
                var balanceResponse = await _apiHelper.GetAsync<ApiResponseDto<BusinessObjects.DTOs.Credit.UserCreditDto>>($"api/EmployerApi/balance/{userId}");
                return Json(new
                {
                    success = balanceResponse?.Success,
                    balance = balanceResponse?.Data?.Balance,
                    message = balanceResponse?.Message,
                    userId = userId
                });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        private async Task LoadUserBalanceForAllActions()
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                var credit = HttpContext.Session.GetInt32("credit");
            }
            catch
            {
                ViewBag.UserBalance = 0;
                TempData["UserBalance"] = 0;
            }
        }
    }

    public class JobCategoryHierarchyDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? ParentId { get; set; }
        public List<JobCategoryHierarchyDto> Children { get; set; } = new List<JobCategoryHierarchyDto>();
    }
}
