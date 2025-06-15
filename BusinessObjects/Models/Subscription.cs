using System;
using System.Collections.Generic;

namespace BusinessObjects.Models;

public partial class Subscription
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int PlanId { get; set; }

    public int SilverPostsRemaining { get; set; }

    public int GoldPostsRemaining { get; set; }

    public int DiamondPostsRemaining { get; set; }

    public int PushTopRemaining { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual SubscriptionPlan Plan { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
