using BusinessObjects.DTOs.JobPost;

namespace BusinessObjects.DTOs.Customer
{
    public class CustomerDetailDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Avatar { get; set; }
        public bool Status { get; set; }
        public string StatusText => Status ? "Active" : "Inactive";
        public string? AuthProvider { get; set; }
        public string? AuthProviderId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public int RoleId { get; set; }

        // User Details
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}".Trim();
        public string? Headline { get; set; }
        public int? ExperienceYears { get; set; }
        public string? Education { get; set; }
        public string? Skills { get; set; }
        public string? DesiredPosition { get; set; }
        public decimal? DesiredSalary { get; set; }
        public string? DesiredLocation { get; set; }
        public string? Availability { get; set; }
        public string? Bio { get; set; }

        // Credit Information
        public decimal CreditBalance { get; set; }
        public DateTime? LastCreditUpdate { get; set; }

        // Statistics
        public int JobPostCount { get; set; }
        public int ApplicationCount { get; set; }

        public List<JobPostListDto> JobPosts { get; set; } = new List<JobPostListDto>();
        public List<CreditTransactionDto> CreditTransactions { get; set; } = new List<CreditTransactionDto>();
    }
}
