using System;
using System.Collections.Generic;

namespace BusinessObjects.Models;

public partial class UserPostCredit
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int SilverPostsAvailable { get; set; }

    public int GoldPostsAvailable { get; set; }

    public int DiamondPostsAvailable { get; set; }

    public DateTime? LastUpdated { get; set; }

    public int AuthenLogoAvailable { get; set; }

    public int PushToTopAvailable { get; set; }

    public virtual User User { get; set; } = null!;
}
