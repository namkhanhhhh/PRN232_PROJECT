using System;
using System.Collections.Generic;

namespace BusinessObjects.Models;

public partial class Wishlist
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int JobPostId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? JobPostId1 { get; set; }

    public virtual JobPost JobPost { get; set; } = null!;

    public virtual JobPost? JobPostId1Navigation { get; set; }

    public virtual User User { get; set; } = null!;
}
