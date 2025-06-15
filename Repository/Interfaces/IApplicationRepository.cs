using BusinessObjects.Models;

namespace Repository.Interfaces
{
    public interface IApplicationRepository
    {
        Task<List<Application>> GetApplicationsByUserAsync(int userId);
        Task<bool> HasUserAppliedAsync(int userId, int jobPostId);
        Task<Application> CreateApplicationAsync(Application application);
        Task<Application?> GetApplicationByIdAsync(int id);
        Task<bool> UpdateApplicationStatusAsync(int applicationId, string status);
        Task<List<Application>> GetApplicationsByJobPostAsync(int jobPostId);
    }
}
