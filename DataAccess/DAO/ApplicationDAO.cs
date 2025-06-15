using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.DAO
{
    public class ApplicationDAO
    {
        private readonly SjobPContext _context;

        public ApplicationDAO(SjobPContext context)
        {
            _context = context;
        }
        public async Task<List<Application>> GetApplicationsByUserAsync(int userId)
        {
            return await _context.Applications
                .Include(a => a.JobPost)
                .ThenInclude(p => p.User)
                .ThenInclude(u => u.UserDetails)
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }
        public async Task<bool> HasUserAppliedAsync(int userId, int jobPostId)
        {
            return await _context.Applications
                .AnyAsync(a => a.UserId == userId && a.JobPostId == jobPostId);
        }

        public async Task<Application> CreateApplicationAsync(Application application)
        {
            _context.Applications.Add(application);
            await _context.SaveChangesAsync();
            return application;
        }

        public async Task<Application?> GetApplicationByIdAsync(int id)
        {
            return await _context.Applications
                .Include(a => a.JobPost)
                .ThenInclude(p => p.User)
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<bool> UpdateApplicationStatusAsync(int applicationId, string status)
        {
            var application = await _context.Applications.FindAsync(applicationId);
            if (application == null) return false;

            application.Status = status;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Application>> GetApplicationsByJobPostAsync(int jobPostId)
        {
            return await _context.Applications
                .Include(a => a.User)
                .ThenInclude(u => u.UserDetails)
                .Where(a => a.JobPostId == jobPostId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }
    }
}
