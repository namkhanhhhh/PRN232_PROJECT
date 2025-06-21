namespace BusinessObjects.DTOs.Worker
{
    public class JobCategoryWithChildrenDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? ParentId { get; set; }
        public int JobCount { get; set; }
        public List<JobCategoryWithChildrenDto> Children { get; set; } = new List<JobCategoryWithChildrenDto>();
    }
}
