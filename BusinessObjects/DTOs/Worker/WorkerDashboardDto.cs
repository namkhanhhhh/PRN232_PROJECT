using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.DTOs.Worker
{
    public class WorkerDashboardDto
    {
        public List<JobCategoryWithChildrenDto> FeaturedCategories { get; set; } = new List<JobCategoryWithChildrenDto>();
        public List<JobPostDto> DiamondPosts { get; set; } = new List<JobPostDto>();
        public List<JobPostDto> MostViewedPosts { get; set; } = new List<JobPostDto>();
        public List<JobPostDto> AllPosts { get; set; } = new List<JobPostDto>();
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int TotalItems { get; set; }

    }
}
