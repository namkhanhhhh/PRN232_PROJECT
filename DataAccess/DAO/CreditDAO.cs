using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.DAO
{
    public class CreditDAO
    {
        private readonly SjobPContext _context;

        public CreditDAO(SjobPContext context)
        {
            _context = context;
        }

        public async Task<UserCredit?> GetUserCreditAsync(int userId)
        {
            return await _context.UserCredits
                .FirstOrDefaultAsync(uc => uc.UserId == userId);
        }

        public async Task<UserCredit> CreateOrUpdateUserCreditAsync(int userId, decimal amount)
        {
            var userCredit = await GetUserCreditAsync(userId);

            if (userCredit == null)
            {
                userCredit = new UserCredit
                {
                    UserId = userId,
                    Balance = amount,
                    LastUpdated = GetVietnamTime()
                };
                _context.UserCredits.Add(userCredit);
            }
            else
            {
                userCredit.Balance += amount;
                userCredit.LastUpdated = GetVietnamTime();
            }

            await _context.SaveChangesAsync();
            return userCredit;
        }

        public async Task<CreditTransaction> CreateTransactionAsync(CreditTransaction transaction)
        {
            transaction.CreatedAt = GetVietnamTime();
            _context.CreditTransactions.Add(transaction);
            await _context.SaveChangesAsync();
            return transaction;
        }

        public async Task<List<CreditTransaction>> GetUserTransactionsAsync(int userId, int page = 1, int pageSize = 10)
        {
            return await _context.CreditTransactions
                .Where(ct => ct.UserId == userId)
                .OrderByDescending(ct => ct.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<bool> TransactionExistsAsync(string orderCode)
        {
            return await _context.CreditTransactions
                .AnyAsync(ct => ct.Description != null && ct.Description.Contains(orderCode));
        }

        private DateTime GetVietnamTime()
        {
            var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTimeZone);
        }
    }
}
