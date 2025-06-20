namespace BusinessObjects.DTOs.Credit
{
    public class CreditTransactionDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        public string? TransactionType { get; set; }
        public int? ReferenceId { get; set; }
        public string? ReferenceType { get; set; }
        public decimal BalanceAfter { get; set; }
        public string? Description { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
