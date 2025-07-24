namespace BusinessObjects.DTOs
{
    public class PaginatedRequestDto
    {
        public string SearchTerm { get; set; } = string.Empty;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 9;
        public string SortBy { get; set; } = "Id";
        public string SortOrder { get; set; } = "asc";
    }
}
