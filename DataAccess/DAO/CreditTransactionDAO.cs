using BusinessObjects.Models;
using BusinessObjects.DTOs.CreditTransaction;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.DAO
{
    public class CreditTransactionDAO
    {
        private readonly SjobPContext _context;

        public CreditTransactionDAO(SjobPContext context)
        {
            _context = context;
        }

        public async Task<List<CreditTransaction>> GetFilteredTransactionsAsync(CreditTransactionFilterDto filter)
        {
            var query = _context.CreditTransactions
                .Include(t => t.User)
                .ThenInclude(u => u.UserDetails)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(filter.Username))
            {
                query = query.Where(t => t.User.Username.Contains(filter.Username));
            }

            if (!string.IsNullOrEmpty(filter.Email))
            {
                query = query.Where(t => t.User.Email.Contains(filter.Email));
            }

            if (!string.IsNullOrEmpty(filter.TransactionType))
            {
                query = query.Where(t => t.TransactionType == filter.TransactionType);
            }

            if (filter.FromDate.HasValue)
            {
                query = query.Where(t => t.CreatedAt.HasValue && t.CreatedAt >= filter.FromDate.Value);
            }

            if (filter.ToDate.HasValue)
            {
                var endDate = filter.ToDate.Value.AddDays(1);
                query = query.Where(t => t.CreatedAt.HasValue && t.CreatedAt < endDate);
            }

            if (filter.MinAmount.HasValue)
            {
                query = query.Where(t => t.Amount >= filter.MinAmount.Value);
            }

            if (filter.MaxAmount.HasValue)
            {
                query = query.Where(t => t.Amount <= filter.MaxAmount.Value);
            }

            // Apply sorting
            query = ApplySorting(query, filter.SortBy, filter.SortDirection);

            // Apply pagination
            return await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();
        }

        public async Task<int> GetFilteredTransactionsCountAsync(CreditTransactionFilterDto filter)
        {
            var query = _context.CreditTransactions
                .Include(t => t.User)
                .AsQueryable();

            // Apply same filters as GetFilteredTransactionsAsync
            if (!string.IsNullOrEmpty(filter.Username))
            {
                query = query.Where(t => t.User.Username.Contains(filter.Username));
            }

            if (!string.IsNullOrEmpty(filter.Email))
            {
                query = query.Where(t => t.User.Email.Contains(filter.Email));
            }

            if (!string.IsNullOrEmpty(filter.TransactionType))
            {
                query = query.Where(t => t.TransactionType == filter.TransactionType);
            }

            if (filter.FromDate.HasValue)
            {
                query = query.Where(t => t.CreatedAt.HasValue && t.CreatedAt >= filter.FromDate.Value);
            }

            if (filter.ToDate.HasValue)
            {
                var endDate = filter.ToDate.Value.AddDays(1);
                query = query.Where(t => t.CreatedAt.HasValue && t.CreatedAt < endDate);
            }

            if (filter.MinAmount.HasValue)
            {
                query = query.Where(t => t.Amount >= filter.MinAmount.Value);
            }

            if (filter.MaxAmount.HasValue)
            {
                query = query.Where(t => t.Amount <= filter.MaxAmount.Value);
            }

            return await query.CountAsync();
        }

        public async Task<CreditTransaction?> GetTransactionByIdAsync(int id)
        {
            return await _context.CreditTransactions
                .Include(t => t.User)
                .ThenInclude(u => u.UserDetails)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<List<CreditTransaction>> GetUserTransactionsAsync(int userId, int limit = 5)
        {
            return await _context.CreditTransactions
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<CreditTransactionDashboardDto> GetDashboardDataAsync()
        {
            var today = DateTime.Today;
            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            var startOfYear = new DateTime(today.Year, 1, 1);
            var last12Months = today.AddMonths(-11);

            // Get total statistics
            var totalTransactionAmount = await _context.CreditTransactions.SumAsync(t => t.Amount);
            var totalTransactionCount = await _context.CreditTransactions.CountAsync();
            var totalTopupAmount = await _context.CreditTransactions
                .Where(t => t.TransactionType == "Topup" || t.TransactionType == "NapTien")
                .SumAsync(t => t.Amount);
            var totalSpentAmount = await _context.CreditTransactions
                .Where(t => t.TransactionType != "Topup" && t.TransactionType != "NapTien")
                .SumAsync(t => t.Amount);
            var activeUsersCount = await _context.UserCredits.CountAsync();

            // Monthly data for charts (last 12 months) - Get raw data first
            var monthlyTransactionsRaw = await _context.CreditTransactions
                .Where(t => t.CreatedAt.HasValue && t.CreatedAt >= last12Months)
                .GroupBy(t => new {
                    Month = t.CreatedAt.Value.Month,
                    Year = t.CreatedAt.Value.Year
                })
                .Select(g => new {
                    Month = g.Key.Month,
                    Year = g.Key.Year,
                    Amount = g.Sum(t => t.Amount),
                    Count = g.Count()
                })
                .ToListAsync();

            // Format the data on client side
            var monthlyTransactions = monthlyTransactionsRaw
                .Select(d => new MonthlyTransactionData
                {
                    Month = new DateTime(d.Year, d.Month, 1).ToString("MMM yyyy"),
                    Amount = d.Amount,
                    Count = d.Count
                })
                .OrderBy(d => d.Month)
                .ToList();

            // Transaction type breakdown
            var transactionTypes = await _context.CreditTransactions
                .GroupBy(t => t.TransactionType)
                .Select(g => new TransactionTypeData
                {
                    Type = g.Key ?? "Unknown",
                    Amount = g.Sum(t => t.Amount),
                    Count = g.Count()
                })
                .ToListAsync();

            // Calculate percentages
            foreach (var type in transactionTypes)
            {
                type.Percentage = totalTransactionAmount > 0 ? (type.Amount / totalTransactionAmount) * 100 : 0;
            }

            // Top users by transaction amount
            var topUsers = await _context.CreditTransactions
                .GroupBy(t => t.UserId)
                .Select(g => new TopUserData
                {
                    UserId = g.Key,
                    TotalAmount = g.Sum(t => t.Amount),
                    TransactionCount = g.Count()
                })
                .OrderByDescending(u => u.TotalAmount)
                .Take(10)
                .ToListAsync();

            // Get user details for top users
            foreach (var user in topUsers)
            {
                var userData = await _context.Users
                    .Include(u => u.UserDetails)
                    .FirstOrDefaultAsync(u => u.Id == user.UserId);

                if (userData != null)
                {
                    user.Username = userData.Username;
                    user.Email = userData.Email;
                }
            }

            // Recent transactions
            var recentTransactions = await GetFilteredTransactionsAsync(new CreditTransactionFilterDto
            {
                Page = 1,
                PageSize = 10,
                SortBy = "CreatedAt",
                SortDirection = "DESC"
            });

            // Least popular services
            var leastPopularServices = await GetLeastPopularServicesAsync();

            return new CreditTransactionDashboardDto
            {
                TotalTransactionAmount = totalTransactionAmount,
                TotalTransactionCount = totalTransactionCount,
                TotalTopupAmount = totalTopupAmount,
                TotalSpentAmount = totalSpentAmount,
                ActiveUsersCount = activeUsersCount,
                MonthlyTransactions = monthlyTransactions,
                TransactionTypes = transactionTypes,
                TopUsers = topUsers,
                RecentTransactions = recentTransactions.Select(t => new CreditTransactionListDto
                {
                    Id = t.Id,
                    UserId = t.UserId,
                    Username = t.User.Username,
                    Email = t.User.Email,
                    Amount = t.Amount,
                    TransactionType = t.TransactionType ?? "Unknown",
                    ReferenceId = t.ReferenceId,
                    ReferenceType = t.ReferenceType,
                    BalanceAfter = t.BalanceAfter,
                    Description = t.Description,
                    CreatedAt = t.CreatedAt ?? DateTime.Now
                }).ToList(),
                LeastPopularServices = leastPopularServices
            };
        }

        public async Task<List<LeastPopularServiceDto>> GetLeastPopularServicesAsync()
        {
            // Get all services first
            var allServices = await _context.AdditionalServices.ToListAsync();

            var result = new List<LeastPopularServiceDto>();

            foreach (var service in allServices)
            {
                // Get purchase count for this service
                var purchaseCount = await _context.ServiceOrders
                    .CountAsync(o => o.ServiceId == service.Id);

                // Get total revenue for this service
                var totalRevenue = purchaseCount * service.Price;

                // Get last purchased date for this service
                var lastPurchased = await _context.ServiceOrders
                    .Where(o => o.ServiceId == service.Id)
                    .OrderByDescending(o => o.CreatedAt)
                    .Select(o => o.CreatedAt)
                    .FirstOrDefaultAsync();

                result.Add(new LeastPopularServiceDto
                {
                    ServiceId = service.Id,
                    ServiceName = service.Name,
                    Price = service.Price,
                    PurchaseCount = purchaseCount,
                    TotalRevenue = totalRevenue,
                    LastPurchased = lastPurchased
                });
            }

            return result
                .OrderBy(s => s.PurchaseCount)
                .ThenBy(s => s.LastPurchased)
                .Take(10)
                .ToList();
        }

        public async Task<List<string>> GetTransactionTypesAsync()
        {
            return await _context.CreditTransactions
                .Select(t => t.TransactionType)
                .Distinct()
                .Where(t => !string.IsNullOrEmpty(t))
                .ToListAsync();
        }

        private IQueryable<CreditTransaction> ApplySorting(IQueryable<CreditTransaction> query, string sortBy, string sortDirection)
        {
            var isAscending = sortDirection.ToLower() == "asc";

            return sortBy.ToLower() switch
            {
                "amount" => isAscending ? query.OrderBy(t => t.Amount) : query.OrderByDescending(t => t.Amount),
                "username" => isAscending ? query.OrderBy(t => t.User.Username) : query.OrderByDescending(t => t.User.Username),
                "email" => isAscending ? query.OrderBy(t => t.User.Email) : query.OrderByDescending(t => t.User.Email),
                "transactiontype" => isAscending ? query.OrderBy(t => t.TransactionType) : query.OrderByDescending(t => t.TransactionType),
                "balanceafter" => isAscending ? query.OrderBy(t => t.BalanceAfter) : query.OrderByDescending(t => t.BalanceAfter),
                _ => isAscending ? query.OrderBy(t => t.CreatedAt) : query.OrderByDescending(t => t.CreatedAt)
            };
        }

        private DateTime GetVietnamTime()
        {
            var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTimeZone);
        }
    }
}
