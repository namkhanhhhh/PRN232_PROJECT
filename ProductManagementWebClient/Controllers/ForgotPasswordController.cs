using BusinessObjects.DTOs;
using BusinessObjects.DTOs.Authen;
using Microsoft.AspNetCore.Mvc;
using ProductManagementWebClient.Helpers;

namespace ProductManagementWebClient.Controllers
{
    public class ForgotPasswordController : Controller
    {
        private readonly ApiHelper _apiHelper;

        public ForgotPasswordController(ApiHelper apiHelper)
        {
            _apiHelper = apiHelper;
        }

        [HttpGet]
        public IActionResult ViewForgotPassword()
        {
            return View("~/Views/ForgotPassword/Index.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> GoForgotPassword(string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                {
                    ModelState.AddModelError("", "Email là bắt buộc");
                    return View("~/Views/ForgotPassword/Index.cshtml");
                }

                var request = new ForgotPasswordDto { Email = email };
                var response = await _apiHelper.PostAsync<ForgotPasswordResponseDto>("api/Auth/forgot-password", request);

                if (response?.Success == true)
                {
                    TempData["Message"] = response.Message;
                    ViewBag.Email = email;
                    return View("~/Views/ForgotPassword/VerifyOtp.cshtml");
                }

                ModelState.AddModelError("", response?.Message ?? "Không thể gửi email đặt lại mật khẩu");
                return View("~/Views/ForgotPassword/Index.cshtml");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Đã xảy ra lỗi khi xử lý yêu cầu");
                return View("~/Views/ForgotPassword/Index.cshtml");
            }
        }

        [HttpPost]
        public async Task<IActionResult> VerifyOtp(string email, string otp)
        {
            try
            {
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(otp))
                {
                    ModelState.AddModelError("", "Email và mã OTP là bắt buộc");
                    ViewBag.Email = email;
                    return View("~/Views/ForgotPassword/VerifyOtp.cshtml");
                }

                var request = new VerifyOtpDto { Email = email, Otp = otp };
                var response = await _apiHelper.PostAsync<ApiResponseDto>("api/Auth/verify-otp", request);

                if (response?.Success == true)
                {
                    return RedirectToAction("ResetPassword", new { email });
                }

                ModelState.AddModelError("", response?.Message ?? "Mã OTP không hợp lệ");
                ViewBag.Email = email;
                return View("~/Views/ForgotPassword/VerifyOtp.cshtml");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Đã xảy ra lỗi khi xác thực OTP");
                ViewBag.Email = email;
                return View("~/Views/ForgotPassword/VerifyOtp.cshtml");
            }
        }

        [HttpGet]
        public IActionResult ResetPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                TempData["ErrorMessage"] = "Email không hợp lệ";
                return RedirectToAction("ViewForgotPassword");
            }

            ViewBag.Email = email;
            return View("~/Views/ForgotPassword/ResetPassword.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(string email, string newPassword, string confirmPassword)
        {
            try
            {
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
                {
                    ModelState.AddModelError("", "Tất cả các trường là bắt buộc");
                    ViewBag.Email = email;
                    return View("~/Views/ForgotPassword/ResetPassword.cshtml");
                }

                if (newPassword != confirmPassword)
                {
                    ModelState.AddModelError("", "Mật khẩu xác nhận không khớp");
                    ViewBag.Email = email;
                    return View("~/Views/ForgotPassword/ResetPassword.cshtml");
                }

                if (newPassword.Length < 6)
                {
                    ModelState.AddModelError("", "Mật khẩu phải có ít nhất 6 ký tự");
                    ViewBag.Email = email;
                    return View("~/Views/ForgotPassword/ResetPassword.cshtml");
                }

                var request = new ResetPasswordDto
                {
                    Email = email,
                    NewPassword = newPassword,
                    ConfirmPassword = confirmPassword
                };

                var response = await _apiHelper.PostAsync<ApiResponseDto>("api/Auth/reset-password", request);

                if (response?.Success == true)
                {
                    TempData["Message"] = response.Message;
                    return View("~/Views/Login/Index.cshtml");
                }

                ModelState.AddModelError("", response?.Message ?? "Không thể đặt lại mật khẩu");
                ViewBag.Email = email;
                return View("~/Views/ForgotPassword/ResetPassword.cshtml");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Đã xảy ra lỗi khi đặt lại mật khẩu");
                ViewBag.Email = email;
                return View("~/Views/ForgotPassword/ResetPassword.cshtml");
            }
        }
    }
}
