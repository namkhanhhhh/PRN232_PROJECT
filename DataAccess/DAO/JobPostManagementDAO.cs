using BusinessObjects.Models;
using BusinessObjects.DTOs.JobPost;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.DAO
{
    public class JobPostManagementDAO
    {
        private readonly SjobPContext _context;

        public JobPostManagementDAO(SjobPContext context)
        {
            _context = context;
        }

        public async Task<List<JobPost>> GetJobPostsByEmployerAsync(int employerId)
        {
            return await _context.JobPosts
                .Include(jp => jp.User)
                .Include(jp => jp.Applications)
                .Include(jp => jp.JobPostCategories)
                    .ThenInclude(jpc => jpc.Category)
                .Where(jp => jp.UserId == employerId)
                .OrderByDescending(jp => jp.CreatedAt)
                .ToListAsync();
        }

        public async Task<JobPost?> GetJobPostByIdAsync(int id)
        {
            return await _context.JobPosts
                .Include(jp => jp.User)
                .Include(jp => jp.Applications)
                .Include(jp => jp.JobPostCategories)
                    .ThenInclude(jpc => jpc.Category)
                .FirstOrDefaultAsync(jp => jp.Id == id);
        }

        public async Task<bool> CreateJobPostAsync(JobPost jobPost)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                jobPost.Status = "active";
                jobPost.CreatedAt = DateTime.Now;
                jobPost.ViewCount = 0;
                jobPost.IsFeatured = false;

                _context.JobPosts.Add(jobPost);
                await _context.SaveChangesAsync();

                // Add categories if provided
                if (jobPost.JobPostCategories?.Any() == true)
                {
                    foreach (var category in jobPost.JobPostCategories)
                    {
                        category.JobPostId = jobPost.Id;
                        category.CreatedAt = DateTime.Now;
                    }
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<bool> UpdateJobPostAsync(JobPost jobPost)
        {
            try
            {
                _context.JobPosts.Update(jobPost);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateJobPostWithCategoriesAsync(JobPostUpdateDto jobPostDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var existingJobPost = await _context.JobPosts
                    .Include(jp => jp.JobPostCategories)
                    .FirstOrDefaultAsync(jp => jp.Id == jobPostDto.Id);

                if (existingJobPost == null) return false;

                // Update job post properties
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

                // Update categories
                if (jobPostDto.CategoryIds?.Any() == true)
                {
                    // Remove existing categories
                    _context.JobPostCategories.RemoveRange(existingJobPost.JobPostCategories);

                    // Add new categories
                    foreach (var categoryId in jobPostDto.CategoryIds)
                    {
                        existingJobPost.JobPostCategories.Add(new JobPostCategory
                        {
                            JobPostId = jobPostDto.Id,
                            CategoryId = categoryId,
                            CreatedAt = DateTime.Now
                        });
                    }
                }
                else
                {
                    // Remove all categories if none selected
                    _context.JobPostCategories.RemoveRange(existingJobPost.JobPostCategories);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<bool> DeleteJobPostAsync(int id)
        {
            try
            {
                var jobPost = await _context.JobPosts.FindAsync(id);
                if (jobPost != null)
                {
                    jobPost.Status = "inactive";
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }


        public async Task<bool> ActivateJobPostAsync(int id)
        {
            try
            {
                var jobPost = await _context.JobPosts.FindAsync(id);
                if (jobPost != null)
                {
                    jobPost.Status = "active";
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ToggleJobPostStatusAsync(int id)
        {
            try
            {
                var jobPost = await _context.JobPosts.FindAsync(id);
                if (jobPost != null)
                {
                    jobPost.Status = jobPost.Status == "active" ? "inactive" : "active";
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> AddCreditTransactionAsync(int userId, decimal amount, string transactionType, string description)
        {
            try
            {
                var userCredit = await _context.UserCredits.FirstOrDefaultAsync(uc => uc.UserId == userId);
                if (userCredit == null)
                {
                    userCredit = new UserCredit
                    {
                        UserId = userId,
                        Balance = 0,
                        LastUpdated = DateTime.Now
                    };
                    _context.UserCredits.Add(userCredit);
                }

                var newBalance = userCredit.Balance + amount;
                userCredit.Balance = newBalance;
                userCredit.LastUpdated = DateTime.Now;

                var transaction = new CreditTransaction
                {
                    UserId = userId,
                    Amount = amount,
                    TransactionType = transactionType,
                    BalanceAfter = newBalance,
                    Description = description,
                    CreatedAt = DateTime.Now
                };

                _context.CreditTransactions.Add(transaction);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<JobCategory>> GetJobCategoriesAsync()
        {
            return await _context.JobCategories
                .Include(c => c.InverseParent)
                .OrderBy(c => c.ParentId)
                .ThenBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<UserPostCredit?> GetUserPostCreditsAsync(int userId)
        {
            return await _context.UserPostCredits
                .FirstOrDefaultAsync(upc => upc.UserId == userId);
        }

        public async Task<bool> DeductPostCreditAsync(int userId, string postType)
        {
            try
            {
                var userPostCredit = await _context.UserPostCredits
                    .FirstOrDefaultAsync(upc => upc.UserId == userId);

                if (userPostCredit == null)
                {
                    return false;
                }

                switch (postType.ToLower())
                {
                    case "silver":
                        if (userPostCredit.SilverPostsAvailable <= 0) return false;
                        userPostCredit.SilverPostsAvailable--;
                        break;
                    case "gold":
                        if (userPostCredit.GoldPostsAvailable <= 0) return false;
                        userPostCredit.GoldPostsAvailable--;
                        break;
                    case "diamond":
                        if (userPostCredit.DiamondPostsAvailable <= 0) return false;
                        userPostCredit.DiamondPostsAvailable--;
                        break;
                    default:
                        return false;
                }

                userPostCredit.LastUpdated = DateTime.Now;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
