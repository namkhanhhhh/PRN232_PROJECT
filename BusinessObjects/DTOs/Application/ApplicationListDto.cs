using System;

namespace BusinessObjects.DTOs.Application
{
    public class ApplicationListDto
    {
        public int Id { get; set; }
        public int JobPostId { get; set; }
        public int UserId { get; set; }
        public string? Status { get; set; }
        public int? EmployerRating { get; set; }
        public int? WorkerRating { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string JobTitle { get; set; } = null!;
        public string? JobLocation { get; set; }
        public string? JobType { get; set; }
        public decimal? SalaryMin { get; set; }
        public decimal? SalaryMax { get; set; }

        public string WorkerName { get; set; } = null!;
        public string? WorkerEmail { get; set; }
        public string? WorkerAvatar { get; set; }
        public string? WorkerPhone { get; set; }
        public string? WorkerAddress { get; set; }
        public string? WorkerHeadline { get; set; }
        public int? WorkerExperienceYears { get; set; }
        public string? WorkerEducation { get; set; }
        public string? WorkerSkills { get; set; }
        public string? WorkerBio { get; set; }
    }
}
