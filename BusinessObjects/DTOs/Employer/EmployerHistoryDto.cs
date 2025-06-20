namespace BusinessObjects.DTOs.Employer
{
    public class EmployerHistoryDto
    {
        public List<JobPostHistoryDto> JobPosts { get; set; } = new List<JobPostHistoryDto>();
        public List<ServiceOrderHistoryDto> ServiceOrders { get; set; } = new List<ServiceOrderHistoryDto>();
        public List<CreditTransactionHistoryDto> CreditTransactions { get; set; } = new List<CreditTransactionHistoryDto>();
    }

    public class JobPostHistoryDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal? SalaryMin { get; set; }
        public decimal? SalaryMax { get; set; }
        public string? Location { get; set; }
        public string? Status { get; set; }
        public string? PostType { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? ViewCount { get; set; }
        public int ApplicationCount { get; set; }
        public bool? IsFeatured { get; set; }

        // Computed properties
        public string SalaryRange =>
            SalaryMin.HasValue && SalaryMax.HasValue
                ? $"{SalaryMin:N0} - {SalaryMax:N0} VNĐ"
                : SalaryMin.HasValue
                    ? $"Từ {SalaryMin:N0} VNĐ"
                    : SalaryMax.HasValue
                        ? $"Tối đa {SalaryMax:N0} VNĐ"
                        : "Thỏa thuận";

        public string StatusDisplay => Status switch
        {
            "draft" => "Nháp",
            "active" => "Đang hoạt động",
            "expired" => "Hết hạn",
            "closed" => "Đã đóng",
            _ => Status ?? "Không xác định"
        };

        public string PostTypeDisplay => PostType switch
        {
            "silver" => "Bạc",
            "gold" => "Vàng",
            "diamond" => "Kim cương",
            _ => PostType ?? "Bạc"
        };
    }

    public class ServiceOrderHistoryDto
    {
        public int Id { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? DiamondPostsApplied { get; set; }
        public int? GoldPostsApplied { get; set; }
        public int? SilverPostsApplied { get; set; }

        // Computed properties
        public string StatusDisplay => Status switch
        {
            "pending" => "Chờ xử lý",
            "active" => "Đang hoạt động",
            "completed" => "Hoàn thành",
            "cancelled" => "Đã hủy",
            _ => Status ?? "Không xác định"
        };

        public string PostsAppliedSummary
        {
            get
            {
                var parts = new List<string>();
                if (DiamondPostsApplied > 0) parts.Add($"{DiamondPostsApplied} Kim cương");
                if (GoldPostsApplied > 0) parts.Add($"{GoldPostsApplied} Vàng");
                if (SilverPostsApplied > 0) parts.Add($"{SilverPostsApplied} Bạc");
                return parts.Any() ? string.Join(", ", parts) : "Không có";
            }
        }
    }

    public class CreditTransactionHistoryDto
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string? TransactionType { get; set; }
        public decimal BalanceAfter { get; set; }
        public string? Description { get; set; }
        public DateTime? CreatedAt { get; set; }

        // Computed properties
        public string TransactionTypeDisplay => TransactionType switch
        {
            "deposit" => "Nạp tiền",
            "withdraw" => "Rút tiền",
            "purchase" => "Mua dịch vụ",
            "refund" => "Hoàn tiền",
            _ => TransactionType ?? "Không xác định"
        };

        public string AmountDisplay => TransactionType == "deposit" || TransactionType == "refund"
            ? $"+{Amount:N0} VNĐ"
            : $"-{Amount:N0} VNĐ";
    }
}
