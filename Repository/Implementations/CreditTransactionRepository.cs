using BusinessObjects.Models;
using BusinessObjects.DTOs.CreditTransaction;
using DataAccess.DAO;
using Repository.Interfaces;

namespace Repository.Implementations
{
    public class CreditTransactionRepository : ICreditTransactionRepository
    {
        private readonly CreditTransactionDAO _creditTransactionDAO;

        public CreditTransactionRepository(CreditTransactionDAO creditTransactionDAO)
        {
            _creditTransactionDAO = creditTransactionDAO;
        }

        public async Task<List<CreditTransaction>> GetFilteredTransactionsAsync(CreditTransactionFilterDto filter)
        {
            return await _creditTransactionDAO.GetFilteredTransactionsAsync(filter);
        }

        public async Task<int> GetFilteredTransactionsCountAsync(CreditTransactionFilterDto filter)
        {
            return await _creditTransactionDAO.GetFilteredTransactionsCountAsync(filter);
        }

        public async Task<CreditTransaction?> GetTransactionByIdAsync(int id)
        {
            return await _creditTransactionDAO.GetTransactionByIdAsync(id);
        }

        public async Task<List<CreditTransaction>> GetUserTransactionsAsync(int userId, int limit = 5)
        {
            return await _creditTransactionDAO.GetUserTransactionsAsync(userId, limit);
        }

        public async Task<CreditTransactionDashboardDto> GetDashboardDataAsync()
        {
            return await _creditTransactionDAO.GetDashboardDataAsync();
        }

        public async Task<List<LeastPopularServiceDto>> GetLeastPopularServicesAsync()
        {
            return await _creditTransactionDAO.GetLeastPopularServicesAsync();
        }

        public async Task<List<string>> GetTransactionTypesAsync()
        {
            return await _creditTransactionDAO.GetTransactionTypesAsync();
        }
    }
}