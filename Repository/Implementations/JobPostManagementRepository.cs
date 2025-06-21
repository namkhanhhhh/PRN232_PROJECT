using BusinessObjects.Models;
using BusinessObjects.DTOs.JobPost;
using DataAccess.DAO;
using Repository.Interfaces;

namespace Repository.Implementations
{
    public class JobPostManagementRepository : IJobPostManagementRepository
    {
        private readonly JobPostManagementDAO _jobPostManagementDAO;

        public JobPostManagementRepository(JobPostManagementDAO jobPostManagementDAO)
        {
            _jobPostManagementDAO = jobPostManagementDAO;
        }

        public async Task<List<JobPost>> GetJobPostsByEmployerAsync(int employerId)
        {
            return await _jobPostManagementDAO.GetJobPostsByEmployerAsync(employerId);
        }

        public async Task<JobPost?> GetJobPostByIdAsync(int id)
        {
            return await _jobPostManagementDAO.GetJobPostByIdAsync(id);
        }

        public async Task<bool> CreateJobPostAsync(JobPost jobPost)
        {
            return await _jobPostManagementDAO.CreateJobPostAsync(jobPost);
        }

        public async Task<bool> UpdateJobPostAsync(JobPost jobPost)
        {
            return await _jobPostManagementDAO.UpdateJobPostAsync(jobPost);
        }

        public async Task<bool> UpdateJobPostWithCategoriesAsync(JobPostUpdateDto jobPostDto)
        {
            return await _jobPostManagementDAO.UpdateJobPostWithCategoriesAsync(jobPostDto);
        }

        public async Task<bool> DeleteJobPostAsync(int id)
        {
            return await _jobPostManagementDAO.DeleteJobPostAsync(id);
        }

        public async Task<bool> AddCreditTransactionAsync(int userId, decimal amount, string transactionType, string description)
        {
            return await _jobPostManagementDAO.AddCreditTransactionAsync(userId, amount, transactionType, description);
        }

        public async Task<List<JobCategory>> GetJobCategoriesAsync()
        {
            return await _jobPostManagementDAO.GetJobCategoriesAsync();
        }

        public async Task<UserPostCredit?> GetUserPostCreditsAsync(int userId)
        {
            return await _jobPostManagementDAO.GetUserPostCreditsAsync(userId);
        }

        public async Task<bool> DeductPostCreditAsync(int userId, string postType)
        {
            return await _jobPostManagementDAO.DeductPostCreditAsync(userId, postType);
        }
    }
}
