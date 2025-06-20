using System;
using System.Collections.Generic;

namespace BusinessObjects.DTOs.Application
{
    public class JobStatsDto
    {
        public int JobPostId { get; set; }
        public string JobTitle { get; set; } = null!;
        public string? JobLocation { get; set; }
        public string? JobType { get; set; }
        public decimal? SalaryMin { get; set; }
        public decimal? SalaryMax { get; set; }
        public DateTime? JobCreatedAt { get; set; }
        public DateTime? JobDeadline { get; set; }
        public string? JobStatus { get; set; }
        public string? PostType { get; set; }

        // Application statistics
        public int TotalApplications { get; set; }
        public int PendingApplications { get; set; }
        public int AcceptedApplications { get; set; }
        public int RejectedApplications { get; set; }

        // View statistics
        public int TotalViews { get; set; }
        public int UniqueViewers { get; set; }
        public double ConversionRate { get; set; }

        // Recent applications
        public List<ApplicationListDto> RecentApplications { get; set; } = new List<ApplicationListDto>();
    }
}
