using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.DTOs.JobPost
{
    public class JobPostCreateDto
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(255, ErrorMessage = "Title cannot exceed 255 characters")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Requirements cannot exceed 500 characters")]
        public string? Requirements { get; set; }

        [StringLength(500, ErrorMessage = "Benefits cannot exceed 500 characters")]
        public string? Benefits { get; set; }

        [StringLength(255, ErrorMessage = "Location cannot exceed 255 characters")]
        public string? Location { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Minimum salary must be positive")]
        public decimal? SalaryMin { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Maximum salary must be positive")]
        public decimal? SalaryMax { get; set; }

        [StringLength(255, ErrorMessage = "Job type cannot exceed 255 characters")]
        public string? JobType { get; set; }

        [StringLength(255, ErrorMessage = "Experience level cannot exceed 255 characters")]
        public string? ExperienceLevel { get; set; }

        public DateOnly? Deadline { get; set; }

        [Required(ErrorMessage = "Main image is required")]
        [StringLength(255, ErrorMessage = "Image path cannot exceed 255 characters")]
        public string ImageMain { get; set; } = string.Empty;

        [StringLength(255, ErrorMessage = "Image path cannot exceed 255 characters")]
        public string? Image2 { get; set; }

        [StringLength(255, ErrorMessage = "Image path cannot exceed 255 characters")]
        public string? Image3 { get; set; }

        [StringLength(255, ErrorMessage = "Image path cannot exceed 255 characters")]
        public string? Image4 { get; set; }

        public string PostType { get; set; } = "silver";

        public List<int> CategoryIds { get; set; } = new List<int>();

        public int UserId { get; set; }
    }
}
