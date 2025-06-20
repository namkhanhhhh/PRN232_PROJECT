using BusinessObjects.Models;
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
                .ThenInclude(u => u.UserDetails)
                .Include(jp => jp.JobPostCategories)
                .ThenInclude(jpc => jpc.Category)
                .Include(jp => jp.Applications)
                .Where(jp => jp.UserId == employerId)
                .OrderByDescending(jp => jp.CreatedAt)
                .ToListAsync();
        }

        public async Task<JobPost?> GetJobPostByIdAsync(int id)
        {
            return await _context.JobPosts
                .Include(jp => jp.User)
                .ThenInclude(u => u.UserDetails)
                .Include(jp => jp.JobPostCategories)
                .ThenInclude(jpc => jpc.Category)
                .Include(jp => jp.Applications)
                .FirstOrDefaultAsync(jp => jp.Id == id);
        }

        public async Task<bool> CreateJobPostAsync(JobPost jobPost)
        {
            try
            {
                jobPost.CreatedAt = DateTime.Now;
                jobPost.Status = "active";
                jobPost.ViewCount = 0;
                jobPost.PriorityLevel = 0;
                jobPost.IsFeatured = false;

                _context.JobPosts.Add(jobPost);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateJobPostAsync(JobPost jobPost)
        {
            try
            {
                var existingJobPost = await _context.JobPosts.FindAsync(jobPost.Id);
                if (existingJobPost == null) return false;

                existingJobPost.Title = jobPost.Title;
                existingJobPost.Description = jobPost.Description;
                existingJobPost.Requirements = jobPost.Requirements;
                existingJobPost.Benefits = jobPost.Benefits;
                existingJobPost.Location = jobPost.Location;
                existingJobPost.SalaryMin = jobPost.SalaryMin;
                existingJobPost.SalaryMax = jobPost.SalaryMax;
                existingJobPost.JobType = jobPost.JobType;
                existingJobPost.ExperienceLevel = jobPost.ExperienceLevel;
                existingJobPost.Deadline = jobPost.Deadline;
                existingJobPost.Status = jobPost.Status;
                existingJobPost.ImageMain = jobPost.ImageMain;
                existingJobPost.Image2 = jobPost.Image2;
                existingJobPost.Image3 = jobPost.Image3;
                existingJobPost.Image4 = jobPost.Image4;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteJobPostAsync(int id)
        {
            try
            {
                var jobPost = await _context.JobPosts.FindAsync(id);
                if (jobPost == null) return false;

                jobPost.Status = jobPost.Status?.ToLower() == "active" ? "inactive" : "active";
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> AddCreditTransactionAsync(int userId, decimal amount, string transactionType, string description)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Get current user credit balance
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

                var previousBalance = userCredit.Balance ?? 0;
                userCredit.Balance = previousBalance + amount;
                userCredit.LastUpdated = DateTime.Now;

                // Add credit transaction record
                var creditTransaction = new CreditTransaction
                {
                    UserId = userId,
                    Amount = amount,
                    TransactionType = transactionType,
                    Description = description,
                    BalanceAfter = userCredit.Balance ?? 0,
                    CreatedAt = DateTime.Now
                };

                _context.CreditTransactions.Add(creditTransaction);
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

        public async Task<List<JobCategory>> GetJobCategoriesAsync()
        {
            return await _context.JobCategories
                .Where(c => c.ParentId == null)
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
                    // Create default credits if not exists
                    userPostCredit = new UserPostCredit
                    {
                        UserId = userId,
                        SilverPostsAvailable = 0,
                        GoldPostsAvailable = 0,
                        DiamondPostsAvailable = 0,
                        AuthenLogoAvailable = 0,
                        PushToTopAvailable = 0,
                        LastUpdated = DateTime.Now
                    };
                    _context.UserPostCredits.Add(userPostCredit);
                }

                // Deduct based on post type
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
