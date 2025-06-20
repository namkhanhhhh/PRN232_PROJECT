using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BusinessObjects.DTOs.Credit;
using BusinessObjects.Models;
using Repository.Interfaces;
using Sjob_API.Services;
using System.Security.Claims;

namespace Sjob_API.Controllers
{
    [ApiController]
    [Route("api/credit")]
    public class CreditApiController : ControllerBase
    {
        private readonly ICreditRepository _creditRepository;
        private readonly PayOSService _payOSService;

        public CreditApiController(ICreditRepository creditRepository, PayOSService payOSService)
        {
            _creditRepository = creditRepository;
            _payOSService = payOSService;
        }

        [HttpGet("balance")]
        [Authorize]
        public async Task<ActionResult<UserCreditDto>> GetUserBalance()
        {
            try
            {
                var userId = GetCurrentUserId();
                var userCredit = await _creditRepository.GetUserCreditAsync(userId);

                if (userCredit == null)
                {
                    return Ok(new UserCreditDto
                    {
                        UserId = userId,
                        Balance = 0,
                        LastUpdated = DateTime.Now
                    });
                }

                return Ok(new UserCreditDto
                {
                    Id = userCredit.Id,
                    UserId = userCredit.UserId,
                    Balance = userCredit.Balance ?? 0,
                    LastUpdated = userCredit.LastUpdated
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetUserBalance: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("create-payment")]
        [Authorize]
        public async Task<ActionResult<CreatePaymentResponseDto>> CreatePayment([FromBody] CreatePaymentRequestDto request)
        {
            try
            {
                Console.WriteLine($"=== CREATE PAYMENT START ===");
                Console.WriteLine($"Request received: Amount={request.Amount}");

                var userId = GetCurrentUserId();
                Console.WriteLine($"User ID: {userId}");

                // Validate amount
                if (request.Amount < 10000 || request.Amount > 50000000)
                {
                    Console.WriteLine($"Invalid amount: {request.Amount}");
                    return Ok(new CreatePaymentResponseDto
                    {
                        Success = false,
                        ErrorMessage = "Số tiền phải từ 10,000 đến 50,000,000 VNĐ"
                    });
                }

                Console.WriteLine($"Creating payment for user {userId}, amount: {request.Amount}");
                Console.WriteLine($"Cancel URL: {request.CancelUrl}");
                Console.WriteLine($"Success URL: {request.SuccessUrl}");

                var result = await _payOSService.CreatePaymentAsync(
                    userId,
                    request.Amount,
                    request.CancelUrl,
                    request.SuccessUrl
                );

                if (result == null)
                {
                    Console.WriteLine("PayOS service returned null result");
                    return Ok(new CreatePaymentResponseDto
                    {
                        Success = false,
                        ErrorMessage = "Không thể tạo giao dịch thanh toán. Vui lòng kiểm tra cấu hình PayOS."
                    });
                }

                Console.WriteLine($"Payment created successfully: {result.checkoutUrl}");
                Console.WriteLine($"Order code: {result.orderCode}");

                return Ok(new CreatePaymentResponseDto
                {
                    Success = true,
                    CheckoutUrl = result.checkoutUrl,
                    OrderCode = result.orderCode.ToString()
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating payment: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                return Ok(new CreatePaymentResponseDto
                {
                    Success = false,
                    ErrorMessage = $"Lỗi hệ thống: {ex.Message}"
                });
            }
        }

        [HttpPost("process-payment")]
        [Authorize]
        public async Task<ActionResult> ProcessPayment([FromBody] PaymentCallbackDto callback)
        {
            try
            {
                var userId = GetCurrentUserId();

                // Giao dịch bị hủy
                if (string.Equals(callback.Status, "CANCELLED", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(callback.Status, "cancel", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(callback.Cancel, "true", StringComparison.OrdinalIgnoreCase))
                {
                    return Ok(new { success = false, message = "Giao dịch đã bị hủy" });
                }

                var userCredit = await _creditRepository.CreateOrUpdateUserCreditAsync(userId, callback.Amount);

                var creditTransaction = new CreditTransaction
                {
                    UserId = userId,
                    Amount = callback.Amount,
                    TransactionType = "NapTien",
                    BalanceAfter = userCredit.Balance ?? 0,
                    Description = "Thanh toán thành công qua PayOS"
                };

                await _creditRepository.CreateTransactionAsync(creditTransaction);

                return Ok(new { success = true, message = "Nạp tiền thành công" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ProcessPayment: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("webhook")]
        [AllowAnonymous]
        public async Task<ActionResult> Webhook([FromBody] WebhookPayloadDto payload, [FromHeader(Name = "x-payos-signature")] string signature)
        {
            try
            {
                Console.WriteLine($"Webhook received: {payload.OrderCode}");

                if (!_payOSService.VerifyWebhookSignature(payload, signature))
                    return Unauthorized();

                if (payload.Status != "PAID")
                    return Ok();

                if (!payload.Description.StartsWith("NAPTIENSJOB+"))
                    return BadRequest("Invalid description format");

                var userIdStr = payload.Description.Replace("NAPTIENSJOB+", "");
                if (!int.TryParse(userIdStr, out int userId))
                    return BadRequest("Invalid userId");

                // Check if transaction already processed
                var exists = await _creditRepository.TransactionExistsAsync(payload.OrderCode);
                if (exists)
                    return Ok();

                var userCredit = await _creditRepository.CreateOrUpdateUserCreditAsync(userId, payload.Amount);

                var creditTransaction = new CreditTransaction
                {
                    UserId = userId,
                    Amount = payload.Amount,
                    TransactionType = "Topup",
                    BalanceAfter = userCredit.Balance ?? 0,
                    Description = $"Thanh toán qua PayOS (Webhook), mã giao dịch {payload.OrderCode}"
                };

                await _creditRepository.CreateTransactionAsync(creditTransaction);

                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Webhook: {ex.Message}");
                return StatusCode(500, "Internal Server Error");
            }
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("UserId")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                throw new UnauthorizedAccessException("User not authenticated.");
            return int.Parse(userIdClaim);
        }
    }
}
