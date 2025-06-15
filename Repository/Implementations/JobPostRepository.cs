using BusinessObjects.Models;
using DataAccess.DAO;
using Repository.Interfaces;

namespace Repository.Implementations
{
    public class JobPostRepository : IJobPostRepository
    {
        private readonly JobPostDAO _jobPostDAO;

        public JobPostRepository(JobPostDAO jobPostDAO)
        {
            _jobPostDAO = jobPostDAO;
        }

        public async Task<List<JobPost>> GetJobPostsAsync(int page, int pageSize, string? keyword = null, string? location = null, string? jobType = null, decimal? minSalary = null, decimal? maxSalary = null, int? categoryId = null)
        {
            return await _jobPostDAO.GetJobPostsAsync(page, pageSize, keyword, location, jobType, minSalary, maxSalary, categoryId);
        }

        public async Task<int> GetJobPostsCountAsync(string? keyword = null, string? location = null, string? jobType = null, decimal? minSalary = null, decimal? maxSalary = null, int? categoryId = null)
        {
            return await _jobPostDAO.GetJobPostsCountAsync(keyword, location, jobType, minSalary, maxSalary, categoryId);
        }

        public async Task<JobPost?> GetJobPostByIdAsync(int id)
        {
            return await _jobPostDAO.GetJobPostByIdAsync(id);
        }

        public async Task<List<JobCategory>> GetFeaturedCategoriesAsync(int count = 8)
        {
            return await _jobPostDAO.GetFeaturedCategoriesAsync(count);
        }

        public async Task<List<JobPost>> GetDiamondPostsAsync(int count = 6)
        {
            return await _jobPostDAO.GetDiamondPostsAsync(count);
        }

        public async Task<List<JobPost>> GetMostViewedPostsAsync(int count = 6)
        {
            return await _jobPostDAO.GetMostViewedPostsAsync(count);
        }

        public async Task<List<JobPost>> GetRelatedJobsAsync(int jobId, List<int> categoryIds, int count = 4)
        {
            return await _jobPostDAO.GetRelatedJobsAsync(jobId, categoryIds, count);
        }

        public async Task<List<JobPost>> GetJobsByCategoryAsync(int categoryId, int page, int pageSize)
        {
            return await _jobPostDAO.GetJobsByCategoryAsync(categoryId, page, pageSize);
        }

        public async Task<int> GetJobsByCategoryCountAsync(int categoryId)
        {
            return await _jobPostDAO.GetJobsByCategoryCountAsync(categoryId);
        }

        public async Task<bool> UpdateViewCountAsync(int jobId)
        {
            return await _jobPostDAO.UpdateViewCountAsync(jobId);
        }

        public async Task<List<JobPost>> GetJobsByEmployerAsync(int employerId)
        {
            return await _jobPostDAO.GetJobsByEmployerAsync(employerId);
        }
    }
}
