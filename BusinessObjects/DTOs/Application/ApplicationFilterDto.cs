namespace BusinessObjects.DTOs.Application
{
    public class ApplicationFilterDto
    {
        public int? JobId { get; set; }
        public string? Status { get; set; }
        public string? Search { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
