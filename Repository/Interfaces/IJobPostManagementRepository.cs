using BusinessObjects.Models;

namespace Repository.Interfaces
{
    public interface IJobPostManagementRepository
    {
        Task<List<JobPost>> GetJobPostsByEmployerAsync(int employerId);
        Task<JobPost?> GetJobPostByIdAsync(int id);
        Task<bool> CreateJobPostAsync(JobPost jobPost);
        Task<bool> UpdateJobPostAsync(JobPost jobPost);
        Task<bool> DeleteJobPostAsync(int id);
        Task<bool> AddCreditTransactionAsync(int userId, decimal amount, string transactionType, string description);
        Task<List<JobCategory>> GetJobCategoriesAsync();
    }
}
