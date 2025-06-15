namespace ProductManagementWebClient.DTOs
{
    public class ExternalLoginRequestDto
    {
        public string Provider { get; set; } = null!;
        public string ProviderId { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Name { get; set; } = null!;
    }
}
