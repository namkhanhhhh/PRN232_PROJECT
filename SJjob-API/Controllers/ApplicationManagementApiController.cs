using BusinessObjects.DTOs.Application;
using BusinessObjects.DTOs.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository.Interfaces;
using System.Security.Claims;

namespace Sjob_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Employer")]
    public class ApplicationManagementApiController : ControllerBase
    {
        private readonly IApplicationManagementRepository _applicationManagementRepository;

        public ApplicationManagementApiController(IApplicationManagementRepository applicationManagementRepository)
        {
            _applicationManagementRepository = applicationManagementRepository;
        }

        [HttpPost("applications")]
        public async Task<ActionResult<ApiResponse<ApplicationSummaryDto>>> GetApplications([FromBody] ApplicationFilterDto filter)
        {
            try
            {
                var employerId = GetCurrentUserId();

                // Get employer's job posts
                var jobPosts = await _applicationManagementRepository.GetEmployerJobPostsAsync(employerId);

                // Get applications with filtering
                var (applications, totalCount) = await _applicationManagementRepository.GetApplicationsAsync(
                    employerId, filter.JobId, filter.Status, filter.Search, filter.Page, filter.PageSize);

                // Get application counts
                var (pending, accepted, rejected, total) = await _applicationManagementRepository.GetApplicationCountsAsync(
                    employerId, filter.JobId);

                // Map to DTOs
                var jobPostDtos = jobPosts.Select(jp => new JobPostSummaryDto
                {
                    Id = jp.Id,
                    Title = jp.Title,
                    Location = jp.Location,
                    JobType = jp.JobType,
                    CreatedAt = jp.CreatedAt,
                    ApplicationCount = jp.Applications?.Count ?? 0
                }).ToList();

                var applicationDtos = applications.Select(a => MapToApplicationListDto(a)).ToList();

                var totalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize);

                var result = new ApplicationSummaryDto
                {
                    JobPosts = jobPostDtos,
                    Applications = applicationDtos,
                    SelectedJobId = filter.JobId,
                    SelectedStatus = filter.Status,
                    SearchTerm = filter.Search,
                    PendingCount = pending,
                    AcceptedCount = accepted,
                    RejectedCount = rejected,
                    TotalCount = total,
                    TotalPages = totalPages,
                    CurrentPage = filter.Page
                };

                return Ok(new ApiResponse<ApplicationSummaryDto>
                {
                    Success = true,
                    Message = "Lấy danh sách đơn ứng tuyển thành công",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<ApplicationSummaryDto>
                {
                    Success = false,
                    Message = "Đã xảy ra lỗi khi lấy danh sách đơn ứng tuyển"
                });
            }
        }

        [HttpGet("applications/{id}")]
        public async Task<ActionResult<ApiResponse<ApplicationDetailDto>>> GetApplicationDetail(int id)
        {
            try
            {
                var employerId = GetCurrentUserId();
                var application = await _applicationManagementRepository.GetApplicationDetailAsync(id, employerId);

                if (application == null)
                {
                    return NotFound(new ApiResponse<ApplicationDetailDto>
                    {
                        Success = false,
                        Message = "Không tìm thấy đơn ứng tuyển"
                    });
                }

                var result = await MapToApplicationDetailDto(application);

                return Ok(new ApiResponse<ApplicationDetailDto>
                {
                    Success = true,
                    Message = "Lấy chi tiết đơn ứng tuyển thành công",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<ApplicationDetailDto>
                {
                    Success = false,
                    Message = "Đã xảy ra lỗi khi lấy chi tiết đơn ứng tuyển"
                });
            }
        }

        [HttpPost("applications/update-status")]
        public async Task<ActionResult<ApiResponse>> UpdateApplicationStatus([FromBody] UpdateApplicationStatusDto request)
        {
            try
            {
                var employerId = GetCurrentUserId();
                var success = await _applicationManagementRepository.UpdateApplicationStatusAsync(
                    request.ApplicationId, employerId, request.Status, request.Note);

                if (!success)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "Không tìm thấy đơn ứng tuyển"
                    });
                }

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Cập nhật trạng thái đơn ứng tuyển thành công"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "Đã xảy ra lỗi khi cập nhật trạng thái"
                });
            }
        }

        [HttpPost("applications/add-note")]
        public async Task<ActionResult<ApiResponse>> AddApplicationNote([FromBody] AddApplicationNoteDto request)
        {
            try
            {
                var employerId = GetCurrentUserId();
                var success = await _applicationManagementRepository.AddApplicationNoteAsync(
                    request.ApplicationId, employerId, request.Note);

                if (!success)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "Không tìm thấy đơn ứng tuyển"
                    });
                }

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Thêm ghi chú thành công"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "Đã xảy ra lỗi khi thêm ghi chú"
                });
            }
        }

        [HttpGet("job-stats/{jobId}")]
        public async Task<ActionResult<ApiResponse<JobStatsDto>>> GetJobStats(int jobId)
        {
            try
            {
                var employerId = GetCurrentUserId();
                var jobPost = await _applicationManagementRepository.GetJobPostWithApplicationsAsync(jobId, employerId);

                if (jobPost == null)
                {
                    return NotFound(new ApiResponse<JobStatsDto>
                    {
                        Success = false,
                        Message = "Không tìm thấy bài đăng"
                    });
                }

                // Get view statistics
                var (totalViews, uniqueViewers) = await _applicationManagementRepository.GetJobViewStatsAsync(jobId);

                // Calculate statistics
                var totalApplications = jobPost.Applications?.Count ?? 0;
                var pendingApplications = jobPost.Applications?.Count(a => a.Status == "pending") ?? 0;
                var acceptedApplications = jobPost.Applications?.Count(a => a.Status == "accepted") ?? 0;
                var rejectedApplications = jobPost.Applications?.Count(a => a.Status == "rejected") ?? 0;

                var conversionRate = uniqueViewers > 0 ? (double)totalApplications / uniqueViewers * 100 : 0;

                // Get recent applications
                var recentApplications = jobPost.Applications?
                    .OrderByDescending(a => a.CreatedAt)
                    .Take(10)
                    .Select(a => MapToApplicationListDto(a))
                    .ToList() ?? new List<ApplicationListDto>();

                var result = new JobStatsDto
                {
                    JobPostId = jobPost.Id,
                    JobTitle = jobPost.Title,
                    JobLocation = jobPost.Location,
                    JobType = jobPost.JobType,
                    SalaryMin = jobPost.SalaryMin,
                    SalaryMax = jobPost.SalaryMax,
                    JobCreatedAt = jobPost.CreatedAt,
                    JobDeadline = jobPost.Deadline.HasValue ? jobPost.Deadline.Value.ToDateTime(TimeOnly.MinValue) : null,
                    JobStatus = jobPost.Status,
                    PostType = jobPost.PostType,
                    TotalApplications = totalApplications,
                    PendingApplications = pendingApplications,
                    AcceptedApplications = acceptedApplications,
                    RejectedApplications = rejectedApplications,
                    TotalViews = totalViews,
                    UniqueViewers = uniqueViewers,
                    ConversionRate = conversionRate,
                    RecentApplications = recentApplications
                };

                return Ok(new ApiResponse<JobStatsDto>
                {
                    Success = true,
                    Message = "Lấy thống kê bài đăng thành công",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<JobStatsDto>
                {
                    Success = false,
                    Message = "Đã xảy ra lỗi khi lấy thống kê"
                });
            }
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
        }

        private ApplicationListDto MapToApplicationListDto(BusinessObjects.Models.Application application)
        {
            var userDetail = application.User?.UserDetails?.FirstOrDefault();
            var fullName = userDetail != null ? $"{userDetail.FirstName} {userDetail.LastName}" : application.User?.Username ?? "";

            return new ApplicationListDto
            {
                Id = application.Id,
                JobPostId = application.JobPostId,
                UserId = application.UserId,
                Status = application.Status,
                EmployerRating = application.EmployerRating,
                WorkerRating = application.WorkerRating,
                CreatedAt = application.CreatedAt,
                JobTitle = application.JobPost?.Title ?? "",
                JobLocation = application.JobPost?.Location,
                JobType = application.JobPost?.JobType,
                SalaryMin = application.JobPost?.SalaryMin,
                SalaryMax = application.JobPost?.SalaryMax,
                WorkerName = fullName,
                WorkerEmail = application.User?.Email,
                WorkerAvatar = application.User?.Avatar,
                WorkerPhone = userDetail?.PhoneNumber,
                WorkerAddress = userDetail?.Address,
                WorkerHeadline = userDetail?.Headline,
                WorkerExperienceYears = userDetail?.ExperienceYears,
                WorkerEducation = userDetail?.Education,
                WorkerSkills = userDetail?.Skills,
                WorkerBio = userDetail?.Bio
            };
        }

        private async Task<ApplicationDetailDto> MapToApplicationDetailDto(BusinessObjects.Models.Application application)
        {
            var userDetail = application.User?.UserDetails?.FirstOrDefault();
            var fullName = userDetail != null ? $"{userDetail.FirstName} {userDetail.LastName}" : application.User?.Username ?? "";

            // Load notes separately
            var notes = await _applicationManagementRepository.GetApplicationNotesAsync(application.Id);

            return new ApplicationDetailDto
            {
                Id = application.Id,
                JobPostId = application.JobPostId,
                UserId = application.UserId,
                Status = application.Status,
                EmployerRating = application.EmployerRating,
                WorkerRating = application.WorkerRating,
                CreatedAt = application.CreatedAt,
                JobTitle = application.JobPost?.Title ?? "",
                JobDescription = application.JobPost?.Description,
                JobRequirements = application.JobPost?.Requirements,
                JobLocation = application.JobPost?.Location,
                JobType = application.JobPost?.JobType,
                SalaryMin = application.JobPost?.SalaryMin,
                SalaryMax = application.JobPost?.SalaryMax,
                JobDeadline = application.JobPost?.Deadline?.ToDateTime(TimeOnly.MinValue),
                WorkerName = fullName,
                WorkerEmail = application.User?.Email,
                WorkerAvatar = application.User?.Avatar,
                WorkerPhone = userDetail?.PhoneNumber,
                WorkerAddress = userDetail?.Address,
                WorkerHeadline = userDetail?.Headline,
                WorkerExperienceYears = userDetail?.ExperienceYears,
                WorkerEducation = userDetail?.Education,
                WorkerSkills = userDetail?.Skills,
                WorkerBio = userDetail?.Bio,
                WorkerDesiredPosition = userDetail?.DesiredPosition,
                WorkerDesiredSalary = userDetail?.DesiredSalary,
                WorkerDesiredLocation = userDetail?.DesiredLocation,
                WorkerAvailability = userDetail?.Availability,
                Notes = notes.Select(n => new ApplicationNoteDto
                {
                    Id = n.Id,
                    ApplicationId = n.ApplicationId,
                    Note = n.Note,
                    CreatedAt = n.CreatedAt
                }).ToList()
            };
        }
    }
}
