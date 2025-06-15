using System;
using System.Collections.Generic;

namespace BusinessObjects.Models;

public partial class ServiceOrder
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int ServiceId { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? DiamondPostsApplied { get; set; }

    public int? GoldPostsApplied { get; set; }

    public bool? PostCreditsApplied { get; set; }

    public int? SilverPostsApplied { get; set; }

    public virtual AdditionalService Service { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
