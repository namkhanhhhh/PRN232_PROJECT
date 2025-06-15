using System;
using System.Collections.Generic;

namespace BusinessObjects.Models;

public partial class Application
{
    public int Id { get; set; }

    public int JobPostId { get; set; }

    public int UserId { get; set; }

    public string? Status { get; set; }

    public int? EmployerRating { get; set; }

    public int? WorkerRating { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<ApplicationNote> ApplicationNoteApplicationId1Navigations { get; set; } = new List<ApplicationNote>();

    public virtual ICollection<ApplicationNote> ApplicationNoteApplications { get; set; } = new List<ApplicationNote>();

    public virtual JobPost JobPost { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
