using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.DTOs.JobPost
{
    public class JobPostListDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Requirements { get; set; }
        public string? Benefits { get; set; }
        public string? Location { get; set; }
        public decimal? SalaryMin { get; set; }
        public decimal? SalaryMax { get; set; }
        public string? JobType { get; set; }
        public string? ExperienceLevel { get; set; }
        public DateOnly? Deadline { get; set; }
        public string ImageMain { get; set; } = string.Empty;
        public string? Image2 { get; set; }
        public string? Image3 { get; set; }
        public string? Image4 { get; set; }
        public string PostType { get; set; } = "silver";
        public string? Status { get; set; }
        public int? ViewCount { get; set; }
        public bool? IsFeatured { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int ApplicationCount { get; set; }
        public List<string> Categories { get; set; } = new List<string>();
        public string EmployerName { get; set; } = string.Empty;
    }
}
