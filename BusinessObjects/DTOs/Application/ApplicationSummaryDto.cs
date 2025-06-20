using System.Collections.Generic;

namespace BusinessObjects.DTOs.Application
{
    public class ApplicationSummaryDto
    {
        public List<JobPostSummaryDto> JobPosts { get; set; } = new List<JobPostSummaryDto>();
        public List<ApplicationListDto> Applications { get; set; } = new List<ApplicationListDto>();

        public int? SelectedJobId { get; set; }
        public string? SelectedStatus { get; set; }
        public string? SearchTerm { get; set; }

        public int PendingCount { get; set; }
        public int AcceptedCount { get; set; }
        public int RejectedCount { get; set; }
        public int TotalCount { get; set; }

        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
    }

    public class JobPostSummaryDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Location { get; set; }
        public string? JobType { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int ApplicationCount { get; set; }
    }
}
