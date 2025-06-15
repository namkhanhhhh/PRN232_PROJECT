using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.DAO
{
    public class WorkerVisitDAO
    {
        private readonly SjobPContext _context;

        public WorkerVisitDAO(SjobPContext context)
        {
            _context = context;
        }

        // Check if user has visited job before
        public async Task<bool> HasUserVisitedJobAsync(int userId, int jobPostId)
        {
            return await _context.WorkerVisits
                .AnyAsync(v => v.UserId == userId && v.JobPostId == jobPostId);
        }

        // Record visit
        public async Task<WorkerVisit> RecordVisitAsync(WorkerVisit visit)
        {
            _context.WorkerVisits.Add(visit);
            await _context.SaveChangesAsync();
            return visit;
        }

        // Get profile view count for employer
        public async Task<int> GetProfileViewCountAsync(int employerId)
        {
            return await _context.WorkerVisits
                .Where(v => v.JobPost.UserId == employerId)
                .Select(v => v.UserId)
                .Distinct()
                .CountAsync();
        }

        // Get visits by user
        public async Task<List<WorkerVisit>> GetVisitsByUserAsync(int userId)
        {
            return await _context.WorkerVisits
                .Include(v => v.JobPost)
                .ThenInclude(p => p.User)
                .Where(v => v.UserId == userId)
                .OrderByDescending(v => v.VisitTime)
                .ToListAsync();
        }
    }
}
