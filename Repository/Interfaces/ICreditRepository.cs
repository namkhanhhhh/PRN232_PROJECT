using BusinessObjects.Models;

namespace Repository.Interfaces
{
    public interface ICreditRepository
    {
        Task<UserCredit?> GetUserCreditAsync(int userId);
        Task<UserCredit> CreateOrUpdateUserCreditAsync(int userId, decimal amount);
        Task<CreditTransaction> CreateTransactionAsync(CreditTransaction transaction);
        Task<List<CreditTransaction>> GetUserTransactionsAsync(int userId, int page = 1, int pageSize = 10);
        Task<bool> TransactionExistsAsync(string orderCode);
    }
}
