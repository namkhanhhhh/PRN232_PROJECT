using System;
using System.Collections.Generic;

namespace BusinessObjects.DTOs.Application
{
    public class ApplicationDetailDto
    {
        public int Id { get; set; }
        public int JobPostId { get; set; }
        public int UserId { get; set; }
        public string? Status { get; set; }
        public int? EmployerRating { get; set; }
        public int? WorkerRating { get; set; }
        public DateTime? CreatedAt { get; set; }
        // Remove UpdatedAt since it doesn't exist in database

        // Job info
        public string JobTitle { get; set; } = null!;
        public string? JobDescription { get; set; }
        public string? JobRequirements { get; set; }
        public string? JobLocation { get; set; }
        public string? JobType { get; set; }
        public decimal? SalaryMin { get; set; }
        public decimal? SalaryMax { get; set; }
        public DateTime? JobDeadline { get; set; }

        // Worker info
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
        public string? WorkerDesiredPosition { get; set; }
        public decimal? WorkerDesiredSalary { get; set; }
        public string? WorkerDesiredLocation { get; set; }
        public string? WorkerAvailability { get; set; }

        // Application notes
        public List<ApplicationNoteDto> Notes { get; set; } = new List<ApplicationNoteDto>();
    }
}
