using BusinessObjects.DTOs.Authen;
using BusinessObjects.DTOs.Common;
using BusinessObjects.DTOs.Employer;
using BusinessObjects.DTOs.Credit;
using DataAccess.DAO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository.Interfaces;
using Sjob_API.Filters;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Sjob_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class EmployerApiController : ControllerBase
    {
        private readonly IEmployerRepository _employerRepository;
        private readonly EmployerDAO _employerDAO;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public EmployerApiController(
            IEmployerRepository employerRepository,
            EmployerDAO employerDAO,
            IWebHostEnvironment webHostEnvironment)
        {
            _employerRepository = employerRepository;
            _employerDAO = employerDAO;
            _webHostEnvironment = webHostEnvironment;
        }

        private int GetCurrentUserId()
        {
            // Thử lấy từ nhiều claim khác nhau
            var userIdClaim = User.FindFirst("UserId")?.Value ??
                             User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                             User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
            {
                // Log để debug
                var allClaims = User.Claims.Select(c => $"{c.Type}: {c.Value}").ToList();
                Console.WriteLine($"All claims: {string.Join(", ", allClaims)}");
                throw new UnauthorizedAccessException("UserId claim is missing or user not authenticated.");
            }

            if (!int.TryParse(userIdClaim, out int userId))
            {
                throw new UnauthorizedAccessException("Invalid UserId format.");
            }

            return userId;
        }

        [HttpGet("balance/{userId}")]
        public async Task<ActionResult<ApiResponse<UserCreditDto>>> GetUserBalance(int userId)
        {
            try
            {
                // Use the repository to get balance
                var balance = await _employerRepository.GetEmployerBalanceAsync(userId);

                var userCreditDto = new UserCreditDto
                {
                    UserId = userId,
                    Balance = balance,
                    LastUpdated = DateTime.Now
                };

                return Ok(new ApiResponse<UserCreditDto>
                {
                    Success = true,
                    Data = userCreditDto,
                    Message = "User balance retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                // Log the error for debugging
                Console.WriteLine($"Error getting balance for user {userId}: {ex.Message}");

                return StatusCode(500, new ApiResponse<UserCreditDto>
                {
                    Success = false,
                    Message = $"Error retrieving user balance: {ex.Message}"
                });
            }
        }

        [Authorize]
        [AuthorizationRequired(Roles = "Employer")]
        [HttpGet("profile")]
        public async Task<ActionResult<ApiResponse<EmployerProfileDto>>> GetProfile()
        {
            try
            {
                var userId = GetCurrentUserId();
                var profile = await _employerRepository.GetEmployerProfileAsync(userId);

                if (profile == null)
                {
                    return NotFound(new ApiResponse<EmployerProfileDto>
                    {
                        Success = false,
                        Message = "Không tìm thấy thông tin người dùng"
                    });
                }

                return Ok(new ApiResponse<EmployerProfileDto>
                {
                    Success = true,
                    Message = "Lấy thông tin profile thành công",
                    Data = profile
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ApiResponse<EmployerProfileDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<EmployerProfileDto>
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        [Authorize]
        [AuthorizationRequired(Roles = "Employer")]
        [HttpGet("profile/edit")]
        public async Task<ActionResult<ApiResponse<EditEmployerProfileDto>>> GetEditProfile()
        {
            try
            {
                var userId = GetCurrentUserId();
                var user = await _employerDAO.GetEmployerWithDetailsAsync(userId);

                if (user == null)
                {
                    return NotFound(new ApiResponse<EditEmployerProfileDto>
                    {
                        Success = false,
                        Message = "Không tìm thấy thông tin người dùng"
                    });
                }

                var userDetail = user.UserDetails.FirstOrDefault();

                var editDto = new EditEmployerProfileDto
                {
                    UserId = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    Avatar = user.Avatar,
                    FirstName = userDetail?.FirstName,
                    LastName = userDetail?.LastName,
                    PhoneNumber = userDetail?.PhoneNumber,
                    Address = userDetail?.Address
                };

                return Ok(new ApiResponse<EditEmployerProfileDto>
                {
                    Success = true,
                    Message = "Lấy thông tin chỉnh sửa thành công",
                    Data = editDto
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ApiResponse<EditEmployerProfileDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<EditEmployerProfileDto>
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        [Authorize]
        [AuthorizationRequired(Roles = "Employer")]
        [HttpPut("profile")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateProfile([FromForm] EditEmployerProfileDto model, IFormFile? avatarFile)
        {
            try
            {
                var userId = GetCurrentUserId();
                model.UserId = userId;

                // Handle avatar upload
                if (avatarFile != null && avatarFile.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(avatarFile.FileName);
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "avatars");

                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await avatarFile.CopyToAsync(stream);
                    }

                    model.Avatar = "/images/avatars/" + fileName;
                }

                var result = await _employerRepository.UpdateEmployerProfileAsync(model);

                if (result)
                {
                    return Ok(new ApiResponse<bool>
                    {
                        Success = true,
                        Message = "Cập nhật thông tin thành công",
                        Data = true
                    });
                }

                return BadRequest(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Cập nhật thông tin thất bại"
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ApiResponse<bool>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        [Authorize]
        [AuthorizationRequired(Roles = "Employer")]
        [HttpPost("change-password")]
        public async Task<ActionResult<ApiResponse<bool>>> ChangePassword([FromBody] ChangePasswordDto model)
        {
            try
            {
                var userId = GetCurrentUserId();
                var user = await _employerDAO.GetUserByIdAsync(userId);

                if (user == null)
                {
                    return NotFound(new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Không tìm thấy người dùng"
                    });
                }

                // Verify current password
                if (user.Password != HashPassword(model.CurrentPassword))
                {
                    return BadRequest(new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Mật khẩu hiện tại không chính xác"
                    });
                }

                // Update password using DAO
                var result = await _employerDAO.UpdateUserPasswordAsync(userId, HashPassword(model.NewPassword));

                if (result)
                {
                    return Ok(new ApiResponse<bool>
                    {
                        Success = true,
                        Message = "Đổi mật khẩu thành công",
                        Data = true
                    });
                }

                return BadRequest(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Đổi mật khẩu thất bại"
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ApiResponse<bool>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        [Authorize]
        [AuthorizationRequired(Roles = "Employer")]
        [HttpGet("history")]
        public async Task<ActionResult<ApiResponse<EmployerHistoryDto>>> GetHistory()
        {
            try
            {
                var userId = GetCurrentUserId();
                var history = await _employerRepository.GetEmployerHistoryAsync(userId);

                return Ok(new ApiResponse<EmployerHistoryDto>
                {
                    Success = true,
                    Message = "Lấy lịch sử thành công",
                    Data = history
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ApiResponse<EmployerHistoryDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<EmployerHistoryDto>
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                return Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(password)));
            }
        }
    }
}
