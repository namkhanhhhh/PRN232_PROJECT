using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace SJOB_EXE201.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);

                // Chỉ xử lý lỗi nếu chưa bắt đầu response và status code là lỗi
                if (!context.Response.HasStarted && context.Response.StatusCode >= 400)
                {
                    await HandleHttpStatusCodeAsync(context);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred");

                if (!context.Response.HasStarted)
                {
                    context.Response.Clear();
                    context.Response.StatusCode = 500;
                    await HandleErrorAsync(context, "Internal Server Error", "Đã xảy ra lỗi không mong muốn trong ứng dụng.");
                }
                else
                {
                    _logger.LogWarning("Response has already started, cannot return error page.");
                }
            }
        }

        private async Task HandleHttpStatusCodeAsync(HttpContext context)
        {
            var code = context.Response.StatusCode;
            string title = "";
            string message = "";

            switch (code)
            {
                case 400:
                    title = "Bad Request";
                    message = "Yêu cầu không hợp lệ hoặc có định dạng không đúng.";
                    break;
                case 401:
                    title = "Unauthorized";
                    message = "Bạn cần đăng nhập để truy cập tài nguyên này.";
                    break;
                case 403:
                    title = "Forbidden";
                    message = "Bạn không có quyền truy cập vào tài nguyên này.";
                    break;
                case 404:
                    title = "Not Found";
                    message = "Trang bạn đang tìm kiếm không tồn tại hoặc đã bị di chuyển.";
                    break;
                case 405:
                    title = "Method Not Allowed";
                    message = "Phương thức HTTP không được phép với tài nguyên này.";
                    break;
                case 408:
                    title = "Request Timeout";
                    message = "Yêu cầu đã hết thời gian chờ.";
                    break;
                case 429:
                    title = "Too Many Requests";
                    message = "Bạn đã gửi quá nhiều yêu cầu. Vui lòng thử lại sau.";
                    break;
                case 500:
                    title = "Internal Server Error";
                    message = "Đã xảy ra lỗi máy chủ nội bộ.";
                    break;
                case 502:
                    title = "Bad Gateway";
                    message = "Máy chủ nhận được phản hồi không hợp lệ.";
                    break;
                case 503:
                    title = "Service Unavailable";
                    message = "Dịch vụ hiện không khả dụng. Vui lòng thử lại sau.";
                    break;
                case 504:
                    title = "Gateway Timeout";
                    message = "Máy chủ không nhận được phản hồi kịp thời.";
                    break;
                default:
                    return;
            }

            await HandleErrorAsync(context, title, message);
        }

        private async Task HandleErrorAsync(HttpContext context, string errorTitle, string errorMessage)
        {
            _logger.LogInformation($"Handling {context.Response.StatusCode} error for request {context.Request.Path}");

            if (!context.Response.HasStarted)
            {
                context.Response.ContentType = "text/html";

                var html = $@"
        <!DOCTYPE html>
        <html lang='vi'>
        <head>
            <meta charset='UTF-8'>
            <meta name='viewport' content='width=device-width, initial-scale=1.0'>
            <title>{context.Response.StatusCode} - {errorTitle}</title>
            <style>
                * {{
                    margin: 0;
                    padding: 0;
                    box-sizing: border-box;
                    font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                }}
                body {{
                    background-color: #f8f9fa;
                    color: #212529;
                    display: flex;
                    justify-content: center;
                    align-items: center;
                    height: 100vh;
                }}
                .error-container {{
                    background-color: #ffffff;
                    border-radius: 8px;
                    box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1);
                    padding: 2.5rem;
                    max-width: 550px;
                    width: 100%;
                    text-align: center;
                }}
                .logo-container {{
                    margin-bottom: 2rem;
                }}
                .logo {{
                    max-height: 60px;
                    max-width: 200px;
                }}
                .error-code {{
                    font-size: 8rem;
                    font-weight: 700;
                    color: #dc3545;
                    margin-bottom: 1rem;
                    opacity: 0.8;
                }}
                .error-title {{
                    font-size: 2rem;
                    font-weight: 600;
                    color: #343a40;
                    margin-bottom: 1rem;
                }}
                .error-message {{
                    font-size: 1.1rem;
                    color: #6c757d;
                    margin-bottom: 2rem;
                }}
                .back-button {{
                    background-color: #007bff;
                    color: white;
                    padding: 0.7rem 1.5rem;
                    border-radius: 4px;
                    text-decoration: none;
                    font-weight: 600;
                    transition: background-color 0.2s;
                }}
                .back-button:hover {{
                    background-color: #0069d9;
                }}
                .divider {{
                    width: 100%;
                    height: 4px;
                    background: linear-gradient(90deg, #007bff, #dc3545);
                    border-radius: 2px;
                    margin: 2rem 0;
                }}
            </style>
        </head>
        <body>
            <div class='error-container'>
                <div class='logo-container'>
                    <img src='/assets/img/noBackground.png' alt='Logo' class='logo'>
                </div>
                <div class='error-code'>{context.Response.StatusCode}</div>
                <h1 class='error-title'>{errorTitle}</h1>
                <div class='divider'></div>
                <p class='error-message'>{errorMessage}</p>
                <a href='/' class='back-button'>Trở về trang chủ</a>
            </div>
        </body>
        </html>";

                await context.Response.WriteAsync(html);
            }
            else
            {
                _logger.LogWarning("Response has already started, cannot modify response further.");
            }
        }
    }

    public static class ExceptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseCustomExceptionMiddleware(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ExceptionMiddleware>();
        }
    }
}

