namespace BusinessObjects.DTOs.CreditTransaction
{
    public class CreditTransactionDashboardDto
    {
        public decimal TotalTransactionAmount { get; set; }
        public int TotalTransactionCount { get; set; }
        public decimal TotalTopupAmount { get; set; }
        public decimal TotalSpentAmount { get; set; }
        public int ActiveUsersCount { get; set; }

        // Monthly data for charts
        public List<MonthlyTransactionData> MonthlyTransactions { get; set; } = new();

        // Transaction type breakdown
        public List<TransactionTypeData> TransactionTypes { get; set; } = new();

        // Top users
        public List<TopUserData> TopUsers { get; set; } = new();

        // Recent transactions
        public List<CreditTransactionListDto> RecentTransactions { get; set; } = new();

        // Least popular services
        public List<LeastPopularServiceDto> LeastPopularServices { get; set; } = new();
    }

    public class MonthlyTransactionData
    {
        public string Month { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public int Count { get; set; }
    }

    public class TransactionTypeData
    {
        public string Type { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }

    public class TopUserData
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public int TransactionCount { get; set; }
    }

    public class LeastPopularServiceDto
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int PurchaseCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public DateTime? LastPurchased { get; set; }
    }
}
