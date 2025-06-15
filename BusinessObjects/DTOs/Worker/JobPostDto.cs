namespace BusinessObjects.DTOs
{
    public class JobPostDto
    {
        public int Id { get; set; }
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

        // User info
        public int UserId { get; set; }
        public string EmployerName { get; set; } = null!;
        public string? EmployerAvatar { get; set; }
        public string? CompanyName { get; set; }

        // Categories
        public List<JobCategoryDto> Categories { get; set; } = new List<JobCategoryDto>();

        // Additional info
        public bool IsInWishlist { get; set; }
        public bool HasApplied { get; set; }

        // Related jobs for job details page
        public List<JobPostDto>? RelatedJobs { get; set; }
    }
}
