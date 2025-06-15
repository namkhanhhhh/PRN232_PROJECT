using System;
using System.Collections.Generic;

namespace BusinessObjects.Models;

public partial class CreditTransaction
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

    public virtual User User { get; set; } = null!;
}
