using SJOB_EXE201.Services;

namespace SJOB_EXE201.Middleware
{
    // Middleware/ExpiredSubscriptionMiddleware.cs
    public class ExpiredSubscriptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceProvider _serviceProvider;

        public ExpiredSubscriptionMiddleware(RequestDelegate next, IServiceProvider serviceProvider)
        {
            _next = next;
            _serviceProvider = serviceProvider;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Chỉ kiểm tra nếu người dùng đã đăng nhập
            if (context.User.Identity.IsAuthenticated)
            {
                // Lấy ID người dùng từ claims
                var userIdClaim = context.User.FindFirst("UserId");
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    // Kiểm tra xem đã kiểm tra gần đây chưa (để tránh kiểm tra quá nhiều)
                    var lastCheck = context.Session.GetString("LastExpiredCheck");
                    var now = DateTime.Now;

                    if (string.IsNullOrEmpty(lastCheck) || DateTime.Parse(lastCheck).AddHours(1) < now)
                    {
                        using (var scope = _serviceProvider.CreateScope())
                        {
                            var service = scope.ServiceProvider.GetRequiredService<ExpiredSubscriptionService>();
                            await service.ProcessExpiredSubscriptionsForUser(userId);
                        }

                        // Lưu thời gian kiểm tra
                        context.Session.SetString("LastExpiredCheck", now.ToString());
                    }
                }
            }

            await _next(context);
        }
    }

}
