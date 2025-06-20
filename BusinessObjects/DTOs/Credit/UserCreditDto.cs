namespace BusinessObjects.DTOs.Credit
{
    public class UserCreditDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public decimal Balance { get; set; }
        public DateTime? LastUpdated { get; set; }
    }
}
