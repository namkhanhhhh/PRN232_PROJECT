using System;
using System.Collections.Generic;

namespace BusinessObjects.Models;

public partial class UserDetail
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Address { get; set; }

    public string? Headline { get; set; }

    public int? ExperienceYears { get; set; }

    public string? Education { get; set; }

    public string? Skills { get; set; }

    public string? DesiredPosition { get; set; }

    public decimal? DesiredSalary { get; set; }

    public string? DesiredLocation { get; set; }

    public string? Availability { get; set; }

    public string? Bio { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
