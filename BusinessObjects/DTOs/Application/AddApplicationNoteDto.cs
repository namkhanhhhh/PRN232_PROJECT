namespace BusinessObjects.DTOs.Application
{
    public class AddApplicationNoteDto
    {
        public int ApplicationId { get; set; }
        public string Note { get; set; } = null!;
    }
}
