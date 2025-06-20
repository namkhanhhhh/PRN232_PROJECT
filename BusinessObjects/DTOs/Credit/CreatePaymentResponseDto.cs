namespace BusinessObjects.DTOs.Credit
{
    public class CreatePaymentResponseDto
    {
        public string CheckoutUrl { get; set; } = string.Empty;
        public string OrderCode { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
