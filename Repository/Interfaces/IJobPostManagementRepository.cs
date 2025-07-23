using BusinessObjects.Models;
using BusinessObjects.DTOs.JobPost;

namespace Repository.Interfaces
{
    public interface IJobPostManagementRepository
    {
        Task<List<JobPost>> GetJobPostsByEmployerAsync(int employerId);
        Task<JobPost?> GetJobPostByIdAsync(int id);
        Task<bool> CreateJobPostAsync(JobPost jobPost);
        Task<bool> UpdateJobPostAsync(JobPost jobPost);
        Task<bool> UpdateJobPostWithCategoriesAsync(JobPostUpdateDto jobPostDto);
        Task<bool> DeleteJobPostAsync(int id);
        Task<bool> AddCreditTransactionAsync(int userId, decimal amount, string transactionType, string description);
        Task<List<JobCategory>> GetJobCategoriesAsync();
        Task<UserPostCredit?> GetUserPostCreditsAsync(int userId);
        Task<bool> DeductPostCreditAsync(int userId, string postType);
        Task<bool> ActivateJobPostAsync(int id);
        Task<bool> ToggleJobPostStatusAsync(int id);
    }
}
