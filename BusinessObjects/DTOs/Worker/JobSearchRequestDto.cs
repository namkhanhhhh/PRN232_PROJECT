using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.DTOs.Worker
{
    public class JobSearchRequestDto : PaginatedRequestDto
    {
        public string? Keyword { get; set; }
        public string? Location { get; set; }
        public string? JobType { get; set; }
        public decimal? MinSalary { get; set; }
        public decimal? MaxSalary { get; set; }
        public int? CategoryId { get; set; }
        public int? UserId { get; set; } // For checking wishlist and application status
    }
}
