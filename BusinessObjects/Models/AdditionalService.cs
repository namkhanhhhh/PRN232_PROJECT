using System;
using System.Collections.Generic;

namespace BusinessObjects.Models;

public partial class AdditionalService
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public int? DurationDays { get; set; }

    public string? ServiceType { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int DiamondPostsIncluded { get; set; }

    public int GoldPostsIncluded { get; set; }

    public int SilverPostsIncluded { get; set; }

    public int AuthenLogoAvailable { get; set; }

    public int PushToTopAvailable { get; set; }

    public virtual ICollection<ServiceOrder> ServiceOrders { get; set; } = new List<ServiceOrder>();
}
