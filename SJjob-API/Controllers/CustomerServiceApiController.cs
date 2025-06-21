using BusinessObjects.DTOs.Common;
using BusinessObjects.DTOs.CustomerService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository.Interfaces;
using System.Security.Claims;

namespace Sjob_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CustomerServiceApiController : ControllerBase
    {
        private readonly ICustomerServiceRepository _customerServiceRepository;

        public CustomerServiceApiController(ICustomerServiceRepository customerServiceRepository)
        {
            _customerServiceRepository = customerServiceRepository;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                throw new UnauthorizedAccessException("User ID not found in token");
            return int.Parse(userIdClaim);
        }

        [HttpGet("index")]
        public async Task<ActionResult<ApiResponse<CustomerServiceIndexDto>>> GetCustomerServiceIndex()
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _customerServiceRepository.GetCustomerServiceIndexAsync(userId);

                return Ok(new ApiResponse<CustomerServiceIndexDto>
                {
                    Success = true,
                    Data = result,
                    Message = "Customer service data retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<CustomerServiceIndexDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpGet("buy")]
        public async Task<ActionResult<ApiResponse<BuyViewDto>>> GetBuyViewData([FromQuery] int id, [FromQuery] string type)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _customerServiceRepository.GetBuyViewDataAsync(userId, id, type);

                if ((type.ToLower() == "service" && result.Service == null) ||
                    (type.ToLower() == "combo" && result.Combo == null))
                {
                    return NotFound(new ApiResponse<BuyViewDto>
                    {
                        Success = false,
                        Message = $"{type} not found"
                    });
                }

                return Ok(new ApiResponse<BuyViewDto>
                {
                    Success = true,
                    Data = result,
                    Message = "Buy view data retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<BuyViewDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpPost("purchase")]
        public async Task<ActionResult<ApiResponse>> ProcessPurchase([FromBody] PurchaseRequestDto request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var success = await _customerServiceRepository.ProcessPurchaseAsync(userId, request);

                if (success)
                {
                    return Ok(new ApiResponse
                    {
                        Success = true,
                        Message = "Purchase completed successfully"
                    });
                }
                else
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "Purchase failed. Please check your balance and try again."
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpGet("user-packages")]
        public async Task<ActionResult<ApiResponse<UserPackagesDto>>> GetUserPackages()
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _customerServiceRepository.GetUserPackagesAsync(userId);

                return Ok(new ApiResponse<UserPackagesDto>
                {
                    Success = true,
                    Data = result,
                    Message = "User packages retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<UserPackagesDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpPost("sync-purchase-history")]
        public async Task<ActionResult<ApiResponse>> SyncPurchaseHistory()
        {
            try
            {
                var userId = GetCurrentUserId();
                var success = await _customerServiceRepository.SyncPurchaseHistoryAsync(userId);

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = success ? "Purchase history synchronized successfully" : "No new data to synchronize"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }
    }
}