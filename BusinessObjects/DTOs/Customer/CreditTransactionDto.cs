using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.DTOs.Customer
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

    public class AddCreditTransactionDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required]
        public string TransactionType { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
    }
}
