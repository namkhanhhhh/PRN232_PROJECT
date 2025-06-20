using System;

namespace BusinessObjects.DTOs.Application
{
    public class ApplicationNoteDto
    {
        public int Id { get; set; }
        public int ApplicationId { get; set; }
        public string Note { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}
