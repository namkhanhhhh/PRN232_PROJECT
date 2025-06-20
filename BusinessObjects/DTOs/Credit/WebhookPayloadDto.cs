namespace BusinessObjects.DTOs.Credit
{
    public class WebhookPayloadDto
    {
        public string OrderCode { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
