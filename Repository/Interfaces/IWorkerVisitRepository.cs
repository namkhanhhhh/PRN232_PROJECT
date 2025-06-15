using BusinessObjects.Models;

namespace Repository.Interfaces
{
    public interface IWorkerVisitRepository
    {
        Task<bool> HasUserVisitedJobAsync(int userId, int jobPostId);
        Task<WorkerVisit> RecordVisitAsync(WorkerVisit visit);
        Task<int> GetProfileViewCountAsync(int employerId);
        Task<List<WorkerVisit>> GetVisitsByUserAsync(int userId);
    }
}
