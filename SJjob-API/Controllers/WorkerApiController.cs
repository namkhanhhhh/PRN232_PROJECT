using BusinessObjects.DTOs;
using BusinessObjects.DTOs.Authen;
using BusinessObjects.DTOs.Worker;
using BusinessObjects.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository.Interfaces;
using System.Security.Claims;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorkerApiController : ControllerBase
    {
        private readonly IJobPostRepository _jobPostRepository;
        private readonly IApplicationRepository _applicationRepository;
        private readonly IWishlistRepository _wishlistRepository;
        private readonly IWorkerVisitRepository _workerVisitRepository;
        private readonly IUserRepository _userRepository;

        public WorkerApiController(
            IJobPostRepository jobPostRepository,
            IApplicationRepository applicationRepository,
            IWishlistRepository wishlistRepository,
            IWorkerVisitRepository workerVisitRepository,
            IUserRepository userRepository)
        {
            _jobPostRepository = jobPostRepository;
            _applicationRepository = applicationRepository;
            _wishlistRepository = wishlistRepository;
            _workerVisitRepository = workerVisitRepository;
            _userRepository = userRepository;
        }

        [HttpPost("dashboard")]
        public async Task<ActionResult<ApiResponseDto<WorkerDashboardDto>>> GetDashboard([FromBody] JobSearchRequestDto request)
        {
            try
            {
                // Get featured categories
                var featuredCategories = await _jobPostRepository.GetFeaturedCategoriesAsync(8);

                // Get diamond posts
                var diamondPosts = await _jobPostRepository.GetDiamondPostsAsync(6);

                // Get most viewed posts
                var mostViewedPosts = await _jobPostRepository.GetMostViewedPostsAsync(6);

                // Get all posts with filtering
                var allPosts = await _jobPostRepository.GetJobPostsAsync(
                    request.Page,
                    request.PageSize,
                    request.Keyword,
                    request.Location,
                    request.JobType,
                    request.MinSalary,
                    request.MaxSalary,
                    request.CategoryId);

                var totalItems = await _jobPostRepository.GetJobPostsCountAsync(
                    request.Keyword,
                    request.Location,
                    request.JobType,
                    request.MinSalary,
                    request.MaxSalary,
                    request.CategoryId);

                var totalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize);

                // Get wishlist and application status if user is authenticated
                List<int> wishlistJobIds = new List<int>();
                List<int> appliedJobIds = new List<int>();

                if (request.UserId.HasValue)
                {
                    wishlistJobIds = await _wishlistRepository.GetWishlistJobIdsByUserAsync(request.UserId.Value);
                    var applications = await _applicationRepository.GetApplicationsByUserAsync(request.UserId.Value);
                    appliedJobIds = applications.Select(a => a.JobPostId).ToList();
                }

                // Convert to DTOs
                var dashboardDto = new WorkerDashboardDto
                {
                    FeaturedCategories = featuredCategories.Select(c => new JobCategoryDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Description = c.Description,
                        ParentId = c.ParentId,
                        JobCount = c.JobPostCategories?.Count ?? 0
                    }).ToList(),
                    DiamondPosts = ConvertToJobPostDtos(diamondPosts, wishlistJobIds, appliedJobIds),
                    MostViewedPosts = ConvertToJobPostDtos(mostViewedPosts, wishlistJobIds, appliedJobIds),
                    AllPosts = ConvertToJobPostDtos(allPosts, wishlistJobIds, appliedJobIds),
                    TotalPages = totalPages,
                    CurrentPage = request.Page,
                    TotalItems = totalItems
                };

                return Ok(new ApiResponseDto<WorkerDashboardDto>
                {
                    Success = true,
                    Message = "Lấy dữ liệu dashboard thành công",
                    Data = dashboardDto
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDto<WorkerDashboardDto>
                {
                    Success = false,
                    Message = "Đã xảy ra lỗi khi lấy dữ liệu dashboard"
                });
            }
        }

        [HttpPost("search")]
        public async Task<ActionResult<PaginatedResponseDto<JobPostDto>>> SearchJobs([FromBody] JobSearchRequestDto request)
        {
            try
            {
                var jobs = await _jobPostRepository.GetJobPostsAsync(
                    request.Page,
                    request.PageSize,
                    request.Keyword,
                    request.Location,
                    request.JobType,
                    request.MinSalary,
                    request.MaxSalary,
                    request.CategoryId);

                var totalItems = await _jobPostRepository.GetJobPostsCountAsync(
                    request.Keyword,
                    request.Location,
                    request.JobType,
                    request.MinSalary,
                    request.MaxSalary,
                    request.CategoryId);

                var totalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize);

                // Get wishlist and application status if user is authenticated
                List<int> wishlistJobIds = new List<int>();
                List<int> appliedJobIds = new List<int>();

                if (request.UserId.HasValue)
                {
                    wishlistJobIds = await _wishlistRepository.GetWishlistJobIdsByUserAsync(request.UserId.Value);
                    var applications = await _applicationRepository.GetApplicationsByUserAsync(request.UserId.Value);
                    appliedJobIds = applications.Select(a => a.JobPostId).ToList();
                }

                var jobDtos = ConvertToJobPostDtos(jobs, wishlistJobIds, appliedJobIds);

                return Ok(new PaginatedResponseDto<JobPostDto>
                {
                    Success = true,
                    Message = "Tìm kiếm công việc thành công",
                    Items = jobDtos,
                    TotalPages = totalPages,
                    CurrentPage = request.Page,
                    TotalItems = totalItems
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new PaginatedResponseDto<JobPostDto>
                {
                    Success = false,
                    Message = "Đã xảy ra lỗi khi tìm kiếm công việc"
                });
            }
        }

        [HttpGet("job/{id}")]
        public async Task<ActionResult<ApiResponseDto<JobPostDto>>> GetJobDetails(int id, [FromQuery] int? userId = null)
        {
            try
            {
                var jobPost = await _jobPostRepository.GetJobPostByIdAsync(id);
                if (jobPost == null)
                {
                    return NotFound(new ApiResponseDto<JobPostDto>
                    {
                        Success = false,
                        Message = "Không tìm thấy công việc"
                    });
                }

                // Record visit and update view count if user is authenticated
                if (userId.HasValue)
                {
                    var hasVisited = await _workerVisitRepository.HasUserVisitedJobAsync(userId.Value, id);
                    if (!hasVisited)
                    {
                        await _workerVisitRepository.RecordVisitAsync(new WorkerVisit
                        {
                            JobPostId = id,
                            UserId = userId.Value,
                            VisitTime = DateTime.Now,
                            IsFirstView = true
                        });
                        await _jobPostRepository.UpdateViewCountAsync(id);
                    }
                    else
                    {
                        await _workerVisitRepository.RecordVisitAsync(new WorkerVisit
                        {
                            JobPostId = id,
                            UserId = userId.Value,
                            VisitTime = DateTime.Now,
                            IsFirstView = false
                        });
                    }
                }

                // Get related jobs
                var categoryIds = jobPost.JobPostCategories.Select(pc => pc.CategoryId).ToList();
                var relatedJobs = await _jobPostRepository.GetRelatedJobsAsync(id, categoryIds, 4);

                // Check wishlist and application status
                bool isInWishlist = false;
                bool hasApplied = false;

                if (userId.HasValue)
                {
                    isInWishlist = await _wishlistRepository.IsJobInWishlistAsync(userId.Value, id);
                    hasApplied = await _applicationRepository.HasUserAppliedAsync(userId.Value, id);
                }

                var jobDto = ConvertToJobPostDto(jobPost, isInWishlist, hasApplied);
                jobDto.RelatedJobs = ConvertToJobPostDtos(relatedJobs, new List<int>(), new List<int>());

                return Ok(new ApiResponseDto<JobPostDto>
                {
                    Success = true,
                    Message = "Lấy chi tiết công việc thành công",
                    Data = jobDto
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDto<JobPostDto>
                {
                    Success = false,
                    Message = "Đã xảy ra lỗi khi lấy chi tiết công việc"
                });
            }
        }

        [HttpGet("category/{categoryId}")]
        public async Task<ActionResult<PaginatedResponseDto<JobPostDto>>> GetJobsByCategory(int categoryId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] int? userId = null)
        {
            try
            {
                var jobs = await _jobPostRepository.GetJobsByCategoryAsync(categoryId, page, pageSize);
                var totalItems = await _jobPostRepository.GetJobsByCategoryCountAsync(categoryId);
                var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

                // Get wishlist and application status if user is authenticated
                List<int> wishlistJobIds = new List<int>();
                List<int> appliedJobIds = new List<int>();

                if (userId.HasValue)
                {
                    wishlistJobIds = await _wishlistRepository.GetWishlistJobIdsByUserAsync(userId.Value);
                    var applications = await _applicationRepository.GetApplicationsByUserAsync(userId.Value);
                    appliedJobIds = applications.Select(a => a.JobPostId).ToList();
                }

                var jobDtos = ConvertToJobPostDtos(jobs, wishlistJobIds, appliedJobIds);

                return Ok(new PaginatedResponseDto<JobPostDto>
                {
                    Success = true,
                    Message = "Lấy danh sách công việc theo danh mục thành công",
                    Items = jobDtos,
                    TotalPages = totalPages,
                    CurrentPage = page,
                    TotalItems = totalItems
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new PaginatedResponseDto<JobPostDto>
                {
                    Success = false,
                    Message = "Đã xảy ra lỗi khi lấy danh sách công việc theo danh mục"
                });
            }
        }

        [HttpPost("apply")]
        [Authorize(Roles = "Worker")]
        public async Task<ActionResult<ApiResponseDto>> ApplyJob([FromBody] int jobId)
        {
            try
            {
                var userId = GetCurrentUserId();

                // Check if already applied
                var hasApplied = await _applicationRepository.HasUserAppliedAsync(userId, jobId);
                if (hasApplied)
                {
                    return BadRequest(new ApiResponseDto
                    {
                        Success = false,
                        Message = "Bạn đã ứng tuyển công việc này rồi"
                    });
                }

                // Get job post details
                var jobPost = await _jobPostRepository.GetJobPostByIdAsync(jobId);
                if (jobPost == null)
                {
                    return NotFound(new ApiResponseDto
                    {
                        Success = false,
                        Message = "Không tìm thấy công việc"
                    });
                }

                // Create application
                var application = new Application
                {
                    JobPostId = jobId,
                    UserId = userId,
                    Status = "pending",
                    CreatedAt = DateTime.Now
                };

                await _applicationRepository.CreateApplicationAsync(application);

                return Ok(new ApiResponseDto
                {
                    Success = true,
                    Message = "Ứng tuyển thành công"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDto
                {
                    Success = false,
                    Message = "Đã xảy ra lỗi khi ứng tuyển"
                });
            }
        }

        [HttpGet("applications")]
        [Authorize(Roles = "Worker")]
        public async Task<ActionResult<ApiResponseDto<List<ApplicationDto>>>> GetMyApplications()
        {
            try
            {
                var userId = GetCurrentUserId();
                var applications = await _applicationRepository.GetApplicationsByUserAsync(userId);

                var applicationDtos = applications.Select(a => new ApplicationDto
                {
                    Id = a.Id,
                    JobPostId = a.JobPostId,
                    UserId = a.UserId,
                    Status = a.Status,
                    CreatedAt = a.CreatedAt,
                    JobTitle = a.JobPost.Title,
                    JobLocation = a.JobPost.Location,
                    JobType = a.JobPost.JobType,
                    SalaryMin = a.JobPost.SalaryMin,
                    SalaryMax = a.JobPost.SalaryMax,
                    EmployerName = a.JobPost.User.Username,
                    EmployerAvatar = a.JobPost.User.Avatar
                }).ToList();

                return Ok(new ApiResponseDto<List<ApplicationDto>>
                {
                    Success = true,
                    Message = "Lấy danh sách đơn ứng tuyển thành công",
                    Data = applicationDtos
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDto<List<ApplicationDto>>
                {
                    Success = false,
                    Message = "Đã xảy ra lỗi khi lấy danh sách đơn ứng tuyển"
                });
            }
        }

        [HttpGet("applications/count")]
        [Authorize(Roles = "Worker")]
        public async Task<ActionResult<ApiResponseDto<int>>> GetApplicationsCount()
        {
            try
            {
                var userId = GetCurrentUserId();
                var applications = await _applicationRepository.GetApplicationsByUserAsync(userId);

                return Ok(new ApiResponseDto<int>
                {
                    Success = true,
                    Message = "Lấy số lượng đơn ứng tuyển thành công",
                    Data = applications.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDto<int>
                {
                    Success = false,
                    Message = "Đã xảy ra lỗi khi lấy số lượng đơn ứng tuyển"
                });
            }
        }

        [HttpGet("wishlist")]
        [Authorize(Roles = "Worker")]
        public async Task<ActionResult<ApiResponseDto<List<WishlistDto>>>> GetWishlist()
        {
            try
            {
                var userId = GetCurrentUserId();
                var wishlistItems = await _wishlistRepository.GetWishlistByUserAsync(userId);

                var wishlistDtos = wishlistItems.Select(w => new WishlistDto
                {
                    Id = w.Id,
                    JobPostId = w.JobPostId,
                    UserId = w.UserId,
                    CreatedAt = w.CreatedAt,
                    JobPost = ConvertToJobPostDto(w.JobPost, true, false)
                }).ToList();

                return Ok(new ApiResponseDto<List<WishlistDto>>
                {
                    Success = true,
                    Message = "Lấy danh sách yêu thích thành công",
                    Data = wishlistDtos
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDto<List<WishlistDto>>
                {
                    Success = false,
                    Message = "Đã xảy ra lỗi khi lấy danh sách yêu thích"
                });
            }
        }

        [HttpPost("wishlist/toggle")]
        [Authorize(Roles = "Worker")]
        public async Task<ActionResult<ApiResponseDto<bool>>> ToggleWishlist([FromBody] int jobId)
        {
            try
            {
                var userId = GetCurrentUserId();

                var isInWishlist = await _wishlistRepository.IsJobInWishlistAsync(userId, jobId);

                if (isInWishlist)
                {
                    await _wishlistRepository.RemoveFromWishlistAsync(userId, jobId);
                    return Ok(new ApiResponseDto<bool>
                    {
                        Success = true,
                        Message = "Đã xóa khỏi danh sách yêu thích",
                        Data = false
                    });
                }
                else
                {
                    var wishlistItem = new Wishlist
                    {
                        UserId = userId,
                        JobPostId = jobId,
                        CreatedAt = DateTime.Now
                    };

                    await _wishlistRepository.AddToWishlistAsync(wishlistItem);
                    return Ok(new ApiResponseDto<bool>
                    {
                        Success = true,
                        Message = "Đã thêm vào danh sách yêu thích",
                        Data = true
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Đã xảy ra lỗi khi cập nhật danh sách yêu thích"
                });
            }
        }

        [HttpGet("wishlist/count")]
        [Authorize(Roles = "Worker")]
        public async Task<ActionResult<ApiResponseDto<int>>> GetWishlistCount()
        {
            try
            {
                var userId = GetCurrentUserId();
                var wishlistItems = await _wishlistRepository.GetWishlistByUserAsync(userId);

                return Ok(new ApiResponseDto<int>
                {
                    Success = true,
                    Message = "Lấy số lượng wishlist thành công",
                    Data = wishlistItems.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDto<int>
                {
                    Success = false,
                    Message = "Đã xảy ra lỗi khi lấy số lượng wishlist"
                });
            }
        }

        [HttpGet("employer/{employerId}")]
        public async Task<ActionResult<ApiResponseDto<UserDto>>> GetEmployerProfile(int employerId, [FromQuery] int? userId = null)
        {
            try
            {
                var employer = await _userRepository.GetUserByIdAsync(employerId);
                if (employer == null)
                {
                    return NotFound(new ApiResponseDto<UserDto>
                    {
                        Success = false,
                        Message = "Không tìm thấy nhà tuyển dụng"
                    });
                }

                // Get employer's job posts
                var jobPosts = await _jobPostRepository.GetJobsByEmployerAsync(employerId);

                // Get profile view count
                var profileViewCount = await _workerVisitRepository.GetProfileViewCountAsync(employerId);

                // Record visit if user is authenticated
                if (userId.HasValue && jobPosts.Any())
                {
                    await _workerVisitRepository.RecordVisitAsync(new WorkerVisit
                    {
                        JobPostId = jobPosts.First().Id,
                        UserId = userId.Value,
                        VisitTime = DateTime.Now,
                        IsFirstView = false
                    });
                }

                // Get wishlist job IDs if user is authenticated
                List<int> wishlistJobIds = new List<int>();
                if (userId.HasValue)
                {
                    wishlistJobIds = await _wishlistRepository.GetWishlistJobIdsByUserAsync(userId.Value);
                }

                // Get user details
                var userDetail = await _userRepository.GetUserDetailByUserIdAsync(employerId);

                var employerDto = new UserDto
                {
                    Id = employer.Id,
                    Username = employer.Username,
                    Email = employer.Email,
                    Avatar = employer.Avatar,
                    Status = employer.Status,
                    RoleName = employer.Role?.Name ?? "",
                    FirstName = userDetail?.FirstName,
                    LastName = userDetail?.LastName,
                    PhoneNumber = userDetail?.PhoneNumber,
                    Address = userDetail?.Address,
                    CreatedAt = userDetail?.CreatedAt,
                    Bio = userDetail?.Bio,
                    TotalJobPosts = jobPosts.Count,
                    ProfileViewCount = profileViewCount,
                    JobPosts = ConvertToJobPostDtos(jobPosts, wishlistJobIds, new List<int>()),
                    UserDetail = userDetail != null ? new UserDetailDto
                    {
                        Id = userDetail.Id,
                        UserId = userDetail.UserId,
                        FirstName = userDetail.FirstName,
                        LastName = userDetail.LastName,
                        PhoneNumber = userDetail.PhoneNumber,
                        Address = userDetail.Address,
                        Bio = userDetail.Bio,
                        CreatedAt = userDetail.CreatedAt
                    } : new UserDetailDto()
                };

                return Ok(new ApiResponseDto<UserDto>
                {
                    Success = true,
                    Message = "Lấy thông tin nhà tuyển dụng thành công",
                    Data = employerDto
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDto<UserDto>
                {
                    Success = false,
                    Message = "Đã xảy ra lỗi khi lấy thông tin nhà tuyển dụng"
                });
            }
        }

        [HttpGet("profile")]
        [Authorize(Roles = "Worker")]
        public async Task<ActionResult<ApiResponseDto<UserDto>>> GetProfile()
        {
            try
            {
                var userId = GetCurrentUserId();
                var user = await _userRepository.GetUserByIdAsync(userId);

                if (user == null)
                {
                    return NotFound(new ApiResponseDto<UserDto>
                    {
                        Success = false,
                        Message = "Không tìm thấy thông tin người dùng"
                    });
                }

                var userDetail = await _userRepository.GetUserDetailByUserIdAsync(userId);

                var userDto = new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    Avatar = user.Avatar,
                    Status = user.Status,
                    RoleName = user.Role?.Name ?? "",
                    FirstName = userDetail?.FirstName,
                    LastName = userDetail?.LastName,
                    PhoneNumber = userDetail?.PhoneNumber,
                    Address = userDetail?.Address,
                    CreatedAt = userDetail?.CreatedAt,
                    UserDetail = userDetail != null ? new UserDetailDto
                    {
                        Id = userDetail.Id,
                        UserId = userDetail.UserId,
                        FirstName = userDetail.FirstName,
                        LastName = userDetail.LastName,
                        PhoneNumber = userDetail.PhoneNumber,
                        Address = userDetail.Address,
                        Bio = userDetail.Bio,
                        Skills = userDetail.Skills,
                        Education = userDetail.Education,
                        ExperienceYears = userDetail.ExperienceYears,
                        DesiredPosition = userDetail.DesiredPosition,
                        DesiredSalary = userDetail.DesiredSalary,
                        DesiredLocation = userDetail.DesiredLocation,
                        Availability = userDetail.Availability,
                        Headline = userDetail.Headline,
                        CreatedAt = userDetail.CreatedAt
                    } : new UserDetailDto()
                };

                return Ok(new ApiResponseDto<UserDto>
                {
                    Success = true,
                    Message = "Lấy thông tin hồ sơ thành công",
                    Data = userDto
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDto<UserDto>
                {
                    Success = false,
                    Message = "Đã xảy ra lỗi khi lấy thông tin hồ sơ"
                });
            }
        }

        [HttpPost("profile/update")]
        [Authorize(Roles = "Worker")]
        public async Task<ActionResult<ApiResponseDto>> UpdateProfile([FromBody] UpdateProfileDto request)
        {
            try
            {
                var userId = GetCurrentUserId();

                // Log để debug
                Console.WriteLine($"Updating profile for user ID: {userId}");
                Console.WriteLine($"Request data: {System.Text.Json.JsonSerializer.Serialize(request)}");

                var user = await _userRepository.GetUserByIdAsync(userId);

                if (user == null)
                {
                    return NotFound(new ApiResponseDto
                    {
                        Success = false,
                        Message = "Không tìm thấy thông tin người dùng"
                    });
                }

                // Update user basic info
                user.Email = request.Email;
                await _userRepository.UpdateUserAsync(user);

                // Update or create user detail
                var userDetail = await _userRepository.GetUserDetailByUserIdAsync(userId);

                if (userDetail == null)
                {
                    userDetail = new UserDetail
                    {
                        UserId = userId,
                        FirstName = request.FirstName ?? "",
                        LastName = request.LastName ?? "",
                        PhoneNumber = request.PhoneNumber,
                        Address = request.Address,
                        Bio = request.Bio,
                        Skills = request.Skills,
                        Education = request.Education,
                        ExperienceYears = request.ExperienceYears,
                        DesiredPosition = request.DesiredPosition,
                        DesiredSalary = request.DesiredSalary,
                        DesiredLocation = request.DesiredLocation,
                        Availability = request.Availability,
                        Headline = request.Headline,
                        CreatedAt = DateTime.Now
                    };

                    await _userRepository.CreateUserDetailAsync(userDetail);
                    Console.WriteLine("Created new user detail");
                }
                else
                {
                    userDetail.FirstName = request.FirstName ?? userDetail.FirstName;
                    userDetail.LastName = request.LastName ?? userDetail.LastName;
                    userDetail.PhoneNumber = request.PhoneNumber;
                    userDetail.Address = request.Address;
                    userDetail.Bio = request.Bio;
                    userDetail.Skills = request.Skills;
                    userDetail.Education = request.Education;
                    userDetail.ExperienceYears = request.ExperienceYears;
                    userDetail.DesiredPosition = request.DesiredPosition;
                    userDetail.DesiredSalary = request.DesiredSalary;
                    userDetail.DesiredLocation = request.DesiredLocation;
                    userDetail.Availability = request.Availability;
                    userDetail.Headline = request.Headline;

                    await _userRepository.UpdateUserDetailAsync(userDetail);
                    Console.WriteLine("Updated existing user detail");
                }

                // Ensure changes are saved
                await _userRepository.SaveChangesAsync();

                return Ok(new ApiResponseDto
                {
                    Success = true,
                    Message = "Cập nhật hồ sơ thành công"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating profile: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                return StatusCode(500, new ApiResponseDto
                {
                    Success = false,
                    Message = $"Đã xảy ra lỗi khi cập nhật hồ sơ: {ex.Message}"
                });
            }
        }

        [HttpPost("change-password")]
        [Authorize(Roles = "Worker")]
        public async Task<ActionResult<ApiResponseDto>> ChangePassword([FromBody] ChangePasswordDto request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var user = await _userRepository.GetUserByIdAsync(userId);

                if (user == null)
                {
                    return NotFound(new ApiResponseDto
                    {
                        Success = false,
                        Message = "Không tìm thấy thông tin người dùng"
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

                // Hash new password and update
                user.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                await _userRepository.UpdateUserAsync(user);

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

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
        }

        private List<JobPostDto> ConvertToJobPostDtos(List<JobPost> jobPosts, List<int> wishlistJobIds, List<int> appliedJobIds)
        {
            return jobPosts.Select(jp => ConvertToJobPostDto(jp, wishlistJobIds.Contains(jp.Id), appliedJobIds.Contains(jp.Id))).ToList();
        }

        private JobPostDto ConvertToJobPostDto(JobPost jobPost, bool isInWishlist, bool hasApplied)
        {
            return new JobPostDto
            {
                Id = jobPost.Id,
                Title = jobPost.Title,
                Description = jobPost.Description,
                Requirements = jobPost.Requirements,
                Location = jobPost.Location,
                JobType = jobPost.JobType,
                SalaryMin = jobPost.SalaryMin,
                SalaryMax = jobPost.SalaryMax,
                PostType = jobPost.PostType,
                PriorityLevel = jobPost.PriorityLevel,
                PushedToTopUntil = jobPost.PushedToTopUntil,
                Status = jobPost.Status,
                ViewCount = jobPost.ViewCount,
                CreatedAt = jobPost.CreatedAt,
                Deadline = jobPost.Deadline,
                UserId = jobPost.UserId,
                EmployerName = jobPost.User?.Username ?? "",
                EmployerAvatar = jobPost.User?.Avatar,
                CompanyName = jobPost.User?.UserDetails?.FirstOrDefault()?.FirstName + " " + jobPost.User?.UserDetails?.FirstOrDefault()?.LastName,
                Categories = jobPost.JobPostCategories?.Select(pc => new JobCategoryDto
                {
                    Id = pc.Category.Id,
                    Name = pc.Category.Name,
                    Description = pc.Category.Description,
                    ParentId = pc.Category.ParentId
                }).ToList() ?? new List<JobCategoryDto>(),
                IsInWishlist = isInWishlist,
                HasApplied = hasApplied
            };
        }
    }
}
