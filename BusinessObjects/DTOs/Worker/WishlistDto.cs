using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.DTOs.Worker
{
    public class WishlistDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int JobPostId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public JobPostDto JobPost { get; set; } = null!;
    }
}
