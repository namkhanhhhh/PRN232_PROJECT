using System;
using System.Collections.Generic;

namespace BusinessObjects.Models;

public partial class JobCategory
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int? ParentId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<JobCategory> InverseParent { get; set; } = new List<JobCategory>();

    public virtual ICollection<JobPostCategory> JobPostCategories { get; set; } = new List<JobPostCategory>();

    public virtual JobCategory? Parent { get; set; }
}
