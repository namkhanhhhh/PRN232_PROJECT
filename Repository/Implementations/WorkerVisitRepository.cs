using BusinessObjects.Models;
using DataAccess.DAO;
using Repository.Interfaces;

namespace Repository.Implementations
{
    public class WorkerVisitRepository : IWorkerVisitRepository
    {
        private readonly WorkerVisitDAO _workerVisitDAO;

        public WorkerVisitRepository(WorkerVisitDAO workerVisitDAO)
        {
            _workerVisitDAO = workerVisitDAO;
        }

        public async Task<bool> HasUserVisitedJobAsync(int userId, int jobPostId)
        {
            return await _workerVisitDAO.HasUserVisitedJobAsync(userId, jobPostId);
        }

        public async Task<WorkerVisit> RecordVisitAsync(WorkerVisit visit)
        {
            return await _workerVisitDAO.RecordVisitAsync(visit);
        }

        public async Task<int> GetProfileViewCountAsync(int employerId)
        {
            return await _workerVisitDAO.GetProfileViewCountAsync(employerId);
        }

        public async Task<List<WorkerVisit>> GetVisitsByUserAsync(int userId)
        {
            return await _workerVisitDAO.GetVisitsByUserAsync(userId);
        }
    }
}
