using BusinessObjects.DTOs.Employer;
using BusinessObjects.Models;
using DataAccess.DAO;
using Repository.Interfaces;

namespace Repository.Implementations
{
    public class EmployerRepository : IEmployerRepository
    {
        private readonly EmployerDAO _employerDAO;

        public EmployerRepository(EmployerDAO employerDAO)
        {
            _employerDAO = employerDAO;
        }

        public async Task<EmployerProfileDto?> GetEmployerProfileAsync(int userId)
        {
            var user = await _employerDAO.GetEmployerWithDetailsAsync(userId);
            if (user == null) return null;

            // Get statistics using DAO
            var totalJobPosts = await _employerDAO.GetJobPostsCountAsync(userId);
            var totalApplications = await _employerDAO.GetApplicationsCountAsync(userId);
            var totalViews = await _employerDAO.GetTotalViewsAsync(userId);

            // Get post credits
            var userPostCredits = await _employerDAO.GetUserPostCreditsAsync(userId);

            // Get balance
            var userCredit = await _employerDAO.GetUserCreditAsync(userId);
            var balance = userCredit?.Balance ?? 0;

            var userDetail = user.UserDetails.FirstOrDefault();

            return new EmployerProfileDto
            {
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                Avatar = user.Avatar,
                FirstName = userDetail?.FirstName,
                LastName = userDetail?.LastName,
                PhoneNumber = userDetail?.PhoneNumber,
                Address = userDetail?.Address,
                JobPostsCount = totalJobPosts,
                ApplicationsCount = totalApplications,
                ViewsCount = totalViews,
                SilverPostsAvailable = userPostCredits?.SilverPostsAvailable ?? 0,
                GoldPostsAvailable = userPostCredits?.GoldPostsAvailable ?? 0,
                DiamondPostsAvailable = userPostCredits?.DiamondPostsAvailable ?? 0,
                AuthenLogoAvailable = userPostCredits?.AuthenLogoAvailable ?? 0,
                PushToTopAvailable = userPostCredits?.PushToTopAvailable ?? 0,
                Balance = balance,
                CreatedAt = userDetail?.CreatedAt
            };
        }

        public async Task<bool> UpdateEmployerProfileAsync(EditEmployerProfileDto dto)
        {
            var user = await _employerDAO.GetEmployerWithDetailsAsync(dto.UserId);
            if (user == null) return false;

            // Update user info
            user.Username = dto.Username;
            user.Email = dto.Email;
            if (!string.IsNullOrEmpty(dto.Avatar))
            {
                user.Avatar = dto.Avatar;
            }

            // Update user
            var userUpdateResult = await _employerDAO.UpdateUserAsync(user);
            if (!userUpdateResult) return false;

            // Update or create user details
            var userDetail = user.UserDetails.FirstOrDefault();
            if (userDetail == null)
            {
                userDetail = new UserDetail
                {
                    UserId = user.Id,
                    FirstName = dto.FirstName ?? "",
                    LastName = dto.LastName ?? "",
                    PhoneNumber = dto.PhoneNumber,
                    Address = dto.Address,
                    CreatedAt = DateTime.Now
                };
            }
            else
            {
                userDetail.FirstName = dto.FirstName ?? userDetail.FirstName;
                userDetail.LastName = dto.LastName ?? userDetail.LastName;
                userDetail.PhoneNumber = dto.PhoneNumber;
                userDetail.Address = dto.Address;
            }

            return await _employerDAO.UpdateUserDetailAsync(userDetail);
        }

        public async Task<EmployerHistoryDto> GetEmployerHistoryAsync(int userId)
        {
            // Get job post history using DAO
            var jobPosts = await _employerDAO.GetJobPostHistoryAsync(userId);
            var jobPostHistory = jobPosts.Select(jp => new JobPostHistoryDto
            {
                Id = jp.Id,
                Title = jp.Title,
                Description = jp.Description,
                SalaryMin = jp.SalaryMin,
                SalaryMax = jp.SalaryMax,
                Location = jp.Location,
                Status = jp.Status,
                PostType = jp.PostType,
                CreatedAt = jp.CreatedAt,
                ViewCount = jp.ViewCount,
                ApplicationCount = jp.Applications.Count,
                IsFeatured = jp.IsFeatured
            }).ToList();

            // Get service order history using DAO
            var serviceOrders = await _employerDAO.GetServiceOrderHistoryAsync(userId);
            var serviceOrderHistory = serviceOrders.Select(so => new ServiceOrderHistoryDto
            {
                Id = so.Id,
                ServiceName = so.Service?.Name ?? "Unknown Service",
                Price = so.Service?.Price ?? 0,
                Status = so.Status,
                CreatedAt = so.CreatedAt,
                StartDate = so.StartDate,
                EndDate = so.EndDate,
                DiamondPostsApplied = so.DiamondPostsApplied,
                GoldPostsApplied = so.GoldPostsApplied,
                SilverPostsApplied = so.SilverPostsApplied
            }).ToList();

            // Get credit transaction history using DAO
            var creditTransactions = await _employerDAO.GetCreditTransactionHistoryAsync(userId);
            var creditTransactionHistory = creditTransactions.Select(ct => new CreditTransactionHistoryDto
            {
                Id = ct.Id,
                Amount = ct.Amount,
                TransactionType = ct.TransactionType,
                BalanceAfter = ct.BalanceAfter,
                Description = ct.Description,
                CreatedAt = ct.CreatedAt
            }).ToList();

            return new EmployerHistoryDto
            {
                JobPosts = jobPostHistory,
                ServiceOrders = serviceOrderHistory,
                CreditTransactions = creditTransactionHistory
            };
        }

        public async Task<bool> UpdateEmployerAvatarAsync(int userId, string avatarPath)
        {
            return await _employerDAO.UpdateUserAvatarAsync(userId, avatarPath);
        }

        public async Task<decimal> GetEmployerBalanceAsync(int userId)
        {
            try
            {
                return await _employerDAO.GetUserBalanceAsync(userId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Repository error getting balance for user {userId}: {ex.Message}");
                return 0;
            }
        }
    }
}
