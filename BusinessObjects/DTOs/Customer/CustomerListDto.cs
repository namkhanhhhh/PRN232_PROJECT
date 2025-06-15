namespace BusinessObjects.DTOs.Customer
{
    public class CustomerListDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public bool Status { get; set; }
        public string StatusText => Status ? "Active" : "Inactive";
        public string RoleName { get; set; } = string.Empty;
        public decimal CreditBalance { get; set; }
        public int JobPostCount { get; set; }
        public string? Avatar { get; set; }
        public string? AuthProvider { get; set; }
    }
}
