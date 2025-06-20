using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.DAO
{
    public class ApplicationManagementDAO
    {
        private readonly SjobPContext _context;

        public ApplicationManagementDAO(SjobPContext context)
        {
            _context = context;
        }

        public async Task<List<JobPost>> GetEmployerJobPostsAsync(int employerId)
        {
            return await _context.JobPosts
                .Where(jp => jp.UserId == employerId)
                .OrderByDescending(jp => jp.CreatedAt)
                .ToListAsync();
        }

        public async Task<(List<Application> applications, int totalCount)> GetApplicationsAsync(
            int employerId,
            int? jobId = null,
            string? status = null,
            string? search = null,
            int page = 1,
            int pageSize = 10)
        {
            var query = _context.Applications
                .Include(a => a.JobPost)
                .Include(a => a.User)
                .ThenInclude(u => u.UserDetails)
                .Where(a => a.JobPost.UserId == employerId);

            // Apply filters
            if (jobId.HasValue)
            {
                query = query.Where(a => a.JobPostId == jobId.Value);
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(a => a.Status == status);
            }

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(a =>
                    a.User.Username.Contains(search) ||
                    a.User.UserDetails.Any(ud =>
                        ud.FirstName.Contains(search) ||
                        ud.LastName.Contains(search)));
            }

            var totalCount = await query.CountAsync();

            var applications = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (applications, totalCount);
        }

        public async Task<Application?> GetApplicationDetailAsync(int applicationId, int employerId)
        {
            return await _context.Applications
                .Include(a => a.JobPost)
                .Include(a => a.User)
                .ThenInclude(u => u.UserDetails)
                .Include(a => a.ApplicationNoteApplications)
                .FirstOrDefaultAsync(a => a.Id == applicationId && a.JobPost.UserId == employerId);
        }

        public async Task<bool> UpdateApplicationStatusAsync(int applicationId, int employerId, string status, string? note = null)
        {
            var application = await _context.Applications
                .Include(a => a.JobPost)
                .FirstOrDefaultAsync(a => a.Id == applicationId && a.JobPost.UserId == employerId);

            if (application == null) return false;

            application.Status = status;
            // Remove UpdatedAt since it doesn't exist in database

            // Add note if provided
            if (!string.IsNullOrEmpty(note))
            {
                var applicationNote = new ApplicationNote
                {
                    ApplicationId = application.Id,
                    Note = note,
                    CreatedAt = DateTime.Now
                };

                _context.ApplicationNotes.Add(applicationNote);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<ApplicationNote>> GetApplicationNotesAsync(int applicationId)
        {
            using var context = new SjobPContext();
            return await context.ApplicationNotes
                .Where(n => n.ApplicationId == applicationId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> AddApplicationNoteAsync(int applicationId, int employerId, string note)
        {
            var application = await _context.Applications
                .Include(a => a.JobPost)
                .FirstOrDefaultAsync(a => a.Id == applicationId && a.JobPost.UserId == employerId);

            if (application == null) return false;

            var applicationNote = new ApplicationNote
            {
                ApplicationId = applicationId,
                Note = note,
                CreatedAt = DateTime.Now
            };

            _context.ApplicationNotes.Add(applicationNote);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<(int pending, int accepted, int rejected, int total)> GetApplicationCountsAsync(
            int employerId,
            int? jobId = null)
        {
            var query = _context.Applications
                .Include(a => a.JobPost)
                .Where(a => a.JobPost.UserId == employerId);

            if (jobId.HasValue)
            {
                query = query.Where(a => a.JobPostId == jobId.Value);
            }

            var pending = await query.CountAsync(a => a.Status == "pending");
            var accepted = await query.CountAsync(a => a.Status == "accepted");
            var rejected = await query.CountAsync(a => a.Status == "rejected");
            var total = await query.CountAsync();

            return (pending, accepted, rejected, total);
        }

        public async Task<JobPost?> GetJobPostWithApplicationsAsync(int jobPostId, int employerId)
        {
            return await _context.JobPosts
                .Include(jp => jp.Applications)
                .ThenInclude(a => a.User)
                .ThenInclude(u => u.UserDetails)
                .FirstOrDefaultAsync(jp => jp.Id == jobPostId && jp.UserId == employerId);
        }

        public async Task<(int totalViews, int uniqueViewers)> GetJobViewStatsAsync(int jobPostId)
        {
            var totalViews = await _context.WorkerVisits
                .Where(v => v.JobPostId == jobPostId)
                .CountAsync();

            var uniqueViewers = await _context.WorkerVisits
                .Where(v => v.JobPostId == jobPostId)
                .Select(v => v.UserId)
                .Distinct()
                .CountAsync();

            return (totalViews, uniqueViewers);
        }
    }
}
