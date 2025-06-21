using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.DTOs.CustomerService
{
    public class AdditionalServiceDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int? DurationDays { get; set; }
        public string? ServiceType { get; set; }
        public int SilverPostsIncluded { get; set; }
        public int GoldPostsIncluded { get; set; }
        public int DiamondPostsIncluded { get; set; }
        public int PushToTopAvailable { get; set; }
        public int AuthenLogoAvailable { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class SubscriptionPlanDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int DurationDays { get; set; }
        public int SilverPosts { get; set; }
        public int GoldPosts { get; set; }
        public int DiamondPosts { get; set; }
        public int PushTopTimes { get; set; }
        public bool? MarketingPackage { get; set; }
        public int? PriorityLevel { get; set; }
        public string? Description { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class CustomerServiceIndexDto
    {
        public List<AdditionalServiceDto> AdditionalServices { get; set; } = new List<AdditionalServiceDto>();
        public List<SubscriptionPlanDto> SubscriptionPlans { get; set; } = new List<SubscriptionPlanDto>();
        public decimal UserBalance { get; set; }
    }

    public class PurchaseRequestDto
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public string Type { get; set; } = string.Empty; // "service" or "combo"

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; } = 1;
    }

    public class UserSubscriptionDto
    {
        public int Id { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public int DaysRemaining { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
    }

    public class UserServiceDto
    {
        public int Id { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public int? DaysRemaining { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
    }

    public class UserPackagesDto
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public decimal UserBalance { get; set; }
        public UserPostCreditDto UserCredits { get; set; } = new UserPostCreditDto();
        public List<UserSubscriptionDto> Subscriptions { get; set; } = new List<UserSubscriptionDto>();
        public List<UserServiceDto> AdditionalServices { get; set; } = new List<UserServiceDto>();
    }

    public class UserPostCreditDto
    {
        public int SilverPostsAvailable { get; set; }
        public int GoldPostsAvailable { get; set; }
        public int DiamondPostsAvailable { get; set; }
        public int PushToTopAvailable { get; set; }
        public int AuthenLogoAvailable { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class BuyViewDto
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int? DurationDays { get; set; }
        public decimal UserBalance { get; set; }
        public string ServiceType { get; set; } = string.Empty;

        // Service specific properties
        public int SilverPostsIncluded { get; set; }
        public int GoldPostsIncluded { get; set; }
        public int DiamondPostsIncluded { get; set; }
        public int PushToTopAvailable { get; set; }
        public int AuthenLogoAvailable { get; set; }

        // Combo specific properties
        public int SilverPosts { get; set; }
        public int GoldPosts { get; set; }
        public int DiamondPosts { get; set; }
        public int PushTopTimes { get; set; }
        public bool? MarketingPackage { get; set; }
        public int? PriorityLevel { get; set; }

        // For backward compatibility
        public AdditionalServiceDto? Service { get; set; }
        public SubscriptionPlanDto? Combo { get; set; }
    }
}