using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.DTOs
{
    public class PaginatedResponseDto<T> : ApiResponseDto
    {
        public List<T> Items { get; set; } = new List<T>();
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int TotalItems { get; set; }
        public string SearchTerm { get; set; } = string.Empty;
        public string SortBy { get; set; } = string.Empty;
        public string SortOrder { get; set; } = string.Empty;
    }
}
