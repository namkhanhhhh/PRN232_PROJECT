using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;

namespace SJjob_API.Services
{
    public class ExpiredSubscriptionService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ExpiredSubscriptionService> _logger;

        public ExpiredSubscriptionService(IServiceProvider serviceProvider, ILogger<ExpiredSubscriptionService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckExpiredSubscriptions();
                    await CheckExpiredServiceOrders();

                    // Run every hour
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while checking expired subscriptions");
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
            }
        }

        private async Task CheckExpiredSubscriptions()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<SjobPContext>();

            var expiredSubscriptions = await context.Subscriptions
                .Where(s => s.Status == "active" && s.EndDate < DateTime.Now)
                .ToListAsync();

            foreach (var subscription in expiredSubscriptions)
            {
                subscription.Status = "expired";
                _logger.LogInformation($"Subscription {subscription.Id} for user {subscription.UserId} has expired");
            }

            if (expiredSubscriptions.Any())
            {
                await context.SaveChangesAsync();
                _logger.LogInformation($"Updated {expiredSubscriptions.Count} expired subscriptions");
            }
        }

        private async Task CheckExpiredServiceOrders()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<SjobPContext>();

            var expiredServiceOrders = await context.ServiceOrders
                .Where(so => so.Status == "active" && so.EndDate.HasValue && so.EndDate < DateTime.Now)
                .ToListAsync();

            foreach (var serviceOrder in expiredServiceOrders)
            {
                serviceOrder.Status = "expired";
                _logger.LogInformation($"Service order {serviceOrder.Id} for user {serviceOrder.UserId} has expired");
            }

            if (expiredServiceOrders.Any())
            {
                await context.SaveChangesAsync();
                _logger.LogInformation($"Updated {expiredServiceOrders.Count} expired service orders");
            }
        }
    }
}