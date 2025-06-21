namespace BusinessObjects.DTOs.CreditTransaction
{
    public class CreditTransactionListDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string TransactionType { get; set; } = string.Empty;
        public int? ReferenceId { get; set; }
        public string? ReferenceType { get; set; }
        public decimal? BalanceAfter { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
