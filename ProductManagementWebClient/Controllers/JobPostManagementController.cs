using BusinessObjects.DTOs.JobPost;
using BusinessObjects.DTOs.Authen;
using Microsoft.AspNetCore.Mvc;
using ProductManagementWebClient.Helpers;
using BusinessObjects.DTOs;

namespace ProductManagementWebClient.Controllers
{
    public class JobPostManagementController : Controller
    {
        private readonly ApiHelper _apiHelper;

        public JobPostManagementController(ApiHelper apiHelper)
        {
            _apiHelper = apiHelper;
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

                var response = await _apiHelper.GetAsync<ApiResponseDto<List<JobPostListDto>>>($"api/JobPostManagementApi/employer/{userId}");

                if (response != null && response.Success && response.Data != null)
                {
                    var jobPosts = response.Data;

                    // Apply status filter including expired
                    if (status == "expired")
                    {
                        jobPosts = jobPosts.Where(jp => jp.Deadline.HasValue && jp.Deadline.Value < DateOnly.FromDateTime(DateTime.Now)).ToList();
                    }
                    else if (status != "all")
                    {
                        jobPosts = jobPosts.Where(jp => jp.Status?.ToLower() == status.ToLower()).ToList();
                    }

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

                    // Set filter values for view
                    ViewBag.CurrentStatus = status;
                    ViewBag.CurrentSearch = search;
                    ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
                    ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");

                    // Count posts by status
                    ViewBag.AllCount = response.Data.Count;
                    ViewBag.ActiveCount = response.Data.Count(jp => jp.Status?.ToLower() == "active");
                    ViewBag.InactiveCount = response.Data.Count(jp => jp.Status?.ToLower() == "inactive");
                    ViewBag.ExpiredCount = response.Data.Count(jp => jp.Deadline.HasValue && jp.Deadline.Value < DateOnly.FromDateTime(DateTime.Now));

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
                return View(new JobPostCreateDto());
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Error loading form: {ex.Message}";
                return View(new JobPostCreateDto());
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(JobPostCreateDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                {
                    return RedirectToAction("Index", "Login");
                }

                model.UserId = userId.Value;

                var response = await _apiHelper.PostAsync<ApiResponseDto<bool>>("api/JobPostManagementApi", model);

                if (response?.Success == true)
                {
                    TempData["SuccessMessage"] = "Job post created successfully!";
                    return RedirectToAction("Index");
                }

                ViewBag.ErrorMessage = response?.Message ?? "Failed to create job post";
                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Error creating job post: {ex.Message}";
                return View(model);
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var response = await _apiHelper.GetAsync<ApiResponseDto<JobPostListDto>>($"api/JobPostManagementApi/{id}");

                if (response?.Success == true && response.Data != null)
                {
                    return View(response.Data);
                }

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
                    var updateDto = new JobPostUpdateDto
                    {
                        Id = response.Data.Id,
                        Title = response.Data.Title,
                        Description = response.Data.Description,
                        Requirements = response.Data.Requirements,
                        Benefits = response.Data.Benefits,
                        Location = response.Data.Location,
                        SalaryMin = response.Data.SalaryMin,
                        SalaryMax = response.Data.SalaryMax,
                        JobType = response.Data.JobType,
                        ExperienceLevel = response.Data.ExperienceLevel,
                        Deadline = response.Data.Deadline,
                        ImageMain = response.Data.ImageMain,
                        PostType = response.Data.PostType,
                        Status = response.Data.Status
                    };

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
        public async Task<IActionResult> Edit(JobPostUpdateDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var response = await _apiHelper.PutAsync<ApiResponseDto<bool>>($"api/JobPostManagementApi/{model.Id}", model);

                if (response?.Success == true)
                {
                    TempData["SuccessMessage"] = "Job post updated successfully!";
                    return RedirectToAction("Index");
                }

                ViewBag.ErrorMessage = response?.Message ?? "Failed to update job post";
                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Error updating job post: {ex.Message}";
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            try
            {
                var response = await _apiHelper.DeleteAsync($"api/JobPostManagementApi/{id}");

                TempData["SuccessMessage"] = "Job post status updated successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error updating job post status: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // API endpoint for job categories (called from JavaScript)
        [HttpGet]
        public async Task<IActionResult> GetJobCategories()
        {
            try
            {
                var response = await _apiHelper.GetAsync<ApiResponseDto<List<JobCategoryDto>>>("api/JobPostManagementApi/categories");
                if (response?.Success == true && response.Data != null)
                {
                    return Json(response.Data);
                }
                return Json(new List<JobCategoryDto>());
            }
            catch (Exception ex)
            {
                return Json(new List<JobCategoryDto>());
            }
        }
    }

    public class JobCategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
