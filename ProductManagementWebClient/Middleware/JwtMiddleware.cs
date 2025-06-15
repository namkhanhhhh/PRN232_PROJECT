using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Net.Http.Headers;

namespace ProductManagementWebClient.Middleware
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;

        public JwtMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            var token = context.Session.GetString("Token");

            if (!string.IsNullOrEmpty(token))
            {
                // Validate token with API
                var httpClient = httpClientFactory.CreateClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var baseUrl = configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7009";
                var response = await httpClient.GetAsync($"{baseUrl}/api/Auth/validate-token");

                if (!response.IsSuccessStatusCode)
                {
                    // Token is invalid or expired, clear session
                    context.Session.Clear();
                    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    context.Response.Redirect("/Login");
                    return;
                }
            }

            await _next(context);
        }
    }

    public static class JwtMiddlewareExtensions
    {
        public static IApplicationBuilder UseJwtMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<JwtMiddleware>();
        }
    }
}
