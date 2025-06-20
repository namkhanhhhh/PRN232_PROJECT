namespace BusinessObjects.DTOs.Credit
{
    public class PaymentCallbackDto
    {
        public string OrderCode { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Cancel { get; set; }
        public decimal Amount { get; set; }
    }
}
