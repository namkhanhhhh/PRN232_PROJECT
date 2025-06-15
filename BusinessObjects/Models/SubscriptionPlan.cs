using System;
using System.Collections.Generic;

namespace BusinessObjects.Models;

public partial class SubscriptionPlan
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public decimal Price { get; set; }

    public int DurationDays { get; set; }

    public int SilverPosts { get; set; }

    public int GoldPosts { get; set; }

    public int DiamondPosts { get; set; }

    public int PushTopTimes { get; set; }

    public bool? MarketingPackage { get; set; }

    public int? PriorityLevel { get; set; }

    public string? Description { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}
