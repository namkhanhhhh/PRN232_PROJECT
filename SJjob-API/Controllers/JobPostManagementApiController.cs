using BusinessObjects.DTOs;
using BusinessObjects.DTOs.Authen;
using BusinessObjects.DTOs.JobPost;
using BusinessObjects.DTOs.Worker;
using BusinessObjects.DTOs.Customer;
using BusinessObjects.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository.Interfaces;
using System.Security.Claims;

namespace Sjob_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Add JWT authorization to the entire controller
    public class JobPostManagementApiController : ControllerBase
    {
        private readonly IJobPostManagementRepository _jobPostManagementRepository;

        public JobPostManagementApiController(IJobPostManagementRepository jobPostManagementRepository)
        {
            _jobPostManagementRepository = jobPostManagementRepository;
        }

        [HttpGet("employer/{employerId}")]
        [Authorize(Roles = "Employer,Admin")]
        public async Task<ActionResult<ApiResponseDto<List<JobPostListDto>>>> GetJobPostsByEmployer(int employerId)
        {
            try
            {
                // Check if user can access this employer's data
                var currentUserId = GetCurrentUserId();
                var currentUserRole = GetCurrentUserRole();

                if (currentUserRole != "Admin" && currentUserId != employerId)
                {
                    return Forbid();
                }

                var jobPosts = await _jobPostManagementRepository.GetJobPostsByEmployerAsync(employerId);
                var jobPostDtos = jobPosts.Select(jp => new JobPostListDto
                {
                    Id = jp.Id,
                    Title = jp.Title,
                    Description = jp.Description,
                    Requirements = jp.Requirements,
                    Benefits = jp.Benefits,
                    Location = jp.Location,
                    SalaryMin = jp.SalaryMin,
                    SalaryMax = jp.SalaryMax,
                    JobType = jp.JobType,
                    ExperienceLevel = jp.ExperienceLevel,
                    Deadline = jp.Deadline,
                    ImageMain = jp.ImageMain,
                    PostType = jp.PostType,
                    Status = jp.Status,
                    ViewCount = jp.ViewCount,
                    IsFeatured = jp.IsFeatured,
                    CreatedAt = jp.CreatedAt,
                    ApplicationCount = jp.Applications?.Count ?? 0,
                    Categories = jp.JobPostCategories?.Select(jpc => jpc.Category?.Name ?? "").ToList() ?? new List<string>(),
                    EmployerName = jp.User?.Username ?? ""
                }).ToList();

                return Ok(new ApiResponseDto<List<JobPostListDto>>
                {
                    Success = true,
                    Data = jobPostDtos,
                    Message = "Job posts retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDto<List<JobPostListDto>>
                {
                    Success = false,
                    Message = $"Error retrieving job posts: {ex.Message}"
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponseDto<JobPostListDto>>> GetJobPostById(int id)
        {
            try
            {
                var jobPost = await _jobPostManagementRepository.GetJobPostByIdAsync(id);
                if (jobPost == null)
                {
                    return NotFound(new ApiResponseDto<JobPostListDto>
                    {
                        Success = false,
                        Message = "Job post not found"
                    });
                }

                var jobPostDto = new JobPostListDto
                {
                    Id = jobPost.Id,
                    Title = jobPost.Title,
                    Description = jobPost.Description,
                    Requirements = jobPost.Requirements,
                    Benefits = jobPost.Benefits,
                    Location = jobPost.Location,
                    SalaryMin = jobPost.SalaryMin,
                    SalaryMax = jobPost.SalaryMax,
                    JobType = jobPost.JobType,
                    ExperienceLevel = jobPost.ExperienceLevel,
                    Deadline = jobPost.Deadline,
                    ImageMain = jobPost.ImageMain,
                    PostType = jobPost.PostType,
                    Status = jobPost.Status,
                    ViewCount = jobPost.ViewCount,
                    IsFeatured = jobPost.IsFeatured,
                    CreatedAt = jobPost.CreatedAt,
                    ApplicationCount = jobPost.Applications?.Count ?? 0,
                    Categories = jobPost.JobPostCategories?.Select(jpc => jpc.Category?.Name ?? "").ToList() ?? new List<string>(),
                    EmployerName = jobPost.User?.Username ?? ""
                };

                return Ok(new ApiResponseDto<JobPostListDto>
                {
                    Success = true,
                    Data = jobPostDto,
                    Message = "Job post retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDto<JobPostListDto>
                {
                    Success = false,
                    Message = $"Error retrieving job post: {ex.Message}"
                });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Employer")]
        public async Task<ActionResult<ApiResponseDto<bool>>> CreateJobPost([FromBody] JobPostCreateDto jobPostDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "Invalid data provided"
                    });
                }

                // Ensure user can only create job posts for themselves
                var currentUserId = GetCurrentUserId();
                if (jobPostDto.UserId != currentUserId)
                {
                    return Forbid();
                }

                var jobPost = new JobPost
                {
                    UserId = jobPostDto.UserId,
                    Title = jobPostDto.Title,
                    Description = jobPostDto.Description,
                    Requirements = jobPostDto.Requirements,
                    Benefits = jobPostDto.Benefits,
                    Location = jobPostDto.Location,
                    SalaryMin = jobPostDto.SalaryMin,
                    SalaryMax = jobPostDto.SalaryMax,
                    JobType = jobPostDto.JobType,
                    ExperienceLevel = jobPostDto.ExperienceLevel,
                    Deadline = jobPostDto.Deadline,
                    ImageMain = jobPostDto.ImageMain,
                    Image2 = jobPostDto.Image2,
                    Image3 = jobPostDto.Image3,
                    Image4 = jobPostDto.Image4,
                    PostType = jobPostDto.PostType
                };

                var result = await _jobPostManagementRepository.CreateJobPostAsync(jobPost);

                return Ok(new ApiResponseDto<bool>
                {
                    Success = result,
                    Data = result,
                    Message = result ? "Job post created successfully" : "Failed to create job post"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = $"Error creating job post: {ex.Message}"
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponseDto<bool>>> UpdateJobPost(int id, [FromBody] JobPostUpdateDto jobPostDto)
        {
            try
            {
                if (id != jobPostDto.Id)
                {
                    return BadRequest(new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "Job post ID mismatch"
                    });
                }

                var existingJobPost = await _jobPostManagementRepository.GetJobPostByIdAsync(id);
                if (existingJobPost == null)
                {
                    return NotFound(new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "Job post not found"
                    });
                }

                existingJobPost.Title = jobPostDto.Title;
                existingJobPost.Description = jobPostDto.Description;
                existingJobPost.Requirements = jobPostDto.Requirements;
                existingJobPost.Benefits = jobPostDto.Benefits;
                existingJobPost.Location = jobPostDto.Location;
                existingJobPost.SalaryMin = jobPostDto.SalaryMin;
                existingJobPost.SalaryMax = jobPostDto.SalaryMax;
                existingJobPost.JobType = jobPostDto.JobType;
                existingJobPost.ExperienceLevel = jobPostDto.ExperienceLevel;
                existingJobPost.Deadline = jobPostDto.Deadline;
                existingJobPost.ImageMain = jobPostDto.ImageMain;
                existingJobPost.Image2 = jobPostDto.Image2;
                existingJobPost.Image3 = jobPostDto.Image3;
                existingJobPost.Image4 = jobPostDto.Image4;
                existingJobPost.Status = jobPostDto.Status;

                var result = await _jobPostManagementRepository.UpdateJobPostAsync(existingJobPost);

                return Ok(new ApiResponseDto<bool>
                {
                    Success = result,
                    Data = result,
                    Message = result ? "Job post updated successfully" : "Failed to update job post"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = $"Error updating job post: {ex.Message}"
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponseDto<bool>>> DeleteJobPost(int id)
        {
            try
            {
                var result = await _jobPostManagementRepository.DeleteJobPostAsync(id);

                return Ok(new ApiResponseDto<bool>
                {
                    Success = result,
                    Data = result,
                    Message = result ? "Job post deactivated successfully" : "Failed to deactivate job post"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = $"Error deactivating job post: {ex.Message}"
                });
            }
        }

        [HttpPost("add-credit-transaction")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponseDto<bool>>> AddCreditTransaction([FromBody] AddCreditTransactionDto dto)
        {
            try
            {
                var result = await _jobPostManagementRepository.AddCreditTransactionAsync(
                    dto.UserId, dto.Amount, dto.TransactionType, dto.Description);

                return Ok(new ApiResponseDto<bool>
                {
                    Success = result,
                    Data = result,
                    Message = result ? "Credit transaction added successfully" : "Failed to add credit transaction"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = $"Error adding credit transaction: {ex.Message}"
                });
            }
        }

        [HttpGet("categories")]
        public async Task<ActionResult<ApiResponseDto<List<JobCategoryDto>>>> GetJobCategories()
        {
            try
            {
                var categories = await _jobPostManagementRepository.GetJobCategoriesAsync();
                var categoryDtos = categories.Select(c => new JobCategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description
                }).ToList();

                return Ok(new ApiResponseDto<List<JobCategoryDto>>
                {
                    Success = true,
                    Data = categoryDtos,
                    Message = "Job categories retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDto<List<JobCategoryDto>>
                {
                    Success = false,
                    Message = $"Error retrieving job categories: {ex.Message}"
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
