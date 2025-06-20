using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.DTOs.Employer
{
    public class EditEmployerProfileDto
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "Tên đăng nhập là bắt buộc")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;

        public string? Avatar { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string? PhoneNumber { get; set; }

        public string? Address { get; set; }
    }
}
