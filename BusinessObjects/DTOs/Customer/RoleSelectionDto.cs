using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.DTOs.Customer
{
    public class RoleSelectionDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<RoleDto> AvailableRoles { get; set; } = new List<RoleDto>();
    }

    public class RoleDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class AssignRoleDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int RoleId { get; set; }
    }
}
