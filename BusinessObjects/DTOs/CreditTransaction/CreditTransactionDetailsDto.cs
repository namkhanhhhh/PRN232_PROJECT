namespace BusinessObjects.DTOs.CreditTransaction
{
    public class CreditTransactionDetailsDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public decimal Amount { get; set; }
        public string TransactionType { get; set; } = string.Empty;
        public int? ReferenceId { get; set; }
        public string? ReferenceType { get; set; }
        public decimal? BalanceAfter { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }

        // User's other transactions
        public List<CreditTransactionListDto> UserTransactions { get; set; } = new();

        // User's current balance
        public decimal CurrentBalance { get; set; }
    }
}
