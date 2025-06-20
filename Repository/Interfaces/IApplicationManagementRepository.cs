using BusinessObjects.Models;

namespace Repository.Interfaces
{
    public interface IApplicationManagementRepository
    {
        Task<List<JobPost>> GetEmployerJobPostsAsync(int employerId);
        Task<(List<Application> applications, int totalCount)> GetApplicationsAsync(
            int employerId,
            int? jobId = null,
            string? status = null,
            string? search = null,
            int page = 1,
            int pageSize = 10);
        Task<Application?> GetApplicationDetailAsync(int applicationId, int employerId);
        Task<bool> UpdateApplicationStatusAsync(int applicationId, int employerId, string status, string? note = null);
        Task<bool> AddApplicationNoteAsync(int applicationId, int employerId, string note);
        Task<(int pending, int accepted, int rejected, int total)> GetApplicationCountsAsync(int employerId, int? jobId = null);
        Task<JobPost?> GetJobPostWithApplicationsAsync(int jobPostId, int employerId);
        Task<(int totalViews, int uniqueViewers)> GetJobViewStatsAsync(int jobPostId);
        Task<List<ApplicationNote>> GetApplicationNotesAsync(int applicationId);
    }
}
