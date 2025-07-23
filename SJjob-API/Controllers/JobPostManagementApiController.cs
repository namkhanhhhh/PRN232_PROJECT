using BusinessObjects.DTOs;
using BusinessObjects.DTOs.Authen;
using BusinessObjects.DTOs.JobPost;
using BusinessObjects.DTOs.Worker;
using BusinessObjects.DTOs.Customer;
using BusinessObjects.DTOs.Employer;
using BusinessObjects.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository.Interfaces;
using System.Security.Claims;
using System.Linq;

namespace Sjob_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
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
                    Image2 = jp.Image2,
                    Image3 = jp.Image3,
                    Image4 = jp.Image4,
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

        [HttpGet("user-credits/{userId}")]
        [Authorize(Roles = "Employer,Admin")]
        public async Task<ActionResult<ApiResponseDto<UserPostCreditsDto>>> GetUserPostCredits(int userId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var currentUserRole = GetCurrentUserRole();

                if (currentUserRole != "Admin" && currentUserId != userId)
                {
                    return Forbid();
                }

                var userPostCredits = await _jobPostManagementRepository.GetUserPostCreditsAsync(userId);

                if (userPostCredits == null)
                {
                    return Ok(new ApiResponseDto<UserPostCreditsDto>
                    {
                        Success = true,
                        Data = new UserPostCreditsDto
                        {
                            SilverPostsAvailable = 0,
                            GoldPostsAvailable = 0,
                            DiamondPostsAvailable = 0,
                            AuthenLogoAvailable = 0,
                            PushToTopAvailable = 0,
                            LastUpdated = DateTime.Now
                        },
                        Message = "User post credits retrieved successfully"
                    });
                }

                var creditsDto = new UserPostCreditsDto
                {
                    SilverPostsAvailable = userPostCredits.SilverPostsAvailable,
                    GoldPostsAvailable = userPostCredits.GoldPostsAvailable,
                    DiamondPostsAvailable = userPostCredits.DiamondPostsAvailable,
                    AuthenLogoAvailable = userPostCredits.AuthenLogoAvailable,
                    PushToTopAvailable = userPostCredits.PushToTopAvailable,
                    LastUpdated = userPostCredits.LastUpdated
                };

                return Ok(new ApiResponseDto<UserPostCreditsDto>
                {
                    Success = true,
                    Data = creditsDto,
                    Message = "User post credits retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDto<UserPostCreditsDto>
                {
                    Success = false,
                    Message = $"Error retrieving user post credits: {ex.Message}"
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
                    Image2 = jobPost.Image2,
                    Image3 = jobPost.Image3,
                    Image4 = jobPost.Image4,
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

        [HttpGet("{id}/categories")]
        public async Task<ActionResult<ApiResponseDto<List<int>>>> GetJobPostCategories(int id)
        {
            try
            {
                var jobPost = await _jobPostManagementRepository.GetJobPostByIdAsync(id);
                if (jobPost == null)
                {
                    return NotFound(new ApiResponseDto<List<int>>
                    {
                        Success = false,
                        Message = "Job post not found"
                    });
                }

                var categoryIds = jobPost.JobPostCategories?.Select(jpc => jpc.CategoryId).ToList() ?? new List<int>();

                return Ok(new ApiResponseDto<List<int>>
                {
                    Success = true,
                    Data = categoryIds,
                    Message = "Job post categories retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDto<List<int>>
                {
                    Success = false,
                    Message = $"Error retrieving job post categories: {ex.Message}"
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

                var currentUserId = GetCurrentUserId();
                if (jobPostDto.UserId != currentUserId)
                {
                    return Forbid();
                }

                var canDeduct = await _jobPostManagementRepository.DeductPostCreditAsync(currentUserId, jobPostDto.PostType);
                if (!canDeduct)
                {
                    return BadRequest(new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = $"Không đủ lượt đăng bài loại {jobPostDto.PostType}. Vui lòng mua thêm gói dịch vụ."
                    });
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

                // Add categories
                if (jobPostDto.CategoryIds?.Any() == true)
                {
                    jobPost.JobPostCategories = jobPostDto.CategoryIds.Select(categoryId => new JobPostCategory
                    {
                        CategoryId = categoryId,
                        CreatedAt = DateTime.Now
                    }).ToList();
                }

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

                var result = await _jobPostManagementRepository.UpdateJobPostWithCategoriesAsync(jobPostDto);

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

        [HttpPost("{id}/deactivate")]
        [Authorize(Roles = "Employer,Admin")]
        public async Task<ActionResult<ApiResponseDto<bool>>> DeactivateJobPost(int id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var currentUserRole = GetCurrentUserRole();

                // Check if user owns this job post (unless admin)
                if (currentUserRole != "Admin")
                {
                    var jobPost = await _jobPostManagementRepository.GetJobPostByIdAsync(id);
                    if (jobPost == null || jobPost.UserId != currentUserId)
                    {
                        return Forbid();
                    }
                }

                var result = await _jobPostManagementRepository.DeleteJobPostAsync(id); // This sets status to inactive

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

        [HttpPost("{id}/activate")]
        [Authorize(Roles = "Employer,Admin")]
        public async Task<ActionResult<ApiResponseDto<bool>>> ActivateJobPost(int id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var currentUserRole = GetCurrentUserRole();

                // Check if user owns this job post (unless admin)
                if (currentUserRole != "Admin")
                {
                    var jobPost = await _jobPostManagementRepository.GetJobPostByIdAsync(id);
                    if (jobPost == null || jobPost.UserId != currentUserId)
                    {
                        return Forbid();
                    }
                }

                var result = await _jobPostManagementRepository.ActivateJobPostAsync(id);

                return Ok(new ApiResponseDto<bool>
                {
                    Success = result,
                    Data = result,
                    Message = result ? "Job post activated successfully" : "Failed to activate job post"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = $"Error activating job post: {ex.Message}"
                });
            }
        }

        [HttpPost("{id}/toggle-status")]
        [Authorize(Roles = "Employer,Admin")]
        public async Task<ActionResult<ApiResponseDto<bool>>> ToggleJobPostStatus(int id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var currentUserRole = GetCurrentUserRole();

                // Check if user owns this job post (unless admin)
                if (currentUserRole != "Admin")
                {
                    var jobPost = await _jobPostManagementRepository.GetJobPostByIdAsync(id);
                    if (jobPost == null || jobPost.UserId != currentUserId)
                    {
                        return Forbid();
                    }
                }

                var result = await _jobPostManagementRepository.ToggleJobPostStatusAsync(id);

                return Ok(new ApiResponseDto<bool>
                {
                    Success = result,
                    Data = result,
                    Message = result ? "Job post status updated successfully" : "Failed to update job post status"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = $"Error updating job post status: {ex.Message}"
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
        public async Task<ActionResult<ApiResponseDto<List<JobCategoryHierarchyDto>>>> GetJobCategories()
        {
            try
            {
                var categories = await _jobPostManagementRepository.GetJobCategoriesAsync();

                // Build hierarchy
                var parentCategories = categories.Where(c => c.ParentId == null).ToList();
                var categoryDtos = parentCategories.Select(c => new JobCategoryHierarchyDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    ParentId = c.ParentId,
                    Children = categories.Where(child => child.ParentId == c.Id)
                        .Select(child => new JobCategoryHierarchyDto
                        {
                            Id = child.Id,
                            Name = child.Name,
                            Description = child.Description,
                            ParentId = child.ParentId,
                            Children = new List<JobCategoryHierarchyDto>()
                        }).ToList()
                }).ToList();

                return Ok(new ApiResponseDto<List<JobCategoryHierarchyDto>>
                {
                    Success = true,
                    Data = categoryDtos,
                    Message = "Job categories retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDto<List<JobCategoryHierarchyDto>>
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

    public class AddCreditTransactionDto
    {
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        public string TransactionType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class JobCategoryHierarchyDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? ParentId { get; set; }
        public List<JobCategoryHierarchyDto> Children { get; set; } = new List<JobCategoryHierarchyDto>();
    }
}
