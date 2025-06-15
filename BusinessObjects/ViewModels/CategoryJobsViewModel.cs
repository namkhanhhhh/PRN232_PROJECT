using BusinessObjects.DTOs;

namespace BusinessObjects.ViewModels
{
    public class CategoryJobsViewModel
    {
        public JobCategoryDto Category { get; set; }
        public PaginatedResponseDto<JobPostDto> Jobs { get; set; }
        public List<JobCategoryDto> RelatedCategories { get; set; }

        public CategoryJobsViewModel()
        {
            Category = new JobCategoryDto();
            Jobs = new PaginatedResponseDto<JobPostDto>();
            RelatedCategories = new List<JobCategoryDto>();
        }
    }
}
