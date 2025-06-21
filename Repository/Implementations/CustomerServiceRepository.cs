using BusinessObjects.DTOs.CustomerService;
using BusinessObjects.Models;
using DataAccess.DAO;
using Repository.Interfaces;

namespace Repository.Implementations
{
    public class CustomerServiceRepository : ICustomerServiceRepository
    {
        private readonly CustomerServiceDAO _customerServiceDAO;

        public CustomerServiceRepository(CustomerServiceDAO customerServiceDAO)
        {
            _customerServiceDAO = customerServiceDAO;
        }

        public async Task<CustomerServiceIndexDto> GetCustomerServiceIndexAsync(int userId)
        {
            var additionalServices = await _customerServiceDAO.GetActiveAdditionalServicesAsync();
            var subscriptionPlans = await _customerServiceDAO.GetActiveSubscriptionPlansAsync();
            var userCredit = await _customerServiceDAO.GetUserCreditAsync(userId);

            return new CustomerServiceIndexDto
            {
                AdditionalServices = additionalServices.Select(s => new AdditionalServiceDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Description = s.Description,
                    Price = s.Price,
                    DurationDays = s.DurationDays,
                    ServiceType = s.ServiceType,
                    SilverPostsIncluded = s.SilverPostsIncluded,
                    GoldPostsIncluded = s.GoldPostsIncluded,
                    DiamondPostsIncluded = s.DiamondPostsIncluded,
                    PushToTopAvailable = s.PushToTopAvailable,
                    AuthenLogoAvailable = s.AuthenLogoAvailable,
                    IsActive = s.IsActive,
                    CreatedAt = s.CreatedAt
                }).ToList(),
                SubscriptionPlans = subscriptionPlans.Select(p => new SubscriptionPlanDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    DurationDays = p.DurationDays,
                    SilverPosts = p.SilverPosts,
                    GoldPosts = p.GoldPosts,
                    DiamondPosts = p.DiamondPosts,
                    PushTopTimes = p.PushTopTimes,
                    MarketingPackage = p.MarketingPackage,
                    PriorityLevel = p.PriorityLevel,
                    Description = p.Description,
                    IsActive = p.IsActive,
                    CreatedAt = p.CreatedAt
                }).ToList(),
                UserBalance = userCredit?.Balance ?? 0
            };
        }

        public async Task<BuyViewDto> GetBuyViewDataAsync(int userId, int id, string type)
        {
            var userCredit = await _customerServiceDAO.GetUserCreditAsync(userId);
            var buyViewDto = new BuyViewDto
            {
                Type = type,
                Id = id,
                UserBalance = userCredit?.Balance ?? 0
            };

            if (type.ToLower() == "service")
            {
                var service = await _customerServiceDAO.GetAdditionalServiceByIdAsync(id);
                if (service != null)
                {
                    // Map service properties directly to BuyViewDto
                    buyViewDto.Name = service.Name;
                    buyViewDto.Description = service.Description ?? "";
                    buyViewDto.Price = service.Price;
                    buyViewDto.DurationDays = service.DurationDays;
                    buyViewDto.ServiceType = service.ServiceType ?? "";
                    buyViewDto.SilverPostsIncluded = service.SilverPostsIncluded;
                    buyViewDto.GoldPostsIncluded = service.GoldPostsIncluded;
                    buyViewDto.DiamondPostsIncluded = service.DiamondPostsIncluded;
                    buyViewDto.PushToTopAvailable = service.PushToTopAvailable;
                    buyViewDto.AuthenLogoAvailable = service.AuthenLogoAvailable;

                    // Keep the Service object for backward compatibility
                    buyViewDto.Service = new AdditionalServiceDto
                    {
                        Id = service.Id,
                        Name = service.Name,
                        Description = service.Description,
                        Price = service.Price,
                        DurationDays = service.DurationDays,
                        ServiceType = service.ServiceType,
                        SilverPostsIncluded = service.SilverPostsIncluded,
                        GoldPostsIncluded = service.GoldPostsIncluded,
                        DiamondPostsIncluded = service.DiamondPostsIncluded,
                        PushToTopAvailable = service.PushToTopAvailable,
                        AuthenLogoAvailable = service.AuthenLogoAvailable,
                        IsActive = service.IsActive,
                        CreatedAt = service.CreatedAt
                    };
                }
            }
            else if (type.ToLower() == "combo")
            {
                var combo = await _customerServiceDAO.GetSubscriptionPlanByIdAsync(id);
                if (combo != null)
                {
                    // Map combo properties directly to BuyViewDto
                    buyViewDto.Name = combo.Name;
                    buyViewDto.Description = combo.Description ?? "";
                    buyViewDto.Price = combo.Price;
                    buyViewDto.DurationDays = combo.DurationDays;
                    buyViewDto.SilverPosts = combo.SilverPosts;
                    buyViewDto.GoldPosts = combo.GoldPosts;
                    buyViewDto.DiamondPosts = combo.DiamondPosts;
                    buyViewDto.PushTopTimes = combo.PushTopTimes;
                    buyViewDto.MarketingPackage = combo.MarketingPackage;
                    buyViewDto.PriorityLevel = combo.PriorityLevel;

                    // Keep the Combo object for backward compatibility
                    buyViewDto.Combo = new SubscriptionPlanDto
                    {
                        Id = combo.Id,
                        Name = combo.Name,
                        Price = combo.Price,
                        DurationDays = combo.DurationDays,
                        SilverPosts = combo.SilverPosts,
                        GoldPosts = combo.GoldPosts,
                        DiamondPosts = combo.DiamondPosts,
                        PushTopTimes = combo.PushTopTimes,
                        MarketingPackage = combo.MarketingPackage,
                        PriorityLevel = combo.PriorityLevel,
                        Description = combo.Description,
                        IsActive = combo.IsActive,
                        CreatedAt = combo.CreatedAt
                    };
                }
            }

            return buyViewDto;
        }

        public async Task<bool> ProcessPurchaseAsync(int userId, PurchaseRequestDto request)
        {
            try
            {
                // Get or create user credit
                var userCredit = await _customerServiceDAO.GetUserCreditAsync(userId);
                if (userCredit == null)
                {
                    userCredit = await _customerServiceDAO.CreateOrUpdateUserCreditAsync(userId, 0);
                }

                // Get or create user post credit
                var postCredit = await _customerServiceDAO.GetUserPostCreditAsync(userId);
                if (postCredit == null)
                {
                    postCredit = new UserPostCredit
                    {
                        UserId = userId,
                        SilverPostsAvailable = 0,
                        GoldPostsAvailable = 0,
                        DiamondPostsAvailable = 0,
                        PushToTopAvailable = 0,
                        AuthenLogoAvailable = 0,
                        LastUpdated = DateTime.Now
                    };
                    postCredit = await _customerServiceDAO.CreateOrUpdateUserPostCreditAsync(userId, postCredit);
                }

                if (request.Type.ToLower() == "service")
                {
                    return await ProcessServicePurchase(userId, request.Id, request.Quantity, userCredit, postCredit);
                }
                else if (request.Type.ToLower() == "combo")
                {
                    return await ProcessComboPurchase(userId, request.Id, request.Quantity, userCredit, postCredit);
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async Task<bool> ProcessServicePurchase(int userId, int serviceId, int quantity, UserCredit userCredit, UserPostCredit postCredit)
        {
            var service = await _customerServiceDAO.GetAdditionalServiceByIdAsync(serviceId);
            if (service == null) return false;

            var totalPrice = service.Price * quantity;

            if (userCredit.Balance < totalPrice)
                return false;

            // Update user credit
            userCredit.Balance -= totalPrice;
            await _customerServiceDAO.CreateOrUpdateUserCreditAsync(userId, userCredit.Balance);

            // Update post credits based on service type
            switch (service.ServiceType?.ToLower())
            {
                case "silver_post":
                    var silverToAdd = service.SilverPostsIncluded > 0 ? service.SilverPostsIncluded : 1;
                    postCredit.SilverPostsAvailable += silverToAdd * quantity;
                    break;
                case "gold_post":
                    var goldToAdd = service.GoldPostsIncluded > 0 ? service.GoldPostsIncluded : 1;
                    postCredit.GoldPostsAvailable += goldToAdd * quantity;
                    break;
                case "diamond_post":
                    var diamondToAdd = service.DiamondPostsIncluded > 0 ? service.DiamondPostsIncluded : 1;
                    postCredit.DiamondPostsAvailable += diamondToAdd * quantity;
                    break;
                case "push_to_top":
                    var pushToAdd = service.PushToTopAvailable > 0 ? service.PushToTopAvailable : 1;
                    postCredit.PushToTopAvailable += pushToAdd * quantity;
                    break;
                case "verified_badge":
                    var badgeToAdd = service.AuthenLogoAvailable > 0 ? service.AuthenLogoAvailable : 1;
                    postCredit.AuthenLogoAvailable += badgeToAdd * quantity;
                    break;
                default:
                    postCredit.SilverPostsAvailable += quantity;
                    break;
            }

            await _customerServiceDAO.CreateOrUpdateUserPostCreditAsync(userId, postCredit);

            // Create service order
            var startDate = DateTime.Now;
            var endDate = service.DurationDays.HasValue
                ? startDate.AddDays(service.DurationDays.Value * quantity)
                : (DateTime?)null;

            var serviceOrder = new ServiceOrder
            {
                UserId = userId,
                ServiceId = service.Id,
                StartDate = startDate,
                EndDate = endDate,
                Status = "active",
                CreatedAt = DateTime.Now,
                DiamondPostsApplied = service.ServiceType == "diamond_post" ? (service.DiamondPostsIncluded > 0 ? service.DiamondPostsIncluded : 1) * quantity : 0,
                GoldPostsApplied = service.ServiceType == "gold_post" ? (service.GoldPostsIncluded > 0 ? service.GoldPostsIncluded : 1) * quantity : 0,
                SilverPostsApplied = service.ServiceType == "silver_post" ? (service.SilverPostsIncluded > 0 ? service.SilverPostsIncluded : 1) * quantity : 0
            };

            await _customerServiceDAO.CreateServiceOrderAsync(serviceOrder);

            // Create transaction record
            var transaction = new CreditTransaction
            {
                UserId = userId,
                Amount = totalPrice,
                TransactionType = "purchase",
                ReferenceId = service.Id,
                ReferenceType = "service",
                BalanceAfter = userCredit.Balance,
                Description = $"Mua dịch vụ: {service.Name} x {quantity}",
                CreatedAt = DateTime.Now
            };

            await _customerServiceDAO.CreateCreditTransactionAsync(transaction);

            return true;
        }

        private async Task<bool> ProcessComboPurchase(int userId, int comboId, int quantity, UserCredit userCredit, UserPostCredit postCredit)
        {
            var combo = await _customerServiceDAO.GetSubscriptionPlanByIdAsync(comboId);
            if (combo == null) return false;

            var totalPrice = combo.Price * quantity;

            if (userCredit.Balance < totalPrice)
                return false;

            // Update user credit
            userCredit.Balance -= totalPrice;
            await _customerServiceDAO.CreateOrUpdateUserCreditAsync(userId, userCredit.Balance);

            // Update post credits
            postCredit.SilverPostsAvailable += combo.SilverPosts * quantity;
            postCredit.GoldPostsAvailable += combo.GoldPosts * quantity;
            postCredit.DiamondPostsAvailable += combo.DiamondPosts * quantity;
            postCredit.PushToTopAvailable += combo.PushTopTimes * quantity;

            await _customerServiceDAO.CreateOrUpdateUserPostCreditAsync(userId, postCredit);

            // Create subscription
            var startDate = DateTime.Now;
            var endDate = startDate.AddDays(combo.DurationDays * quantity);

            var subscription = new Subscription
            {
                UserId = userId,
                PlanId = combo.Id,
                SilverPostsRemaining = combo.SilverPosts * quantity,
                GoldPostsRemaining = combo.GoldPosts * quantity,
                DiamondPostsRemaining = combo.DiamondPosts * quantity,
                PushTopRemaining = combo.PushTopTimes * quantity,
                StartDate = startDate,
                EndDate = endDate,
                Status = "active",
                CreatedAt = DateTime.Now
            };

            await _customerServiceDAO.CreateSubscriptionAsync(subscription);

            // Create transaction record
            var transaction = new CreditTransaction
            {
                UserId = userId,
                Amount = totalPrice,
                TransactionType = "purchase",
                ReferenceId = combo.Id,
                ReferenceType = "combo",
                BalanceAfter = userCredit.Balance,
                Description = $"Mua combo: {combo.Name} x {quantity}",
                CreatedAt = DateTime.Now
            };

            await _customerServiceDAO.CreateCreditTransactionAsync(transaction);

            return true;
        }

        public async Task<UserPackagesDto> GetUserPackagesAsync(int userId)
        {
            var user = await _customerServiceDAO.GetUserByIdAsync(userId);
            var userCredit = await _customerServiceDAO.GetUserCreditAsync(userId);
            var userPostCredit = await _customerServiceDAO.GetUserPostCreditAsync(userId);
            var subscriptions = await _customerServiceDAO.GetUserSubscriptionsAsync(userId);
            var serviceOrders = await _customerServiceDAO.GetUserServiceOrdersAsync(userId);

            var today = DateTime.Now.Date;

            return new UserPackagesDto
            {
                Username = user?.Username ?? "Unknown",
                Email = user?.Email ?? "Unknown",
                UserBalance = userCredit?.Balance ?? 0,
                UserCredits = userPostCredit != null ? new UserPostCreditDto
                {
                    SilverPostsAvailable = userPostCredit.SilverPostsAvailable,
                    GoldPostsAvailable = userPostCredit.GoldPostsAvailable,
                    DiamondPostsAvailable = userPostCredit.DiamondPostsAvailable,
                    PushToTopAvailable = userPostCredit.PushToTopAvailable,
                    AuthenLogoAvailable = userPostCredit.AuthenLogoAvailable,
                    LastUpdated = userPostCredit.LastUpdated ?? DateTime.Now
                } : new UserPostCreditDto(),
                Subscriptions = subscriptions.Select(s => new UserSubscriptionDto
                {
                    Id = s.Id,
                    PlanName = s.Plan?.Name ?? "Unknown Plan",
                    StartDate = s.StartDate ?? s.CreatedAt ?? DateTime.Now,
                    EndDate = s.EndDate,
                    Status = s.Status ?? "Unknown",
                    DaysRemaining = (s.EndDate.Date - today).Days,
                    Description = s.Plan?.Description,
                    Price = s.Plan?.Price ?? 0
                }).ToList(),
                AdditionalServices = serviceOrders.Select(so => new UserServiceDto
                {
                    Id = so.Id,
                    ServiceName = so.Service?.Name ?? "Unknown Service",
                    StartDate = so.StartDate ?? so.CreatedAt ?? DateTime.Now,
                    EndDate = so.EndDate,
                    Status = so.Status ?? "Unknown",
                    DaysRemaining = so.EndDate.HasValue ? (so.EndDate.Value.Date - today).Days : null,
                    Description = so.Service?.Description,
                    Price = so.Service?.Price ?? 0
                }).ToList()
            };
        }

        public async Task<bool> SyncPurchaseHistoryAsync(int userId)
        {
            try
            {
                var transactions = await _customerServiceDAO.GetUserPurchaseTransactionsAsync(userId);

                int subscriptionsCreated = 0;
                int serviceOrdersCreated = 0;

                foreach (var transaction in transactions)
                {
                    if (!transaction.CreatedAt.HasValue) continue;

                    if (transaction.ReferenceType == "combo")
                    {
                        var exists = await _customerServiceDAO.SubscriptionExistsAsync(
                            userId, transaction.ReferenceId ?? 0, transaction.CreatedAt.Value);

                        if (!exists)
                        {
                            var plan = await _customerServiceDAO.GetSubscriptionPlanByIdAsync(transaction.ReferenceId ?? 0);
                            if (plan != null)
                            {
                                var startDate = transaction.CreatedAt.Value;
                                var endDate = startDate.AddDays(plan.DurationDays);

                                var subscription = new Subscription
                                {
                                    UserId = userId,
                                    PlanId = plan.Id,
                                    SilverPostsRemaining = plan.SilverPosts,
                                    GoldPostsRemaining = plan.GoldPosts,
                                    DiamondPostsRemaining = plan.DiamondPosts,
                                    PushTopRemaining = plan.PushTopTimes,
                                    StartDate = startDate,
                                    EndDate = endDate,
                                    Status = DateTime.Now > endDate ? "expired" : "active",
                                    CreatedAt = transaction.CreatedAt
                                };

                                await _customerServiceDAO.CreateSubscriptionAsync(subscription);
                                subscriptionsCreated++;
                            }
                        }
                    }
                    else if (transaction.ReferenceType == "service")
                    {
                        var exists = await _customerServiceDAO.ServiceOrderExistsAsync(
                            userId, transaction.ReferenceId ?? 0, transaction.CreatedAt.Value);

                        if (!exists)
                        {
                            var service = await _customerServiceDAO.GetAdditionalServiceByIdAsync(transaction.ReferenceId ?? 0);
                            if (service != null)
                            {
                                var startDate = transaction.CreatedAt.Value;
                                var endDate = service.DurationDays.HasValue
                                    ? startDate.AddDays(service.DurationDays.Value)
                                    : (DateTime?)null;

                                var serviceOrder = new ServiceOrder
                                {
                                    UserId = userId,
                                    ServiceId = service.Id,
                                    StartDate = startDate,
                                    EndDate = endDate,
                                    Status = endDate.HasValue && DateTime.Now > endDate ? "expired" : "active",
                                    CreatedAt = transaction.CreatedAt,
                                    DiamondPostsApplied = service.ServiceType == "diamond_post" ? (service.DiamondPostsIncluded > 0 ? service.DiamondPostsIncluded : 1) : 0,
                                    GoldPostsApplied = service.ServiceType == "gold_post" ? (service.GoldPostsIncluded > 0 ? service.GoldPostsIncluded : 1) : 0,
                                    SilverPostsApplied = service.ServiceType == "silver_post" ? (service.SilverPostsIncluded > 0 ? service.SilverPostsIncluded : 1) : 0
                                };

                                await _customerServiceDAO.CreateServiceOrderAsync(serviceOrder);
                                serviceOrdersCreated++;
                            }
                        }
                    }
                }

                return subscriptionsCreated > 0 || serviceOrdersCreated > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
