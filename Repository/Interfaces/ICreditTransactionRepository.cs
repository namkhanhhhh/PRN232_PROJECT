using BusinessObjects.Models;
using BusinessObjects.DTOs.CreditTransaction;

namespace Repository.Interfaces
{
    public interface ICreditTransactionRepository
    {
        Task<List<CreditTransaction>> GetFilteredTransactionsAsync(CreditTransactionFilterDto filter);
        Task<int> GetFilteredTransactionsCountAsync(CreditTransactionFilterDto filter);
        Task<CreditTransaction?> GetTransactionByIdAsync(int id);
        Task<List<CreditTransaction>> GetUserTransactionsAsync(int userId, int limit = 5);
        Task<CreditTransactionDashboardDto> GetDashboardDataAsync();
        Task<List<LeastPopularServiceDto>> GetLeastPopularServicesAsync();
        Task<List<string>> GetTransactionTypesAsync();
    }
}