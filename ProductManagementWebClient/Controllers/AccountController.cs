using BusinessObjects.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ProductManagementWebClient.Helpers;
using System.Security.Claims;
using OfficeOpenXml;
using BusinessObjects.DTOs.Authen;

namespace ProductManagementWebClient.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly ApiHelper _apiHelper;

        public AccountController(ApiHelper apiHelper)
        {
            _apiHelper = apiHelper;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index(string searchTerm = "", int page = 1, string sortBy = "Username", string sortOrder = "asc")
        {
            try
            {
                var request = new PaginatedRequestDto
                {
                    SearchTerm = searchTerm,
                    Page = page,
                    PageSize = 10,
                    SortBy = sortBy,
                    SortOrder = sortOrder
                };

                var response = await _apiHelper.PostAsync<PaginatedResponseDto<UserDto>>("api/AccountApi/users", request);

                if (response?.Success == true)
                {
                    ViewData["CurrentSearch"] = searchTerm;
                    ViewData["CurrentPage"] = page;
                    ViewData["CurrentSortBy"] = sortBy;
                    ViewData["CurrentSortOrder"] = sortOrder;

                    return View(response);
                }

                TempData["ErrorMessage"] = response?.Message ?? "Không thể lấy danh sách người dùng";
                return View(new PaginatedResponseDto<UserDto> { Success = false, Items = new List<UserDto>() });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi lấy danh sách người dùng";
                return View(new PaginatedResponseDto<UserDto> { Success = false, Items = new List<UserDto>() });
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();

                // Only admin can view other users' details
                if (id != currentUserId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                var response = await _apiHelper.GetAsync<ApiResponseDto<UserDto>>($"api/AccountApi/{id}");

                if (response?.Success == true && response.Data != null)
                {
                    return View(response.Data);
                }

                TempData["ErrorMessage"] = response?.Message ?? "Không tìm thấy thông tin người dùng";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi lấy thông tin người dùng";
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> Profile()
        {
            var currentUserId = GetCurrentUserId();
            return RedirectToAction("Details", new { id = currentUserId });
        }

        [HttpGet]
        public async Task<IActionResult> EditProfile(int? id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var targetUserId = id ?? currentUserId;

                // Only admin can edit other users' profiles
                if (targetUserId != currentUserId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                var response = await _apiHelper.GetAsync<ApiResponseDto<UserDto>>($"api/AccountApi/{targetUserId}");

                if (response?.Success == true && response.Data != null)
                {
                    return View(response.Data);
                }

                TempData["ErrorMessage"] = response?.Message ?? "Không tìm thấy thông tin người dùng";
                return RedirectToAction("Profile");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi lấy thông tin người dùng";
                return RedirectToAction("Profile");
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditProfile(UserDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var currentUserId = GetCurrentUserId();

                // Only admin can edit other users' profiles
                if (model.Id != currentUserId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                var response = await _apiHelper.PutAsync<ApiResponseDto>("api/AccountApi/profile", model);

                if (response?.Success == true)
                {
                    TempData["SuccessMessage"] = response.Message;
                    return RedirectToAction("Details", new { id = model.Id });
                }

                ModelState.AddModelError("", response?.Message ?? "Không thể cập nhật thông tin cá nhân");
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Đã xảy ra lỗi khi cập nhật thông tin cá nhân");
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var response = await _apiHelper.PostAsync<ApiResponseDto>("api/AccountApi/change-password", model);

                if (response?.Success == true)
                {
                    TempData["SuccessMessage"] = response.Message;
                    return RedirectToAction("Profile");
                }

                ModelState.AddModelError("", response?.Message ?? "Không thể đổi mật khẩu");
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Đã xảy ra lỗi khi đổi mật khẩu");
                return View(model);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateAccount()
        {
            try
            {
                var rolesResponse = await _apiHelper.GetAsync<ApiResponseDto<List<BusinessObjects.Models.Role>>>("api/AccountApi/roles");

                if (rolesResponse?.Success == true && rolesResponse.Data != null)
                {
                    ViewBag.Roles = new SelectList(rolesResponse.Data, "Id", "Name");
                }
                else
                {
                    ViewBag.Roles = new SelectList(new List<BusinessObjects.Models.Role>(), "Id", "Name");
                }

                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Roles = new SelectList(new List<BusinessObjects.Models.Role>(), "Id", "Name");
                return View();
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateAccount(CreateAccountDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    await LoadRoles();
                    return View(model);
                }

                var response = await _apiHelper.PostAsync<ApiResponseDto>("api/AccountApi/create", model);

                if (response?.Success == true)
                {
                    TempData["SuccessMessage"] = response.Message;
                    return RedirectToAction("Index");
                }

                ModelState.AddModelError("", response?.Message ?? "Không thể tạo tài khoản");
                await LoadRoles();
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Đã xảy ra lỗi khi tạo tài khoản");
                await LoadRoles();
                return View(model);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            try
            {
                var response = await _apiHelper.PostAsync<ApiResponseDto>($"api/AccountApi/toggle-status/{id}", new { });

                if (response?.Success == true)
                {
                    TempData["SuccessMessage"] = response.Message;
                }
                else
                {
                    TempData["ErrorMessage"] = response?.Message ?? "Không thể thay đổi trạng thái người dùng";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi thay đổi trạng thái người dùng";
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ExportUsers(string searchTerm = "", string sortBy = "Username", string sortOrder = "asc")
        {
            try
            {
                var request = new PaginatedRequestDto
                {
                    SearchTerm = searchTerm,
                    Page = 1,
                    PageSize = int.MaxValue, // Get all users for export
                    SortBy = sortBy,
                    SortOrder = sortOrder
                };

                var response = await _apiHelper.PostAsync<PaginatedResponseDto<UserDto>>("api/AccountApi/users", request);

                if (response?.Success == true && response.Items?.Any() == true)
                {
                    using (var package = new OfficeOpenXml.ExcelPackage())
                    {
                        var worksheet = package.Workbook.Worksheets.Add("Danh sách người dùng");

                        // Headers
                        worksheet.Cells[1, 1].Value = "STT";
                        worksheet.Cells[1, 2].Value = "Tên đăng nhập";
                        worksheet.Cells[1, 3].Value = "Email";
                        worksheet.Cells[1, 4].Value = "Họ";
                        worksheet.Cells[1, 5].Value = "Tên";
                        worksheet.Cells[1, 6].Value = "Số điện thoại";
                        worksheet.Cells[1, 7].Value = "Địa chỉ";
                        worksheet.Cells[1, 8].Value = "Vai trò";
                        worksheet.Cells[1, 9].Value = "Trạng thái";
                        worksheet.Cells[1, 10].Value = "Ngày tạo";

                        // Style headers
                        using (var range = worksheet.Cells[1, 1, 1, 10])
                        {
                            range.Style.Font.Bold = true;
                            range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                            range.Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                        }

                        // Data
                        int row = 2;
                        int stt = 1;
                        foreach (var user in response.Items)
                        {
                            worksheet.Cells[row, 1].Value = stt++;
                            worksheet.Cells[row, 2].Value = user.Username;
                            worksheet.Cells[row, 3].Value = user.Email;
                            worksheet.Cells[row, 4].Value = user.FirstName;
                            worksheet.Cells[row, 5].Value = user.LastName;
                            worksheet.Cells[row, 6].Value = user.PhoneNumber;
                            worksheet.Cells[row, 7].Value = user.Address;
                            worksheet.Cells[row, 8].Value = user.RoleName;
                            worksheet.Cells[row, 9].Value = user.Status ? "Hoạt động" : "Vô hiệu hóa";
                            worksheet.Cells[row, 10].Value = user.CreatedAt?.ToString("dd/MM/yyyy");
                            row++;
                        }

                        // Auto-fit columns
                        worksheet.Cells.AutoFitColumns();

                        // Add borders to all data
                        using (var range = worksheet.Cells[1, 1, row - 1, 10])
                        {
                            range.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                            range.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                            range.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                            range.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        }

                        var fileName = $"DanhSachNguoiDung_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                        var fileBytes = package.GetAsByteArray();

                        return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                    }
                }

                TempData["ErrorMessage"] = "Không có dữ liệu để xuất";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi xuất dữ liệu";
                return RedirectToAction("Index");
            }
        }

        private async Task LoadRoles()
        {
            try
            {
                var rolesResponse = await _apiHelper.GetAsync<ApiResponseDto<List<BusinessObjects.Models.Role>>>("api/AccountApi/roles");

                if (rolesResponse?.Success == true && rolesResponse.Data != null)
                {
                    ViewBag.Roles = new SelectList(rolesResponse.Data, "Id", "Name");
                }
                else
                {
                    ViewBag.Roles = new SelectList(new List<BusinessObjects.Models.Role>(), "Id", "Name");
                }
            }
            catch
            {
                ViewBag.Roles = new SelectList(new List<BusinessObjects.Models.Role>(), "Id", "Name");
            }
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
        }
    }
}
