namespace BusinessObjects.DTOs.Application
{
    public class UpdateApplicationStatusDto
    {
        public int ApplicationId { get; set; }
        public string Status { get; set; } = null!;
        public string? Note { get; set; }
    }
}
