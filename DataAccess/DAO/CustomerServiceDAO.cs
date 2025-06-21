using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.DAO
{
    public class CustomerServiceDAO
    {
        private readonly SjobPContext _context;

        public CustomerServiceDAO(SjobPContext context)
        {
            _context = context;
        }

        public async Task<List<AdditionalService>> GetActiveAdditionalServicesAsync()
        {
            return await _context.AdditionalServices
                .Where(x => x.IsActive == true)
                .ToListAsync();
        }

        public async Task<List<SubscriptionPlan>> GetActiveSubscriptionPlansAsync()
        {
            return await _context.SubscriptionPlans
                .Where(x => x.IsActive == true)
                .ToListAsync();
        }

        public async Task<UserCredit?> GetUserCreditAsync(int userId)
        {
            return await _context.UserCredits
                .FirstOrDefaultAsync(x => x.UserId == userId);
        }

        public async Task<UserPostCredit?> GetUserPostCreditAsync(int userId)
        {
            return await _context.UserPostCredits
                .FirstOrDefaultAsync(x => x.UserId == userId);
        }

        public async Task<AdditionalService?> GetAdditionalServiceByIdAsync(int id)
        {
            return await _context.AdditionalServices
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<SubscriptionPlan?> GetSubscriptionPlanByIdAsync(int id)
        {
            return await _context.SubscriptionPlans
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<UserCredit> CreateOrUpdateUserCreditAsync(int userId, decimal? balance)
        {
            var userCredit = await _context.UserCredits
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (userCredit == null)
            {
                userCredit = new UserCredit
                {
                    UserId = userId,
                    Balance = balance,
                    LastUpdated = DateTime.Now
                };
                _context.UserCredits.Add(userCredit);
            }
            else
            {
                userCredit.Balance = balance;
                userCredit.LastUpdated = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return userCredit;
        }

        public async Task<UserPostCredit> CreateOrUpdateUserPostCreditAsync(int userId, UserPostCredit postCredit)
        {
            var existingPostCredit = await _context.UserPostCredits
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (existingPostCredit == null)
            {
                postCredit.UserId = userId;
                postCredit.LastUpdated = DateTime.Now;
                _context.UserPostCredits.Add(postCredit);
            }
            else
            {
                existingPostCredit.SilverPostsAvailable = postCredit.SilverPostsAvailable;
                existingPostCredit.GoldPostsAvailable = postCredit.GoldPostsAvailable;
                existingPostCredit.DiamondPostsAvailable = postCredit.DiamondPostsAvailable;
                existingPostCredit.PushToTopAvailable = postCredit.PushToTopAvailable;
                existingPostCredit.AuthenLogoAvailable = postCredit.AuthenLogoAvailable;
                existingPostCredit.LastUpdated = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return existingPostCredit ?? postCredit;
        }

        public async Task<ServiceOrder> CreateServiceOrderAsync(ServiceOrder serviceOrder)
        {
            _context.ServiceOrders.Add(serviceOrder);
            await _context.SaveChangesAsync();
            return serviceOrder;
        }

        public async Task<Subscription> CreateSubscriptionAsync(Subscription subscription)
        {
            _context.Subscriptions.Add(subscription);
            await _context.SaveChangesAsync();
            return subscription;
        }

        public async Task<CreditTransaction> CreateCreditTransactionAsync(CreditTransaction transaction)
        {
            _context.CreditTransactions.Add(transaction);
            await _context.SaveChangesAsync();
            return transaction;
        }

        public async Task<List<Subscription>> GetUserSubscriptionsAsync(int userId)
        {
            return await _context.Subscriptions
                .Include(s => s.Plan)
                .Where(s => s.UserId == userId)
                .ToListAsync();
        }

        public async Task<List<ServiceOrder>> GetUserServiceOrdersAsync(int userId)
        {
            return await _context.ServiceOrders
                .Include(so => so.Service)
                .Where(so => so.UserId == userId)
                .ToListAsync();
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<List<CreditTransaction>> GetUserPurchaseTransactionsAsync(int userId)
        {
            return await _context.CreditTransactions
                .Where(t => t.UserId == userId && t.TransactionType == "purchase")
                .OrderBy(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> SubscriptionExistsAsync(int userId, int planId, DateTime transactionDate)
        {
            return await _context.Subscriptions
                .AnyAsync(s => s.UserId == userId && s.PlanId == planId &&
                              s.CreatedAt.HasValue && s.CreatedAt.Value.Date == transactionDate.Date);
        }

        public async Task<bool> ServiceOrderExistsAsync(int userId, int serviceId, DateTime transactionDate)
        {
            return await _context.ServiceOrders
                .AnyAsync(s => s.UserId == userId && s.ServiceId == serviceId &&
                              s.CreatedAt.HasValue && s.CreatedAt.Value.Date == transactionDate.Date);
        }
    }
}