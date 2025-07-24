using BusinessObjects.DTOs.Customer;
using BusinessObjects.DTOs.Common;
using Microsoft.AspNetCore.Mvc;
using Repository.Interfaces;
using BusinessObjects.DTOs.Authen; // Add this for LoginResponseDto
using Sjob_API.Services;
using BusinessObjects.DTOs; // Add this for JwtService

namespace Sjob_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerApiController : ControllerBase
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IUserRepository _userRepository; // Inject IUserRepository
        private readonly JwtService _jwtService; // Inject JwtService

        public CustomerApiController(ICustomerRepository customerRepository, IUserRepository userRepository, JwtService jwtService)
        {
            _customerRepository = customerRepository;
            _userRepository = userRepository; // Initialize
            _jwtService = jwtService; // Initialize
        }

        [HttpGet("roles")]
        public async Task<ActionResult<ApiResponse<List<RoleDto>>>> GetRoles()
        {
            try
            {
                var roles = await _customerRepository.GetAllRolesAsync();
                var roleDtos = roles.Select(r => new RoleDto
                {
                    Id = r.Id,
                    Name = r.Name
                }).ToList();

                return Ok(ApiResponse<List<RoleDto>>.SuccessResult(roleDtos));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<RoleDto>>.ErrorResult($"Error retrieving roles: {ex.Message}"));
            }
        }

        [HttpPost("assign-role")]
        public async Task<ActionResult<ApiResponse<LoginResponseDto>>> AssignRole([FromBody] AssignRoleDto dto) // Change return type
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(ApiResponse<LoginResponseDto>.ErrorResult("Validation failed", errors));
                }

                var success = await _customerRepository.AssignRoleAsync(dto.UserId, dto.RoleId);

                if (success)
                {
                    // Fetch the user again to get the updated role
                    var updatedUser = await _userRepository.GetUserByIdAsync(dto.UserId);
                    if (updatedUser == null)
                    {
                        return NotFound(ApiResponse<LoginResponseDto>.ErrorResult("User not found after role assignment."));
                    }

                    // Generate a new JWT token with the updated role
                    string newToken = _jwtService.GenerateJwtToken(updatedUser, updatedUser.Role?.Name ?? "Customer");

                    // Map to UserDto for the response
                    var userDetail = await _userRepository.GetUserDetailByUserIdAsync(updatedUser.Id);
                    var userDto = new UserDto
                    {
                        Id = updatedUser.Id,
                        Email = updatedUser.Email,
                        Username = updatedUser.Username,
                        Avatar = updatedUser.Avatar,
                        Status = updatedUser.Status,
                        RoleName = updatedUser.Role?.Name ?? "",
                        FirstName = userDetail?.FirstName,
                        LastName = userDetail?.LastName
                    };

                    return Ok(ApiResponse<LoginResponseDto>.SuccessResult(new LoginResponseDto
                    {
                        Success = true,
                        Message = "Role assigned successfully",
                        User = userDto,
                        Token = newToken // Return the new token
                    }));
                }

                return BadRequest(ApiResponse<LoginResponseDto>.ErrorResult("Failed to assign role"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<LoginResponseDto>.ErrorResult($"Error assigning role: {ex.Message}"));
            }
        }

        [HttpGet("has-role/{userId}")]
        public async Task<ActionResult<ApiResponse<bool>>> HasRole(int userId)
        {
            try
            {
                var hasRole = await _customerRepository.UserHasRoleAsync(userId);
                return Ok(ApiResponse<bool>.SuccessResult(hasRole));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.ErrorResult($"Error checking user role: {ex.Message}"));
            }
        }

        [HttpGet("user-role/{userId}")]
        public async Task<ActionResult<ApiResponse<string>>> GetUserRole(int userId)
        {
            try
            {
                var user = await _customerRepository.GetUserByIdAsync(userId);

                if (user == null)
                {
                    return NotFound(ApiResponse<string>.ErrorResult("User not found"));
                }

                var roleName = user.Role?.Name ?? "Customer";
                return Ok(ApiResponse<string>.SuccessResult(roleName));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.ErrorResult($"Error retrieving user role: {ex.Message}"));
            }
        }
    }
}
