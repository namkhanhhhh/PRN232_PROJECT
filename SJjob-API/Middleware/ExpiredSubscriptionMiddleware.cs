using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace SJjob_API.Middleware
{
    public class ExpiredSubscriptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExpiredSubscriptionMiddleware> _logger;

        public ExpiredSubscriptionMiddleware(RequestDelegate next, ILogger<ExpiredSubscriptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, SjobPContext dbContext)
        {
            // Only check for authenticated users
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = context.User.FindFirst("UserId")?.Value;
                if (int.TryParse(userIdClaim, out int userId))
                {
                    await CheckAndUpdateExpiredItems(dbContext, userId);
                }
            }

            await _next(context);
        }

        private async Task CheckAndUpdateExpiredItems(SjobPContext context, int userId)
        {
            try
            {
                var now = DateTime.Now;

                // Check expired subscriptions
                var expiredSubscriptions = await context.Subscriptions
                    .Where(s => s.UserId == userId && s.Status == "active" && s.EndDate < now)
                    .ToListAsync();

                foreach (var subscription in expiredSubscriptions)
                {
                    subscription.Status = "expired";
                    _logger.LogInformation($"Subscription {subscription.Id} for user {userId} has expired");
                }

                // Check expired service orders
                var expiredServiceOrders = await context.ServiceOrders
                    .Where(so => so.UserId == userId && so.Status == "active" &&
                                so.EndDate.HasValue && so.EndDate < now)
                    .ToListAsync();

                foreach (var serviceOrder in expiredServiceOrders)
                {
                    serviceOrder.Status = "expired";
                    _logger.LogInformation($"Service order {serviceOrder.Id} for user {userId} has expired");
                }

                if (expiredSubscriptions.Any() || expiredServiceOrders.Any())
                {
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking expired items for user {userId}");
            }
        }
    }
}