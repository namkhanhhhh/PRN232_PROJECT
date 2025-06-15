using BusinessObjects.DTOs.Customer;
using BusinessObjects.DTOs.Common;
using Microsoft.AspNetCore.Mvc;
using Repository.Interfaces;

namespace Sjob_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerApiController : ControllerBase
    {
        private readonly ICustomerRepository _customerRepository;

        public CustomerApiController(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
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
        public async Task<ActionResult<ApiResponse>> AssignRole([FromBody] AssignRoleDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(ApiResponse.ErrorResult("Validation failed", errors));
                }

                var success = await _customerRepository.AssignRoleAsync(dto.UserId, dto.RoleId);

                if (success)
                {
                    return Ok(ApiResponse.SuccessResult("Role assigned successfully"));
                }

                return BadRequest(ApiResponse.ErrorResult("Failed to assign role"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.ErrorResult($"Error assigning role: {ex.Message}"));
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
