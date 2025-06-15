using System;
using System.Collections.Generic;

namespace BusinessObjects.Models;

public partial class WorkerVisit
{
    public int Id { get; set; }

    public int JobPostId { get; set; }

    public int UserId { get; set; }

    public DateTime? VisitTime { get; set; }

    public bool? IsFirstView { get; set; }

    public virtual JobPost JobPost { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
