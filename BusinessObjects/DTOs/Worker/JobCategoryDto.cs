namespace BusinessObjects.DTOs
{
    public class JobCategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int? ParentId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int JobCount { get; set; } // For featured categories
    }
}