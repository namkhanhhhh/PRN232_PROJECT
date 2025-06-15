using BusinessObjects.Models;
using DataAccess.DAO;
using Repository.Interfaces;

namespace Repository.Implementations
{
    public class ApplicationRepository : IApplicationRepository
    {
        private readonly ApplicationDAO _applicationDAO;

        public ApplicationRepository(ApplicationDAO applicationDAO)
        {
            _applicationDAO = applicationDAO;
        }

        public async Task<List<Application>> GetApplicationsByUserAsync(int userId)
        {
            return await _applicationDAO.GetApplicationsByUserAsync(userId);
        }

        public async Task<bool> HasUserAppliedAsync(int userId, int jobPostId)
        {
            return await _applicationDAO.HasUserAppliedAsync(userId, jobPostId);
        }

        public async Task<Application> CreateApplicationAsync(Application application)
        {
            return await _applicationDAO.CreateApplicationAsync(application);
        }

        public async Task<Application?> GetApplicationByIdAsync(int id)
        {
            return await _applicationDAO.GetApplicationByIdAsync(id);
        }

        public async Task<bool> UpdateApplicationStatusAsync(int applicationId, string status)
        {
            return await _applicationDAO.UpdateApplicationStatusAsync(applicationId, status);
        }

        public async Task<List<Application>> GetApplicationsByJobPostAsync(int jobPostId)
        {
            return await _applicationDAO.GetApplicationsByJobPostAsync(jobPostId);
        }
    }
}
