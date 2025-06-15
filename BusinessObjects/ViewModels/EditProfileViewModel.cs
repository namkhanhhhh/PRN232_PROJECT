using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.ViewModels
{
    public class EditProfileViewModel
    {
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(100)]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Phone]
        public string PhoneNumber { get; set; }

        public string Address { get; set; }

        [StringLength(255)]
        public string Headline { get; set; }

        public string Bio { get; set; }

        [Range(0, 50)]
        public int? ExperienceYears { get; set; }

        public string Skills { get; set; }

        public string Education { get; set; }

        [StringLength(255)]
        public string DesiredPosition { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? DesiredSalary { get; set; }

        [StringLength(255)]
        public string DesiredLocation { get; set; }

        public string Availability { get; set; }

        public string CurrentAvatar { get; set; }
    }
}
