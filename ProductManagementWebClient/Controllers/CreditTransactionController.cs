using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ProductManagementWebClient.Helpers;
using BusinessObjects.DTOs.CreditTransaction;
using BusinessObjects.DTOs.Common;

namespace ProductManagementWebClient.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CreditTransactionController : Controller
    {
        private readonly ApiHelper _apiHelper;

        public CreditTransactionController(ApiHelper apiHelper)
        {
            _apiHelper = apiHelper;
        }

        public async Task<IActionResult> Index(
            string searchEmail = "",
            DateTime? fromDate = null,
            DateTime? toDate = null,
            decimal? minAmount = null,
            decimal? maxAmount = null,
            string transactionType = "",
            int page = 1,
            string sortBy = "CreatedAt",
            string sortDirection = "DESC")
        {
            try
            {
                var filter = new CreditTransactionFilterDto
                {
                    Email = searchEmail,
                    FromDate = fromDate,
                    ToDate = toDate,
                    MinAmount = minAmount,
                    MaxAmount = maxAmount,
                    TransactionType = transactionType,
                    Page = page,
                    PageSize = 20,
                    SortBy = sortBy,
                    SortDirection = sortDirection
                };

                var queryString = $"?Email={searchEmail}&FromDate={fromDate:yyyy-MM-dd}&ToDate={toDate:yyyy-MM-dd}" +
                                $"&MinAmount={minAmount}&MaxAmount={maxAmount}&TransactionType={transactionType}" +
                                $"&Page={page}&PageSize=20&SortBy={sortBy}&SortDirection={sortDirection}";

                var response = await _apiHelper.GetAsync<ApiResponse<List<CreditTransactionListDto>>>($"api/credit-transaction{queryString}");

                if (response?.Success == true)
                {
                    ViewBag.Transactions = response.Data;
                    ViewBag.TotalCount = response.TotalCount;
                    ViewBag.TotalPages = (int)Math.Ceiling((double)(response.TotalCount) / 20);
                    ViewBag.CurrentPage = page;

                    // Get transaction types for filter
                    var typesResponse = await _apiHelper.GetAsync<ApiResponse<List<string>>>("api/credit-transaction/transaction-types");
                    ViewBag.TransactionTypes = typesResponse?.Data ?? new List<string>();
                }
                else
                {
                    ViewBag.Transactions = new List<CreditTransactionListDto>();
                    ViewBag.TotalCount = 0;
                    ViewBag.TotalPages = 0;
                    ViewBag.CurrentPage = 1;
                    ViewBag.TransactionTypes = new List<string>();
                }

                // Set filter values for the view
                ViewBag.SearchEmail = searchEmail;
                ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
                ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
                ViewBag.MinAmount = minAmount;
                ViewBag.MaxAmount = maxAmount;
                ViewBag.TransactionType = transactionType;
                ViewBag.SortBy = sortBy;
                ViewBag.SortDirection = sortDirection;

                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading transactions: {ex.Message}";
                return View();
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var response = await _apiHelper.GetAsync<ApiResponse<CreditTransactionDetailsDto>>($"api/credit-transaction/{id}");

                if (response?.Success == true && response.Data != null)
                {
                    return View(response.Data);
                }
                else
                {
                    TempData["ErrorMessage"] = response?.Message ?? "Transaction not found";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading transaction details: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var response = await _apiHelper.GetAsync<ApiResponse<CreditTransactionDashboardDto>>("api/credit-transaction/dashboard");

                if (response?.Success == true && response.Data != null)
                {
                    return View(response.Data);
                }
                else
                {
                    TempData["ErrorMessage"] = response?.Message ?? "Error loading dashboard data";
                    return View(new CreditTransactionDashboardDto());
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading dashboard: {ex.Message}";
                return View(new CreditTransactionDashboardDto());
            }
        }

        public async Task<IActionResult> LeastPopularServices()
        {
            try
            {
                var response = await _apiHelper.GetAsync<ApiResponse<List<LeastPopularServiceDto>>>("api/credit-transaction/least-popular-services");

                if (response?.Success == true && response.Data != null)
                {
                    return View(response.Data);
                }
                else
                {
                    TempData["ErrorMessage"] = response?.Message ?? "Error loading services data";
                    return View(new List<LeastPopularServiceDto>());
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading services: {ex.Message}";
                return View(new List<LeastPopularServiceDto>());
            }
        }

        public async Task<IActionResult> ExportTransactions(
            string searchEmail = "",
            DateTime? fromDate = null,
            DateTime? toDate = null,
            decimal? minAmount = null,
            decimal? maxAmount = null,
            string transactionType = "")
        {
            try
            {
                var filter = new CreditTransactionFilterDto
                {
                    Email = searchEmail,
                    FromDate = fromDate,
                    ToDate = toDate,
                    MinAmount = minAmount,
                    MaxAmount = maxAmount,
                    TransactionType = transactionType,
                    Page = 1,
                    PageSize = 10000, // Large number to get all records
                    SortBy = "CreatedAt",
                    SortDirection = "DESC"
                };

                var queryString = $"?Email={searchEmail}&FromDate={fromDate:yyyy-MM-dd}&ToDate={toDate:yyyy-MM-dd}" +
                                $"&MinAmount={minAmount}&MaxAmount={maxAmount}&TransactionType={transactionType}" +
                                $"&Page=1&PageSize=10000&SortBy=CreatedAt&SortDirection=DESC";

                var response = await _apiHelper.GetAsync<ApiResponse<List<CreditTransactionListDto>>>($"api/credit-transaction{queryString}");

                if (response?.Success == true && response.Data != null)
                {
                    var csv = new System.Text.StringBuilder();
                    csv.AppendLine("ID,Username,Email,Amount,Transaction Type,Balance After,Description,Date");

                    foreach (var transaction in response.Data)
                    {
                        csv.AppendLine($"{transaction.Id},{transaction.Username},{transaction.Email}," +
                                     $"{transaction.Amount},{transaction.TransactionType},{transaction.BalanceAfter}," +
                                     $"\"{transaction.Description}\",{transaction.CreatedAt:yyyy-MM-dd HH:mm:ss}");
                    }

                    return File(System.Text.Encoding.UTF8.GetBytes(csv.ToString()),
                               "text/csv",
                               $"credit_transactions_{DateTime.Now:yyyyMMdd}.csv");
                }
                else
                {
                    TempData["ErrorMessage"] = "No data to export";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error exporting data: {ex.Message}";
                return RedirectToAction("Index");
            }
        }
    }
}