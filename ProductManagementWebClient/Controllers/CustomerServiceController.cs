using BusinessObjects.DTOs.Common;
using BusinessObjects.DTOs.CustomerService;
using BusinessObjects.DTOs.Credit;
using BusinessObjects.DTOs;
using Microsoft.AspNetCore.Mvc;
using ProductManagementWebClient.Helpers;

namespace ProductManagementWebClient.Controllers
{
    public class CustomerServiceController : Controller
    {
        private readonly ApiHelper _apiHelper;

        public CustomerServiceController(ApiHelper apiHelper)
        {
            _apiHelper = apiHelper;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var response = await _apiHelper.GetAsync<ApiResponse<CustomerServiceIndexDto>>("api/CustomerServiceApi/index");

                if (response?.Success == true && response.Data != null)
                {
                    return View(response.Data);
                }
                else
                {
                    TempData["ErrorMessage"] = response?.Message ?? "Failed to load customer service data";
                    return View(new CustomerServiceIndexDto());
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading customer service data: {ex.Message}";
                return View(new CustomerServiceIndexDto());
            }
        }

        [HttpGet]
        public async Task<IActionResult> Buy(int id, string type)
        {
            try
            {
                var response = await _apiHelper.GetAsync<ApiResponse<BuyViewDto>>($"api/CustomerServiceApi/buy?id={id}&type={type}");

                if (response?.Success == true && response.Data != null)
                {
                    return View(response.Data);
                }
                else
                {
                    TempData["ErrorMessage"] = response?.Message ?? "Item not found";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading buy page: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Buy(int id, string type, int quantity = 1)
        {
            try
            {
                var request = new PurchaseRequestDto
                {
                    Id = id,
                    Type = type,
                    Quantity = quantity
                };

                var response = await _apiHelper.PostAsync<ApiResponse>("api/CustomerServiceApi/purchase", request);

                if (response?.Success == true)
                {
                    // Update session balance after successful purchase
                    await UpdateSessionBalance();

                    TempData["SuccessMessage"] = "Mua dịch vụ thành công!";
                    return RedirectToAction("UserPackages");
                }
                else
                {
                    TempData["ErrorMessage"] = response?.Message ?? "Mua dịch vụ thất bại. Vui lòng kiểm tra số dư.";
                    return RedirectToAction("Buy", new { id = id, type = type });
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi mua dịch vụ: {ex.Message}";
                return RedirectToAction("Buy", new { id = id, type = type });
            }
        }

        public async Task<IActionResult> UserPackages()
        {
            try
            {
                var response = await _apiHelper.GetAsync<ApiResponse<UserPackagesDto>>("api/CustomerServiceApi/user-packages");

                if (response?.Success == true && response.Data != null)
                {
                    return View(response.Data);
                }
                else
                {
                    TempData["ErrorMessage"] = response?.Message ?? "Failed to load user packages";
                    return View(new UserPackagesDto());
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading user packages: {ex.Message}";
                return View(new UserPackagesDto());
            }
        }

        [HttpPost]
        public async Task<IActionResult> SyncPurchaseHistory()
        {
            try
            {
                var response = await _apiHelper.PostAsync<ApiResponse>("api/CustomerServiceApi/sync-purchase-history", new { });

                if (response?.Success == true)
                {
                    TempData["SuccessMessage"] = response.Message;
                }
                else
                {
                    TempData["ErrorMessage"] = response?.Message ?? "Sync failed";
                }

                return RedirectToAction("UserPackages");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error syncing purchase history: {ex.Message}";
                return RedirectToAction("UserPackages");
            }
        }

        // API endpoint to get current balance
        [HttpGet]
        public async Task<IActionResult> GetCurrentBalance()
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                {
                    return Json(new { success = false, message = "User not logged in" });
                }

                var balanceResponse = await _apiHelper.GetAsync<ApiResponseDto<UserCreditDto>>($"api/EmployerApi/balance/{userId}");

                if (balanceResponse?.Success == true && balanceResponse.Data != null)
                {
                    // Update session with current balance
                    HttpContext.Session.SetInt32("credit", (int)balanceResponse.Data.Balance);

                    return Json(new
                    {
                        success = true,
                        balance = balanceResponse.Data.Balance,
                        formattedBalance = ((decimal)balanceResponse.Data.Balance).ToString("N0", new System.Globalization.CultureInfo("vi-VN"))
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

        private async Task UpdateSessionBalance()
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId != null)
                {
                    var balanceResponse = await _apiHelper.GetAsync<ApiResponseDto<UserCreditDto>>($"api/EmployerApi/balance/{userId}");
                    if (balanceResponse?.Success == true && balanceResponse.Data != null)
                    {
                        HttpContext.Session.SetInt32("credit", (int)balanceResponse.Data.Balance);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error but don't throw
                Console.WriteLine($"Error updating session balance: {ex.Message}");
            }
        }
    }
}
