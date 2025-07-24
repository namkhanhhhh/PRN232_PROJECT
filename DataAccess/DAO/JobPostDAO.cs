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

        public async Task<List<JobPost>> GetJobPostsAsync(int page, int pageSize, string? keyword = null, string? location = null, string? jobType = null, decimal? minSalary = null, decimal? maxSalary = null, int? categoryId = null)
        {
            var query = _context.JobPosts
                .Include(jp => jp.User)
                .Include(jp => jp.JobPostCategories)
                    .ThenInclude(jpc => jpc.Category)
                .Where(jp => jp.Status == "active" && (jp.Deadline == null || jp.Deadline >= DateOnly.FromDateTime(DateTime.Today))); // Added deadline filter

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(jp => jp.Title.Contains(keyword) || jp.Description.Contains(keyword));
            }

            if (!string.IsNullOrEmpty(location))
            {
                query = query.Where(jp => jp.Location != null && jp.Location.Contains(location));
            }

            if (!string.IsNullOrEmpty(jobType))
            {
                query = query.Where(jp => jp.JobType == jobType);
            }

            if (minSalary.HasValue)
            {
                query = query.Where(jp => jp.SalaryMin >= minSalary || jp.SalaryMax >= minSalary);
            }

            if (maxSalary.HasValue)
            {
                query = query.Where(jp => jp.SalaryMax <= maxSalary || jp.SalaryMin <= maxSalary);
            }

            if (categoryId.HasValue)
            {
                query = query.Where(jp => jp.JobPostCategories.Any(jpc => jpc.CategoryId == categoryId));
            }

            return await query
                .OrderBy(jp => jp.PostType == "diamond" ? 1 : // Custom sorting for post_type
                               jp.PostType == "gold" ? 2 :
                               jp.PostType == "silver" ? 3 : 4)
                .ThenByDescending(jp => jp.CreatedAt) // Then by creation date
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetJobPostsCountAsync(string? keyword = null, string? location = null, string? jobType = null, decimal? minSalary = null, decimal? maxSalary = null, int? categoryId = null)
        {
            var query = _context.JobPosts.Where(jp => jp.Status == "active" && (jp.Deadline == null || jp.Deadline >= DateOnly.FromDateTime(DateTime.Today))); // Added deadline filter

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(jp => jp.Title.Contains(keyword) || jp.Description.Contains(keyword));
            }

            if (!string.IsNullOrEmpty(location))
            {
                query = query.Where(jp => jp.Location != null && jp.Location.Contains(location));
            }

            if (!string.IsNullOrEmpty(jobType))
            {
                query = query.Where(jp => jp.JobType == jobType);
            }

            if (minSalary.HasValue)
            {
                query = query.Where(jp => jp.SalaryMin >= minSalary || jp.SalaryMax >= minSalary);
            }

            if (maxSalary.HasValue)
            {
                query = query.Where(jp => jp.SalaryMax <= maxSalary || jp.SalaryMin <= maxSalary);
            }

            if (categoryId.HasValue)
            {
                query = query.Where(jp => jp.JobPostCategories.Any(jpc => jpc.CategoryId == categoryId));
            }

            return await query.CountAsync();
        }

        public async Task<JobPost?> GetJobPostByIdAsync(int id)
        {
            return await _context.JobPosts
                .Include(jp => jp.User)
                    .ThenInclude(u => u.UserDetails)
                .Include(jp => jp.JobPostCategories)
                    .ThenInclude(jpc => jpc.Category)
                .FirstOrDefaultAsync(jp => jp.Id == id && (jp.Deadline == null || jp.Deadline >= DateOnly.FromDateTime(DateTime.Today))); // Added deadline filter
        }

        public async Task<List<JobCategory>> GetFeaturedCategoriesAsync(int count = 8)
        {
            return await _context.JobCategories
                .Include(c => c.JobPostCategories)
                .Include(c => c.InverseParent)
                .Where(c => c.ParentId == null) // Only parent categories
                .OrderByDescending(c => c.JobPostCategories.Count)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<JobCategory>> GetCategoriesWithChildrenAsync()
        {
            return await _context.JobCategories
                .Include(c => c.InverseParent)
                    .ThenInclude(child => child.JobPostCategories)
                .Include(c => c.JobPostCategories)
                .Where(c => c.ParentId == null)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<List<JobPost>> GetDiamondPostsAsync(int count = 6)
        {
            return await _context.JobPosts
                .Include(jp => jp.User)
                .Include(jp => jp.JobPostCategories)
                    .ThenInclude(jpc => jpc.Category)
                .Where(jp => jp.Status == "active" && jp.PostType == "diamond" && (jp.Deadline == null || jp.Deadline >= DateOnly.FromDateTime(DateTime.Today))) // Added deadline filter
                .OrderByDescending(jp => jp.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<JobPost>> GetMostViewedPostsAsync(int count = 6)
        {
            return await _context.JobPosts
                .Include(jp => jp.User)
                .Include(jp => jp.JobPostCategories)
                    .ThenInclude(jpc => jpc.Category)
                .Where(jp => jp.Status == "active" && (jp.Deadline == null || jp.Deadline >= DateOnly.FromDateTime(DateTime.Today))) // Added deadline filter
                .OrderByDescending(jp => jp.ViewCount)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<JobPost>> GetAllJobsAsync(int page, int pageSize)
        {
            return await _context.JobPosts
                .Include(jp => jp.User)
                .Include(jp => jp.JobPostCategories)
                    .ThenInclude(jpc => jpc.Category)
                .Where(jp => jp.Status == "active" && (jp.Deadline == null || jp.Deadline >= DateOnly.FromDateTime(DateTime.Today))) // Added deadline filter
                .OrderBy(jp => jp.PostType == "diamond" ? 1 : // Custom sorting for post_type
                               jp.PostType == "gold" ? 2 :
                               jp.PostType == "silver" ? 3 : 4)
                .ThenByDescending(jp => jp.CreatedAt) // Then by creation date
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetAllJobsCountAsync()
        {
            return await _context.JobPosts
                .Where(jp => jp.Status == "active" && (jp.Deadline == null || jp.Deadline >= DateOnly.FromDateTime(DateTime.Today))) // Added deadline filter
                .CountAsync();
        }

        public async Task<List<JobPost>> GetRelatedJobsAsync(int jobId, List<int> categoryIds, int count = 4)
        {
            return await _context.JobPosts
                .Include(jp => jp.User)
                .Include(jp => jp.JobPostCategories)
                    .ThenInclude(jpc => jpc.Category)
                .Where(jp => jp.Id != jobId &&
                           jp.Status == "active" &&
                           (jp.Deadline == null || jp.Deadline >= DateOnly.FromDateTime(DateTime.Today)) && // Added deadline filter
                           jp.JobPostCategories.Any(jpc => categoryIds.Contains(jpc.CategoryId)))
                .OrderByDescending(jp => jp.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<JobPost>> GetJobsByCategoryAsync(int categoryId, int page, int pageSize)
        {
            // Get the category and its children
            var category = await _context.JobCategories
                .Include(c => c.InverseParent)
                .FirstOrDefaultAsync(c => c.Id == categoryId);

            var categoryIds = new List<int> { categoryId };
            if (category?.InverseParent != null)
            {
                categoryIds.AddRange(category.InverseParent.Select(c => c.Id));
            }

            return await _context.JobPosts
                .Include(jp => jp.User)
                .Include(jp => jp.JobPostCategories)
                    .ThenInclude(jpc => jpc.Category)
                .Where(jp => jp.Status == "active" &&
                           (jp.Deadline == null || jp.Deadline >= DateOnly.FromDateTime(DateTime.Today)) && // Added deadline filter
                           jp.JobPostCategories.Any(jpc => categoryIds.Contains(jpc.CategoryId)))
                .OrderBy(jp => jp.PostType == "diamond" ? 1 : // Custom sorting for post_type
                               jp.PostType == "gold" ? 2 :
                               jp.PostType == "silver" ? 3 : 4)
                .ThenByDescending(jp => jp.CreatedAt) // Then by creation date
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetJobsByCategoryCountAsync(int categoryId)
        {
            var category = await _context.JobCategories
                .Include(c => c.InverseParent)
                .FirstOrDefaultAsync(c => c.Id == categoryId);

            var categoryIds = new List<int> { categoryId };
            if (category?.InverseParent != null)
            {
                categoryIds.AddRange(category.InverseParent.Select(c => c.Id));
            }

            return await _context.JobPosts
                .Where(jp => jp.Status == "active" &&
                           (jp.Deadline == null || jp.Deadline >= DateOnly.FromDateTime(DateTime.Today)) && // Added deadline filter
                           jp.JobPostCategories.Any(jpc => categoryIds.Contains(jpc.CategoryId)))
                .CountAsync();
        }

        public async Task<bool> UpdateViewCountAsync(int jobId)
        {
            var jobPost = await _context.JobPosts.FindAsync(jobId);
            if (jobPost != null)
            {
                jobPost.ViewCount = (jobPost.ViewCount ?? 0) + 1;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<List<JobPost>> GetJobsByEmployerAsync(int employerId)
        {
            return await _context.JobPosts
                .Include(jp => jp.User)
                .Include(jp => jp.JobPostCategories)
                    .ThenInclude(jpc => jpc.Category)
                .Where(jp => jp.UserId == employerId && jp.Status == "active" && (jp.Deadline == null || jp.Deadline >= DateOnly.FromDateTime(DateTime.Today))) // Added deadline filter
                .OrderByDescending(jp => jp.CreatedAt)
                .ToListAsync();
        }
    }
}
