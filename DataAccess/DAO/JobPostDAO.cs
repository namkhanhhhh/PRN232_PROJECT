using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.DAO
{
    public class JobPostDAO
    {
        private readonly SjobPContext _context;

        public JobPostDAO(SjobPContext context)
        {
            _context = context;
        }

        public async Task<List<JobPost>> GetJobPostsAsync(
            int page,
            int pageSize,
            string? keyword = null,
            string? location = null,
            string? jobType = null,
            decimal? minSalary = null,
            decimal? maxSalary = null,
            int? categoryId = null)
        {
            var query = _context.JobPosts
                .Include(p => p.User)
                .ThenInclude(u => u.UserDetails)
                .Include(p => p.JobPostCategories)
                .ThenInclude(pc => pc.Category)
                .Where(p => p.Status == "active")
                .AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(p =>
                    p.Title.Contains(keyword) ||
                    p.Description.Contains(keyword) ||
                    p.Requirements.Contains(keyword));
            }

            if (!string.IsNullOrEmpty(location))
            {
                query = query.Where(p => p.Location.Contains(location));
            }

            if (!string.IsNullOrEmpty(jobType))
            {
                query = query.Where(p => p.JobType == jobType);
            }

            if (minSalary.HasValue)
            {
                query = query.Where(p => p.SalaryMin >= minSalary);
            }

            if (maxSalary.HasValue)
            {
                query = query.Where(p => p.SalaryMax <= maxSalary);
            }

            if (categoryId.HasValue && categoryId > 0)
            {
                query = query.Where(p => p.JobPostCategories.Any(pc => pc.CategoryId == categoryId));
            }

            query = query.OrderByDescending(p => p.PostType == "diamond" ? 3 : p.PostType == "gold" ? 2 : 1)
                .ThenByDescending(p => p.PriorityLevel)
                .ThenByDescending(p => p.PushedToTopUntil)
                .ThenByDescending(p => p.CreatedAt);

            return await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetJobPostsCountAsync(
            string? keyword = null,
            string? location = null,
            string? jobType = null,
            decimal? minSalary = null,
            decimal? maxSalary = null,
            int? categoryId = null)
        {
            var query = _context.JobPosts
                .Where(p => p.Status == "active")
                .AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(p =>
                    p.Title.Contains(keyword) ||
                    p.Description.Contains(keyword) ||
                    p.Requirements.Contains(keyword));
            }

            if (!string.IsNullOrEmpty(location))
            {
                query = query.Where(p => p.Location.Contains(location));
            }

            if (!string.IsNullOrEmpty(jobType))
            {
                query = query.Where(p => p.JobType == jobType);
            }

            if (minSalary.HasValue)
            {
                query = query.Where(p => p.SalaryMin >= minSalary);
            }

            if (maxSalary.HasValue)
            {
                query = query.Where(p => p.SalaryMax <= maxSalary);
            }

            if (categoryId.HasValue && categoryId > 0)
            {
                query = query.Where(p => p.JobPostCategories.Any(pc => pc.CategoryId == categoryId));
            }

            return await query.CountAsync();
        }

        // Get job post by ID
        public async Task<JobPost?> GetJobPostByIdAsync(int id)
        {
            return await _context.JobPosts
                .Include(p => p.User)
                .ThenInclude(u => u.UserDetails)
                .Include(p => p.JobPostCategories)
                .ThenInclude(pc => pc.Category)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        // Get featured categories
        public async Task<List<JobCategory>> GetFeaturedCategoriesAsync(int count = 8)
        {
            return await _context.JobCategories
                .Where(c => c.ParentId == null)
                .Take(count)
                .ToListAsync();
        }

        // Get diamond posts
        public async Task<List<JobPost>> GetDiamondPostsAsync(int count = 6)
        {
            return await _context.JobPosts
                .Include(p => p.User)
                .ThenInclude(u => u.UserDetails)
                .Include(p => p.JobPostCategories)
                .ThenInclude(pc => pc.Category)
                .Where(p => p.PostType == "diamond" && p.Status == "active")
                .OrderByDescending(p => p.PriorityLevel)
                .ThenByDescending(p => p.PushedToTopUntil)
                .ThenByDescending(p => p.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        // Get most viewed posts
        public async Task<List<JobPost>> GetMostViewedPostsAsync(int count = 6)
        {
            return await _context.JobPosts
                .Include(p => p.User)
                .ThenInclude(u => u.UserDetails)
                .Include(p => p.JobPostCategories)
                .ThenInclude(pc => pc.Category)
                .Where(p => p.Status == "active")
                .OrderByDescending(p => p.ViewCount)
                .Take(count)
                .ToListAsync();
        }

        // Get related jobs
        public async Task<List<JobPost>> GetRelatedJobsAsync(int jobId, List<int> categoryIds, int count = 4)
        {
            return await _context.JobPosts
                .Include(p => p.User)
                .ThenInclude(u => u.UserDetails)
                .Include(p => p.JobPostCategories)
                .ThenInclude(pc => pc.Category)
                .Where(p => p.Id != jobId && p.Status == "active" &&
                       p.JobPostCategories.Any(pc => categoryIds.Contains(pc.CategoryId)))
                .OrderByDescending(p => p.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        // Get jobs by category
        public async Task<List<JobPost>> GetJobsByCategoryAsync(int categoryId, int page, int pageSize)
        {
            return await _context.JobPosts
                .Include(p => p.User)
                .ThenInclude(u => u.UserDetails)
                .Include(p => p.JobPostCategories)
                .ThenInclude(pc => pc.Category)
                .Where(p => p.Status == "active" &&
                       p.JobPostCategories.Any(pc => pc.CategoryId == categoryId))
                .OrderByDescending(p => p.PostType == "diamond" ? 3 : p.PostType == "gold" ? 2 : 1)
                .ThenByDescending(p => p.PriorityLevel)
                .ThenByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetJobsByCategoryCountAsync(int categoryId)
        {
            return await _context.JobPosts
                .Where(p => p.Status == "active" &&
                       p.JobPostCategories.Any(pc => pc.CategoryId == categoryId))
                .CountAsync();
        }

        // Update view count
        public async Task<bool> UpdateViewCountAsync(int jobId)
        {
            var jobPost = await _context.JobPosts.FindAsync(jobId);
            if (jobPost == null) return false;

            jobPost.ViewCount++;
            await _context.SaveChangesAsync();
            return true;
        }

        // Get jobs by employer
        public async Task<List<JobPost>> GetJobsByEmployerAsync(int employerId)
        {
            return await _context.JobPosts
                .Include(p => p.User)
                .ThenInclude(u => u.UserDetails)
                .Include(p => p.JobPostCategories)
                .ThenInclude(pc => pc.Category)
                .Where(p => p.UserId == employerId && p.Status == "active")
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }
    }
}
