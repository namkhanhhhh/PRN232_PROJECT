using BusinessObjects.DTOs;
using BusinessObjects.DTOs.Application;
using BusinessObjects.DTOs.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductManagementWebClient.Helpers;
using System.Security.Claims;

namespace ProductManagementWebClient.Controllers
{
    [Authorize(Roles = "Employer")]
    public class ApplicationManagementController : Controller
    {
        private readonly ApiHelper _apiHelper;

        public ApplicationManagementController(ApiHelper apiHelper)
        {
            _apiHelper = apiHelper;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? jobId = null, string status = null, string search = null, int page = 1)
        {
            try
            {
                var filter = new ApplicationFilterDto
                {
                    JobId = jobId,
                    Status = status,
                    Search = search,
                    Page = page,
                    PageSize = 10
                };

                var response = await _apiHelper.PostAsync<ApiResponse<ApplicationSummaryDto>>("api/ApplicationManagementApi/applications", filter);

                if (response?.Success == true && response.Data != null)
                {
                    ViewBag.SelectedJobId = jobId;
                    ViewBag.SelectedStatus = status;
                    ViewBag.SearchTerm = search;
                    ViewBag.CurrentPage = page;

                    return View(response.Data);
                }

                TempData["ErrorMessage"] = response?.Message ?? "Không thể lấy danh sách đơn ứng tuyển";
                return View(new ApplicationSummaryDto());
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi tải trang";
                return View(new ApplicationSummaryDto());
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var response = await _apiHelper.GetAsync<ApiResponse<ApplicationDetailDto>>($"api/ApplicationManagementApi/applications/{id}");

                if (response?.Success == true && response.Data != null)
                {
                    return View(response.Data);
                }

                TempData["ErrorMessage"] = response?.Message ?? "Không tìm thấy đơn ứng tuyển";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi lấy chi tiết đơn ứng tuyển";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int applicationId, string status, string note)
        {
            try
            {
                var request = new UpdateApplicationStatusDto
                {
                    ApplicationId = applicationId,
                    Status = status,
                    Note = note
                };

                var response = await _apiHelper.PostAsync<ApiResponse>("api/ApplicationManagementApi/applications/update-status", request);

                if (response?.Success == true)
                {
                    TempData["SuccessMessage"] = response.Message;
                }
                else
                {
                    TempData["ErrorMessage"] = response?.Message ?? "Không thể cập nhật trạng thái";
                }

                return RedirectToAction("Details", new { id = applicationId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi cập nhật trạng thái";
                return RedirectToAction("Details", new { id = applicationId });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddNote(int applicationId, string note)
        {
            try
            {
                var request = new AddApplicationNoteDto
                {
                    ApplicationId = applicationId,
                    Note = note
                };

                var response = await _apiHelper.PostAsync<ApiResponse>("api/ApplicationManagementApi/applications/add-note", request);

                if (response?.Success == true)
                {
                    TempData["SuccessMessage"] = response.Message;
                }
                else
                {
                    TempData["ErrorMessage"] = response?.Message ?? "Không thể thêm ghi chú";
                }

                return RedirectToAction("Details", new { id = applicationId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi thêm ghi chú";
                return RedirectToAction("Details", new { id = applicationId });
            }
        }

        [HttpGet]
        public async Task<IActionResult> JobStats(int id)
        {
            try
            {
                var response = await _apiHelper.GetAsync<ApiResponse<JobStatsDto>>($"api/ApplicationManagementApi/job-stats/{id}");

                if (response?.Success == true && response.Data != null)
                {
                    return View(response.Data);
                }

                TempData["ErrorMessage"] = response?.Message ?? "Không tìm thấy bài đăng";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi lấy thống kê bài đăng";
                return RedirectToAction("Index");
            }
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
        }
    }
}
