namespace BusinessObjects.DTOs.Worker
{
    public class AllJobsRequestDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 9;
        public int? UserId { get; set; }
    }
}
