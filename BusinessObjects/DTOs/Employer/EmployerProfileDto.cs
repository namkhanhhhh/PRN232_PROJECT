namespace BusinessObjects.DTOs.Employer
{
    public class EmployerProfileDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Avatar { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }

        // Statistics
        public int JobPostsCount { get; set; }
        public int ApplicationsCount { get; set; }
        public int ViewsCount { get; set; }

        // Post Credits
        public int SilverPostsAvailable { get; set; }
        public int GoldPostsAvailable { get; set; }
        public int DiamondPostsAvailable { get; set; }
        public int AuthenLogoAvailable { get; set; }
        public int PushToTopAvailable { get; set; }

        // Balance
        public decimal Balance { get; set; }

        public DateTime? CreatedAt { get; set; }

        // Computed properties
        public string FullName => $"{FirstName} {LastName}".Trim();
        public int TotalPostsAvailable => SilverPostsAvailable + GoldPostsAvailable + DiamondPostsAvailable;
    }
}
