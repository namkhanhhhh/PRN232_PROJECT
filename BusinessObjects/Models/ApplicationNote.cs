using System;
using System.Collections.Generic;

namespace BusinessObjects.Models;

public partial class ApplicationNote
{
    public int Id { get; set; }

    public int ApplicationId { get; set; }

    public string Note { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public int? ApplicationId1 { get; set; }

    public virtual Application Application { get; set; } = null!;

    public virtual Application? ApplicationId1Navigation { get; set; }
}
