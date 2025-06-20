using BusinessObjects.Models;
using DataAccess.DAO;
using Repository.Interfaces;

namespace Repository.Implementations
{
    public class CreditRepository : ICreditRepository
    {
        private readonly CreditDAO _creditDAO;

        public CreditRepository(CreditDAO creditDAO)
        {
            _creditDAO = creditDAO;
        }

        public async Task<UserCredit?> GetUserCreditAsync(int userId)
        {
            return await _creditDAO.GetUserCreditAsync(userId);
        }

        public async Task<UserCredit> CreateOrUpdateUserCreditAsync(int userId, decimal amount)
        {
            return await _creditDAO.CreateOrUpdateUserCreditAsync(userId, amount);
        }

        public async Task<CreditTransaction> CreateTransactionAsync(CreditTransaction transaction)
        {
            return await _creditDAO.CreateTransactionAsync(transaction);
        }

        public async Task<List<CreditTransaction>> GetUserTransactionsAsync(int userId, int page = 1, int pageSize = 10)
        {
            return await _creditDAO.GetUserTransactionsAsync(userId, page, pageSize);
        }

        public async Task<bool> TransactionExistsAsync(string orderCode)
        {
            return await _creditDAO.TransactionExistsAsync(orderCode);
        }
    }
}
