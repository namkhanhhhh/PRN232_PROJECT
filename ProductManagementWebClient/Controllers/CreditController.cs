using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ProductManagementWebClient.Helpers;
using BusinessObjects.DTOs.Credit;
using System.Text.Json;

namespace ProductManagementWebClient.Controllers
{
    [Authorize(Roles = "Employer")]
    public class CreditController : Controller
    {
        private readonly ApiHelper _apiHelper;

        public CreditController(ApiHelper apiHelper)
        {
            _apiHelper = apiHelper;
        }

        public IActionResult Deposit()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreatePayment(decimal amount)
        {
            try
            {
                Console.WriteLine($"=== CLIENT CREATE PAYMENT START ===");
                Console.WriteLine($"Amount: {amount}");

                // Validate amount
                if (amount < 10000 || amount > 50000000)
                {
                    TempData["ErrorMessage"] = "Số tiền phải từ 10,000 đến 50,000,000 VNĐ";
                    return RedirectToAction("Error");
                }

                // Lưu số tiền vào Session
                HttpContext.Session.SetString("Amount", amount.ToString());

                var request = new CreatePaymentRequestDto
                {
                    Amount = amount,
                    CancelUrl = Url.Action("Cancel", "Credit", null, Request.Scheme)!,
                    SuccessUrl = Url.Action("Success", "Credit", null, Request.Scheme)!
                };

                Console.WriteLine($"Sending request to API...");
                Console.WriteLine($"Cancel URL: {request.CancelUrl}");
                Console.WriteLine($"Success URL: {request.SuccessUrl}");

                var response = await _apiHelper.PostAsync<CreatePaymentResponseDto>("api/Credit/create-payment", request);

                Console.WriteLine($"API Response received");
                Console.WriteLine($"Response is null: {response == null}");

                if (response == null)
                {
                    Console.WriteLine("Response is null - API connection failed");
                    TempData["ErrorMessage"] = "Không thể kết nối đến server. Vui lòng thử lại sau.";
                    return RedirectToAction("Error");
                }

                Console.WriteLine($"Response Success: {response.Success}");
                Console.WriteLine($"Response Error: {response.ErrorMessage}");

                if (!response.Success)
                {
                    TempData["ErrorMessage"] = response.ErrorMessage ?? "Không thể tạo giao dịch thanh toán. Vui lòng thử lại sau.";
                    return RedirectToAction("Error");
                }

                Console.WriteLine($"Redirecting to: {response.CheckoutUrl}");
                return Redirect(response.CheckoutUrl);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in CreatePayment: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                TempData["ErrorMessage"] = $"Đã xảy ra lỗi: {ex.Message}";
                return RedirectToAction("Error");
            }
        }

        public async Task<IActionResult> Success()
        {
            try
            {
                // Lấy số tiền từ Sessi
                var amountStr = HttpContext.Session.GetString("Amount");
                string orderCode = Request.Query["orderCode"];
                string status = Request.Query["status"];
                string cancelStr = Request.Query["cancel"];

                if (!decimal.TryParse(amountStr, out var amount))
                {
                    TempData["ErrorMessage"] = "Số tiền không hợp lệ.";
                    return RedirectToAction("Error");
                }

                var callback = new PaymentCallbackDto
                {
                    OrderCode = orderCode,
                    Status = status,
                    Cancel = cancelStr,
                    Amount = amount
                };

                var response = await _apiHelper.PostAsync<object>("api/Credit/process-payment", callback);

                if (response == null)
                {
                    TempData["ErrorMessage"] = "Không thể xử lý giao dịch.";
                    return RedirectToAction("Error");
                }
                var balance = await _apiHelper.GetAsync<UserCreditDto>("api/Credit/balance");
                int credit = (int)balance.Balance;
                HttpContext.Session.SetInt32("credit", credit);
                return View("Success");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Đã xảy ra lỗi khi xử lý giao dịch: {ex.Message}";
                return RedirectToAction("Error");
            }
        }

        public IActionResult Cancel()
        {
            return View();
        }

        public IActionResult Error()
        {
            ViewBag.ErrorMessage = TempData["ErrorMessage"];
            return View();
        }

        public async Task<IActionResult> Balance()
        {
            try
            {
                var balance = await _apiHelper.GetAsync<UserCreditDto>("api/Credit/balance");
                int credit = (int)balance.Balance;
                HttpContext.Session.SetInt32("credit", credit);
                return View(balance);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Không thể tải thông tin số dư: {ex.Message}";
                return RedirectToAction("Error");
            }
        }

        public async Task<IActionResult> Transactions(int page = 1, int pageSize = 10)
        {
            try
            {
                var transactions = await _apiHelper.GetAsync<List<CreditTransactionDto>>($"api/Credit/transactions?page={page}&pageSize={pageSize}");
                ViewBag.CurrentPage = page;
                ViewBag.PageSize = pageSize;
                return View(transactions);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Không thể tải lịch sử giao dịch: {ex.Message}";
                return RedirectToAction("Error");
            }
        }
    }
}
