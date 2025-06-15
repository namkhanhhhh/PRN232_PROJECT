using System;
using System.Collections.Generic;

namespace BusinessObjects.Models;

public partial class JobPostCategory
{
    public int Id { get; set; }

    public int JobPostId { get; set; }

    public int CategoryId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual JobCategory Category { get; set; } = null!;

    public virtual JobPost JobPost { get; set; } = null!;
}
