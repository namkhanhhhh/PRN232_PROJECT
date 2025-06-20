using BusinessObjects.Models;
using DataAccess.DAO;
using Repository.Interfaces;

namespace Repository.Implementations
{
    public class ApplicationManagementRepository : IApplicationManagementRepository
    {
        private readonly ApplicationManagementDAO _applicationManagementDAO;

        public ApplicationManagementRepository(ApplicationManagementDAO applicationManagementDAO)
        {
            _applicationManagementDAO = applicationManagementDAO;
        }

        public async Task<List<JobPost>> GetEmployerJobPostsAsync(int employerId)
        {
            return await _applicationManagementDAO.GetEmployerJobPostsAsync(employerId);
        }

        public async Task<(List<Application> applications, int totalCount)> GetApplicationsAsync(
            int employerId,
            int? jobId = null,
            string? status = null,
            string? search = null,
            int page = 1,
            int pageSize = 10)
        {
            return await _applicationManagementDAO.GetApplicationsAsync(employerId, jobId, status, search, page, pageSize);
        }

        public async Task<BusinessObjects.Models.Application> GetApplicationDetailAsync(int applicationId, int employerId)
        {
            var application = await _applicationManagementDAO.GetApplicationDetailAsync(applicationId, employerId);
            if (application != null)
            {
                // Load notes separately
                var notes = await _applicationManagementDAO.GetApplicationNotesAsync(applicationId);
                // You can add notes to a custom property or handle them in the controller
            }
            return application;
        }

        public async Task<List<ApplicationNote>> GetApplicationNotesAsync(int applicationId)
        {
            return await _applicationManagementDAO.GetApplicationNotesAsync(applicationId);
        }

        public async Task<bool> UpdateApplicationStatusAsync(int applicationId, int employerId, string status, string? note = null)
        {
            return await _applicationManagementDAO.UpdateApplicationStatusAsync(applicationId, employerId, status, note);
        }

        public async Task<bool> AddApplicationNoteAsync(int applicationId, int employerId, string note)
        {
            return await _applicationManagementDAO.AddApplicationNoteAsync(applicationId, employerId, note);
        }

        public async Task<(int pending, int accepted, int rejected, int total)> GetApplicationCountsAsync(int employerId, int? jobId = null)
        {
            return await _applicationManagementDAO.GetApplicationCountsAsync(employerId, jobId);
        }

        public async Task<JobPost?> GetJobPostWithApplicationsAsync(int jobPostId, int employerId)
        {
            return await _applicationManagementDAO.GetJobPostWithApplicationsAsync(jobPostId, employerId);
        }

        public async Task<(int totalViews, int uniqueViewers)> GetJobViewStatsAsync(int jobPostId)
        {
            return await _applicationManagementDAO.GetJobViewStatsAsync(jobPostId);
        }
    }
}
