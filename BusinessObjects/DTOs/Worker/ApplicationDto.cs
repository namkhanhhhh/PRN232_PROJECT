using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.DTOs.Worker
{
    public class ApplicationDto
    {
        public int Id { get; set; }
        public int JobPostId { get; set; }
        public int UserId { get; set; }
        public string? Status { get; set; }
        public int? EmployerRating { get; set; }
        public int? WorkerRating { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Job info
        public string JobTitle { get; set; } = null!;
        public string? JobLocation { get; set; }
        public string? JobType { get; set; }
        public decimal? SalaryMin { get; set; }
        public decimal? SalaryMax { get; set; }

        // Employer info
        public string EmployerName { get; set; } = null!;
        public string? EmployerAvatar { get; set; }

        public string? JobImageMain { get; set; }
    }
}
