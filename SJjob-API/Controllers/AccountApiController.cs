using BusinessObjects.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository.Interfaces;
using System.Security.Claims;
using BusinessObjects.Models;
using Org.BouncyCastle.Crypto.Generators;
using BusinessObjects.DTOs.Authen;

namespace Sjob_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AccountApiController : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        public AccountApiController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpPost("users")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PaginatedResponseDto<UserDto>>> GetUsers([FromBody] PaginatedRequestDto request)
        {
            try
            {
                var users = await _userRepository.GetAllUsersAsync(request.Page, request.PageSize, request.SearchTerm, request.SortBy, request.SortOrder);
                var totalUsers = await _userRepository.GetTotalUsersCountAsync(request.SearchTerm);
                var totalPages = (int)Math.Ceiling(totalUsers / (double)request.PageSize);

                var userDtos = users.Select(u => new UserDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    Status = u.Status,
                    RoleName = u.Role?.Name ?? "",
                    FirstName = u.UserDetails.FirstOrDefault()?.FirstName ?? "",
                    LastName = u.UserDetails.FirstOrDefault()?.LastName ?? "",
                    PhoneNumber = u.UserDetails.FirstOrDefault()?.PhoneNumber,
                    Address = u.UserDetails.FirstOrDefault()?.Address,
                    Avatar = u.Avatar,
                    CreatedAt = u.UserDetails.FirstOrDefault()?.CreatedAt
                }).ToList();

                return Ok(new PaginatedResponseDto<UserDto>
                {
                    Success = true,
                    Message = "Lấy danh sách người dùng thành công",
                    Items = userDtos,
                    TotalPages = totalPages,
                    CurrentPage = request.Page,
                    TotalItems = totalUsers,
                    SearchTerm = request.SearchTerm,
                    SortBy = request.SortBy,
                    SortOrder = request.SortOrder
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new PaginatedResponseDto<UserDto>
                {
                    Success = false,
                    Message = "Đã xảy ra lỗi khi lấy danh sách người dùng"
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponseDto<UserDto>>> GetUserById(int id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var currentUserRole = GetCurrentUserRole();

                // Only admin can view other users, or user can view their own profile
                if (currentUserRole != "Admin" && currentUserId != id)
                {
                    return Forbid();
                }

                var user = await _userRepository.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound(new ApiResponseDto<UserDto>
                    {
                        Success = false,
                        Message = "Không tìm thấy người dùng"
                    });
                }

                var userDto = new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    Status = user.Status,
                    RoleName = user.Role?.Name ?? "",
                    FirstName = user.UserDetails.FirstOrDefault()?.FirstName ?? "",
                    LastName = user.UserDetails.FirstOrDefault()?.LastName ?? "",
                    PhoneNumber = user.UserDetails.FirstOrDefault()?.PhoneNumber,
                    Address = user.UserDetails.FirstOrDefault()?.Address,
                    Avatar = user.Avatar,
                    CreatedAt = user.UserDetails.FirstOrDefault()?.CreatedAt,
                    Headline = user.UserDetails.FirstOrDefault()?.Headline,
                    ExperienceYears = user.UserDetails.FirstOrDefault()?.ExperienceYears,
                    Education = user.UserDetails.FirstOrDefault()?.Education,
                    Skills = user.UserDetails.FirstOrDefault()?.Skills,
                    Bio = user.UserDetails.FirstOrDefault()?.Bio
                };

                return Ok(new ApiResponseDto<UserDto>
                {
                    Success = true,
                    Message = "Lấy thông tin người dùng thành công",
                    Data = userDto
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDto<UserDto>
                {
                    Success = false,
                    Message = "Đã xảy ra lỗi khi lấy thông tin người dùng"
                });
            }
        }

        [HttpPost("toggle-status/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponseDto>> ToggleUserStatus(int id)
        {
            try
            {
                var user = await _userRepository.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound(new ApiResponseDto
                    {
                        Success = false,
                        Message = "Không tìm thấy người dùng"
                    });
                }

                var success = await _userRepository.ToggleUserStatusAsync(id);
                if (!success)
                {
                    return StatusCode(500, new ApiResponseDto
                    {
                        Success = false,
                        Message = "Không thể thay đổi trạng thái người dùng"
                    });
                }

                var newStatus = !user.Status;
                return Ok(new ApiResponseDto
                {
                    Success = true,
                    Message = $"Người dùng đã được {(newStatus ? "vô hiệu hóa" : "kích hoạt")} thành công"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDto
                {
                    Success = false,
                    Message = "Đã xảy ra lỗi khi thay đổi trạng thái người dùng"
                });
            }
        }

        [HttpPut("profile")]
        public async Task<ActionResult<ApiResponseDto>> UpdateProfile([FromBody] UserDto userDto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var currentUserRole = GetCurrentUserRole();

                // Only admin can update other users, or user can update their own profile
                if (currentUserRole != "Admin" && currentUserId != userDto.Id)
                {
                    return Forbid();
                }

                var success = await _userRepository.UpdateUserProfileAsync(
                    userDto.Id,
                    userDto.FirstName,
                    userDto.LastName,
                    userDto.PhoneNumber,
                    userDto.Address
                );

                if (!success)
                {
                    return StatusCode(500, new ApiResponseDto
                    {
                        Success = false,
                        Message = "Không thể cập nhật thông tin cá nhân"
                    });
                }

                return Ok(new ApiResponseDto
                {
                    Success = true,
                    Message = "Cập nhật thông tin cá nhân thành công"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDto
                {
                    Success = false,
                    Message = "Đã xảy ra lỗi khi cập nhật thông tin cá nhân"
                });
            }
        }

        [HttpPost("change-password")]
        public async Task<ActionResult<ApiResponseDto>> ChangePassword([FromBody] ChangePasswordDto request)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var user = await _userRepository.GetUserByIdAsync(currentUserId);

                if (user == null)
                {
                    return NotFound(new ApiResponseDto
                    {
                        Success = false,
                        Message = "Không tìm thấy người dùng"
                    });
                }

                // Verify current password
                if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.Password))
                {
                    return BadRequest(new ApiResponseDto
                    {
                        Success = false,
                        Message = "Mật khẩu hiện tại không đúng"
                    });
                }

                // Validate new password
                if (request.NewPassword != request.ConfirmPassword)
                {
                    return BadRequest(new ApiResponseDto
                    {
                        Success = false,
                        Message = "Mật khẩu xác nhận không khớp"
                    });
                }

                // Hash new password
                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                var success = await _userRepository.UpdatePasswordAsync(currentUserId, hashedPassword);

                if (!success)
                {
                    return StatusCode(500, new ApiResponseDto
                    {
                        Success = false,
                        Message = "Không thể cập nhật mật khẩu"
                    });
                }

                return Ok(new ApiResponseDto
                {
                    Success = true,
                    Message = "Đổi mật khẩu thành công"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDto
                {
                    Success = false,
                    Message = "Đã xảy ra lỗi khi đổi mật khẩu"
                });
            }
        }

        [HttpPost("create")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponseDto>> CreateAccount([FromBody] CreateAccountDto request)
        {
            try
            {
                // Validate input
                if (await _userRepository.EmailExistsAsync(request.Email))
                {
                    return BadRequest(new ApiResponseDto
                    {
                        Success = false,
                        Message = "Email đã tồn tại"
                    });
                }

                if (await _userRepository.UsernameExistsAsync(request.Username))
                {
                    return BadRequest(new ApiResponseDto
                    {
                        Success = false,
                        Message = "Tên đăng nhập đã tồn tại"
                    });
                }

                if (request.Password != request.ConfirmPassword)
                {
                    return BadRequest(new ApiResponseDto
                    {
                        Success = false,
                        Message = "Mật khẩu xác nhận không khớp"
                    });
                }

                // Create user
                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);
                var user = new User
                {
                    Username = request.Username,
                    Email = request.Email,
                    Password = hashedPassword,
                    RoleId = request.RoleId,
                    Status = true
                };

                var createdUser = await _userRepository.CreateUserAsync(user);

                // Create user detail
                var userDetail = new UserDetail
                {
                    UserId = createdUser.Id,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    PhoneNumber = request.PhoneNumber,
                    CreatedAt = DateTime.Now
                };

                await _userRepository.CreateUserDetailAsync(userDetail);

                return Ok(new ApiResponseDto
                {
                    Success = true,
                    Message = "Tạo tài khoản thành công"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDto
                {
                    Success = false,
                    Message = "Đã xảy ra lỗi khi tạo tài khoản"
                });
            }
        }

        [HttpGet("roles")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponseDto<List<Role>>>> GetRoles()
        {
            try
            {
                var roles = await _userRepository.GetAllRolesAsync();
                return Ok(new ApiResponseDto<List<Role>>
                {
                    Success = true,
                    Message = "Lấy danh sách vai trò thành công",
                    Data = roles
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDto<List<Role>>
                {
                    Success = false,
                    Message = "Đã xảy ra lỗi khi lấy danh sách vai trò"
                });
            }
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
        }

        private string GetCurrentUserRole()
        {
            var roleClaim = User.FindFirst(ClaimTypes.Role);
            return roleClaim?.Value ?? "";
        }
    }
}
