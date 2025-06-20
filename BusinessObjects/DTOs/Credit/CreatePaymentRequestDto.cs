namespace BusinessObjects.DTOs.Credit
{
    public class CreatePaymentRequestDto
    {
        public decimal Amount { get; set; }
        public string CancelUrl { get; set; } = string.Empty;
        public string SuccessUrl { get; set; } = string.Empty;
    }
}
