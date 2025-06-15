using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.DTOs.Worker
{
    public class UpdateProfileDto
    {
        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }

        public string? Address { get; set; }

        public string? Headline { get; set; }

        public string? Bio { get; set; }

        public int? ExperienceYears { get; set; }

        public string? Skills { get; set; }

        public string? Education { get; set; }

        public string? DesiredPosition { get; set; }

        public decimal? DesiredSalary { get; set; }

        public string? DesiredLocation { get; set; }

        public string? Availability { get; set; }
    }
}
