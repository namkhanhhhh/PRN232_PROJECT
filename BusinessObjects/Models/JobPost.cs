using System;
using System.Collections.Generic;

namespace BusinessObjects.Models;

public partial class JobPost
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string? Requirements { get; set; }

    public string? Benefits { get; set; }

    public string? Location { get; set; }

    public decimal? SalaryMin { get; set; }

    public decimal? SalaryMax { get; set; }

    public string? JobType { get; set; }

    public string? ExperienceLevel { get; set; }

    public DateOnly? Deadline { get; set; }

    public string ImageMain { get; set; } = null!;

    public string? Image2 { get; set; }

    public string? Image3 { get; set; }

    public string? Image4 { get; set; }

    public string? PostType { get; set; }

    public string? Status { get; set; }

    public int? PriorityLevel { get; set; }

    public int? ViewCount { get; set; }

    public bool? IsFeatured { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? PushedToTopUntil { get; set; }

    public virtual ICollection<Application> Applications { get; set; } = new List<Application>();

    public virtual ICollection<JobPostCategory> JobPostCategories { get; set; } = new List<JobPostCategory>();

    public virtual User User { get; set; } = null!;

    public virtual ICollection<Wishlist> WishlistJobPostId1Navigations { get; set; } = new List<Wishlist>();

    public virtual ICollection<Wishlist> WishlistJobPosts { get; set; } = new List<Wishlist>();

    public virtual ICollection<WorkerVisit> WorkerVisits { get; set; } = new List<WorkerVisit>();
}
