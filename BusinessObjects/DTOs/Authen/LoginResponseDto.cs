namespace BusinessObjects.DTOs
{
    public class LoginResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public UserDto? User { get; set; }
        public string? Token { get; set; }
        public bool NeedsRoleSelection { get; set; }

    }
}
