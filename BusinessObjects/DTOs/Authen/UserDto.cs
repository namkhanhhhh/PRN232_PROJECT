using BusinessObjects.DTOs.Worker;

namespace BusinessObjects.DTOs
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string? Avatar { get; set; }
        public bool Status { get; set; }
        public string RoleName { get; set; } = null!;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? Headline { get; set; }
        public int? ExperienceYears { get; set; }
        public string? Education { get; set; }
        public string? Skills { get; set; }
        public string? Bio { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }

        // Additional properties for employer profile
        public int TotalJobPosts { get; set; }
        public int ProfileViewCount { get; set; }
        public List<JobPostDto>? JobPosts { get; set; }
        public UserDetailDto UserDetail { get; set; }
    }
}
