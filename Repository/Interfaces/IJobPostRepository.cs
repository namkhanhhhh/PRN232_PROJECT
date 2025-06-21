using BusinessObjects.Models;

namespace Repository.Interfaces
{
    public interface IJobPostRepository
    {
        Task<List<JobPost>> GetJobPostsAsync(int page, int pageSize, string? keyword = null, string? location = null, string? jobType = null, decimal? minSalary = null, decimal? maxSalary = null, int? categoryId = null);
        Task<int> GetJobPostsCountAsync(string? keyword = null, string? location = null, string? jobType = null, decimal? minSalary = null, decimal? maxSalary = null, int? categoryId = null);
        Task<JobPost?> GetJobPostByIdAsync(int id);
        Task<List<JobCategory>> GetFeaturedCategoriesAsync(int count = 8);
        Task<List<JobCategory>> GetCategoriesWithChildrenAsync();
        Task<List<JobPost>> GetDiamondPostsAsync(int count = 6);
        Task<List<JobPost>> GetMostViewedPostsAsync(int count = 6);
        Task<List<JobPost>> GetAllJobsAsync(int page, int pageSize);
        Task<int> GetAllJobsCountAsync();
        Task<List<JobPost>> GetRelatedJobsAsync(int jobId, List<int> categoryIds, int count = 4);
        Task<List<JobPost>> GetJobsByCategoryAsync(int categoryId, int page, int pageSize);
        Task<int> GetJobsByCategoryCountAsync(int categoryId);
        Task<bool> UpdateViewCountAsync(int jobId);
        Task<List<JobPost>> GetJobsByEmployerAsync(int employerId);
    }
}
