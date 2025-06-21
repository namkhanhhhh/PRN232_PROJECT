using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BusinessObjects.DTOs.CreditTransaction;
using BusinessObjects.DTOs.Common;
using Repository.Interfaces;
using System.Security.Claims;

namespace Sjob_API.Controllers
{
    [ApiController]
    [Route("api/credit-transaction")]
    [Authorize(Roles = "Admin")]
    public class CreditTransactionApiController : ControllerBase
    {
        private readonly ICreditTransactionRepository _creditTransactionRepository;

        public CreditTransactionApiController(ICreditTransactionRepository creditTransactionRepository)
        {
            _creditTransactionRepository = creditTransactionRepository;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<CreditTransactionListDto>>>> GetTransactions([FromQuery] CreditTransactionFilterDto filter)
        {
            try
            {
                var transactions = await _creditTransactionRepository.GetFilteredTransactionsAsync(filter);
                var totalCount = await _creditTransactionRepository.GetFilteredTransactionsCountAsync(filter);

                var transactionDtos = transactions.Select(t => new CreditTransactionListDto
                {
                    Id = t.Id,
                    UserId = t.UserId,
                    Username = t.User.Username,
                    Email = t.User.Email,
                    Amount = t.Amount,
                    TransactionType = t.TransactionType ?? "Unknown",
                    ReferenceId = t.ReferenceId,
                    ReferenceType = t.ReferenceType,
                    BalanceAfter = t.BalanceAfter,
                    Description = t.Description,
                    CreatedAt = t.CreatedAt ?? DateTime.Now
                }).ToList();

                return Ok(new ApiResponse<List<CreditTransactionListDto>>
                {
                    Success = true,
                    Data = transactionDtos,
                    Message = "Transactions retrieved successfully",
                    TotalCount = totalCount
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<List<CreditTransactionListDto>>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<CreditTransactionDetailsDto>>> GetTransactionDetails(int id)
        {
            try
            {
                var transaction = await _creditTransactionRepository.GetTransactionByIdAsync(id);
                if (transaction == null)
                {
                    return NotFound(new ApiResponse<CreditTransactionDetailsDto>
                    {
                        Success = false,
                        Message = "Transaction not found"
                    });
                }

                var userTransactions = await _creditTransactionRepository.GetUserTransactionsAsync(transaction.UserId);

                var userDetail = transaction.User.UserDetails?.FirstOrDefault();
                var detailsDto = new CreditTransactionDetailsDto
                {
                    Id = transaction.Id,
                    UserId = transaction.UserId,
                    Username = transaction.User.Username,
                    Email = transaction.User.Email,
                    FirstName = userDetail?.FirstName,
                    LastName = userDetail?.LastName,
                    PhoneNumber = userDetail?.PhoneNumber,
                    Amount = transaction.Amount,
                    TransactionType = transaction.TransactionType ?? "Unknown",
                    ReferenceId = transaction.ReferenceId,
                    ReferenceType = transaction.ReferenceType,
                    BalanceAfter = transaction.BalanceAfter,
                    Description = transaction.Description,
                    CreatedAt = transaction.CreatedAt ?? DateTime.Now,
                    UserTransactions = userTransactions.Select(t => new CreditTransactionListDto
                    {
                        Id = t.Id,
                        UserId = t.UserId,
                        Username = t.User?.Username ?? "",
                        Email = t.User?.Email ?? "",
                        Amount = t.Amount,
                        TransactionType = t.TransactionType ?? "Unknown",
                        ReferenceId = t.ReferenceId,
                        ReferenceType = t.ReferenceType,
                        BalanceAfter = t.BalanceAfter,
                        Description = t.Description,
                        CreatedAt = t.CreatedAt ?? DateTime.Now
                    }).ToList(),
                    CurrentBalance = transaction.BalanceAfter ?? 0
                };

                return Ok(new ApiResponse<CreditTransactionDetailsDto>
                {
                    Success = true,
                    Data = detailsDto,
                    Message = "Transaction details retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<CreditTransactionDetailsDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpGet("dashboard")]
        public async Task<ActionResult<ApiResponse<CreditTransactionDashboardDto>>> GetDashboard()
        {
            try
            {
                var dashboardData = await _creditTransactionRepository.GetDashboardDataAsync();

                return Ok(new ApiResponse<CreditTransactionDashboardDto>
                {
                    Success = true,
                    Data = dashboardData,
                    Message = "Dashboard data retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<CreditTransactionDashboardDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpGet("least-popular-services")]
        public async Task<ActionResult<ApiResponse<List<LeastPopularServiceDto>>>> GetLeastPopularServices()
        {
            try
            {
                var services = await _creditTransactionRepository.GetLeastPopularServicesAsync();

                return Ok(new ApiResponse<List<LeastPopularServiceDto>>
                {
                    Success = true,
                    Data = services,
                    Message = "Least popular services retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<List<LeastPopularServiceDto>>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpGet("transaction-types")]
        public async Task<ActionResult<ApiResponse<List<string>>>> GetTransactionTypes()
        {
            try
            {
                var types = await _creditTransactionRepository.GetTransactionTypesAsync();

                return Ok(new ApiResponse<List<string>>
                {
                    Success = true,
                    Data = types,
                    Message = "Transaction types retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<List<string>>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }
    }
}