using BusinessObjects.DTOs;
using BusinessObjects.DTOs.Authen;
using BusinessObjects.Models;
using Microsoft.AspNetCore.Mvc;
using Repository.Interfaces;
using Sjob_API.Services;
using Sjob_API.Utils;
using System.Security.Cryptography;
using System.Text;

namespace Sjob_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly JwtService _jwtService;
        private readonly EmailService _emailService;
        private static readonly List<PasswordResetModel> _resetRequests = new();

        public AuthController(IUserRepository userRepository, JwtService jwtService, EmailService emailService)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
            _emailService = emailService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new LoginResponseDto
                    {
                        Success = false,
                        Message = "Invalid request data"
                    });
                }

                var user = await _userRepository.GetUserByEmailAsync(request.Email);

                if (user == null)
                {
                    return BadRequest(new LoginResponseDto
                    {
                        Success = false,
                        Message = "Wrong email or password!"
                    });
                }

                if (!VerifyPassword(request.Password, user.Password))
                {
                    return BadRequest(new LoginResponseDto
                    {
                        Success = false,
                        Message = "Wrong email or password!"
                    });
                }

                if (!user.Status)
                {
                    return BadRequest(new LoginResponseDto
                    {
                        Success = false,
                        Message = "Your account has been banned. Please contact Admin"
                    });
                }

                var userDto = await MapToUserDtoAsync(user);

                // Generate JWT token
                string token = _jwtService.GenerateJwtToken(user, user.Role?.Name ?? "Customer");

                return Ok(new LoginResponseDto
                {
                    Success = true,
                    Message = "Login successful",
                    User = userDto,
                    Token = token
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new LoginResponseDto
                {
                    Success = false,
                    Message = "An error occurred during login"
                });
            }
        }

        [HttpPost("register")]
        public async Task<ActionResult<RegisterResponseDto>> Register([FromBody] RegisterRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new RegisterResponseDto
                    {
                        Success = false,
                        Message = "Invalid request data"
                    });
                }

                // Check if email already exists
                if (await _userRepository.EmailExistsAsync(request.Email))
                {
                    return BadRequest(new RegisterResponseDto
                    {
                        Success = false,
                        Message = "Email này đã được sử dụng!"
                    });
                }

                // Check password confirmation
                if (request.Password != request.ConfirmPassword)
                {
                    return BadRequest(new RegisterResponseDto
                    {
                        Success = false,
                        Message = "Mật khẩu xác nhận không khớp!"
                    });
                }

                // Validate password length
                if (request.Password.Length < 6)
                {
                    return BadRequest(new RegisterResponseDto
                    {
                        Success = false,
                        Message = "Mật khẩu phải có ít nhất 6 ký tự!"
                    });
                }

                // Get default role (Customer)
                var defaultRole = await _userRepository.GetRoleByNameAsync("Customer");
                if (defaultRole == null)
                {
                    return BadRequest(new RegisterResponseDto
                    {
                        Success = false,
                        Message = "Lỗi hệ thống: không thể xác định vai trò người dùng!"
                    });
                }

                // Create new user
                var newUser = new User
                {
                    Username = request.Username,
                    Password = HashPassword(request.Password),
                    Email = request.Email,
                    RoleId = defaultRole.Id,
                    Status = true,
                    AuthProvider = "local"
                };

                var createdUser = await _userRepository.CreateUserAsync(newUser);

                // Create user details
                var newUserDetail = new UserDetail
                {
                    UserId = createdUser.Id,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    CreatedAt = DateTime.Now
                };

                await _userRepository.CreateUserDetailAsync(newUserDetail);

                // Create user post credits (5 silver posts for new users)
                var postCredit = new UserPostCredit
                {
                    UserId = createdUser.Id,
                    SilverPostsAvailable = 5,
                    GoldPostsAvailable = 0,
                    DiamondPostsAvailable = 0,
                    PushToTopAvailable = 0,
                    AuthenLogoAvailable = 0,
                    LastUpdated = DateTime.Now
                };

                await _userRepository.CreateUserPostCreditAsync(postCredit);

                // Get user with role for response
                var userWithRole = await _userRepository.GetUserByIdAsync(createdUser.Id);
                var userDto = await MapToUserDtoAsync(userWithRole!);

                // Generate JWT token
                string token = _jwtService.GenerateJwtToken(userWithRole!, defaultRole.Name);

                return Ok(new RegisterResponseDto
                {
                    Success = true,
                    Message = "Đăng ký tài khoản thành công! Vui lòng đăng nhập.",
                    User = userDto,
                    Token = token
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new RegisterResponseDto
                {
                    Success = false,
                    Message = "An error occurred during registration"
                });
            }
        }

        [HttpPost("external-login")]
        public async Task<ActionResult<LoginResponseDto>> ExternalLogin([FromBody] ExternalLoginRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new LoginResponseDto
                    {
                        Success = false,
                        Message = "Invalid request data"
                    });
                }

                var user = await _userRepository.GetUserByEmailAsync(request.Email);

                if (user == null)
                {
                    // Get Customer role
                    var customerRole = await _userRepository.GetRoleByNameAsync(DbConstants.User_Role_CUSTOMER);
                    if (customerRole == null)
                    {
                        return BadRequest(new LoginResponseDto
                        {
                            Success = false,
                            Message = "System error: Cannot determine user role"
                        });
                    }

                    // Create new user
                    user = new User
                    {
                        Username = request.Email.Split('@')[0] ?? "Unknown",
                        Email = request.Email,
                        AuthProvider = request.Provider,
                        AuthProviderId = request.ProviderId,
                        RoleId = customerRole.Id,
                        Status = true,
                        Password = "external_auth"
                    };

                    user = await _userRepository.CreateUserAsync(user);

                    // Create user details
                    UserDetail userDetail;
                    if ("Google".Equals(request.Provider))
                    {
                        var nameParts = request.Name.Split(' ');
                        userDetail = new UserDetail
                        {
                            UserId = user.Id,
                            FirstName = nameParts.Length > 0 ? nameParts[^1] : request.Name,
                            LastName = nameParts.Length > 1 ? string.Join(" ", nameParts[..^1]) : ""
                        };
                    }
                    else
                    {
                        userDetail = new UserDetail
                        {
                            UserId = user.Id,
                            FirstName = request.Email.Split('@')[0],
                            LastName = request.Email.Split('@')[0]
                        };
                    }

                    await _userRepository.CreateUserDetailAsync(userDetail);

                    // Add 5 silver posts for new user
                    var postCredit = new UserPostCredit
                    {
                        UserId = user.Id,
                        SilverPostsAvailable = 5,
                        GoldPostsAvailable = 0,
                        DiamondPostsAvailable = 0,
                        PushToTopAvailable = 0,
                        AuthenLogoAvailable = 0,
                        LastUpdated = DateTime.Now
                    };

                    await _userRepository.CreateUserPostCreditAsync(postCredit);
                }

                // Get user with role
                var userWithRole = await _userRepository.GetUserByIdAsync(user.Id);
                var userDto = await MapToUserDtoAsync(userWithRole!);

                // Generate JWT token
                string token = _jwtService.GenerateJwtToken(userWithRole!, userWithRole!.Role?.Name ?? "Customer");

                return Ok(new LoginResponseDto
                {
                    Success = true,
                    Message = "External login successful",
                    User = userDto,
                    Token = token
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new LoginResponseDto
                {
                    Success = false,
                    Message = "An error occurred during external login"
                });
            }
        }

        [HttpPost("forgot-password")]
        public async Task<ActionResult<ForgotPasswordResponseDto>> ForgotPassword([FromBody] ForgotPasswordDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ForgotPasswordResponseDto
                    {
                        Success = false,
                        Message = "Email không hợp lệ"
                    });
                }

                var user = await _userRepository.GetUserByEmailAsync(request.Email);
                if (user == null)
                {
                    return BadRequest(new ForgotPasswordResponseDto
                    {
                        Success = false,
                        Message = "Email không tồn tại trong hệ thống"
                    });
                }

                if (!user.Status)
                {
                    return BadRequest(new ForgotPasswordResponseDto
                    {
                        Success = false,
                        Message = "Tài khoản của bạn đã bị vô hiệu hóa. Vui lòng liên hệ quản trị viên."
                    });
                }

                // Tạo mã OTP ngẫu nhiên 6 số
                var otp = new Random().Next(100000, 999999).ToString();

                // Xóa các request cũ của email này
                _resetRequests.RemoveAll(r => r.Email == request.Email);

                // Thêm request mới
                _resetRequests.Add(new PasswordResetModel
                {
                    Email = request.Email,
                    OTP = otp,
                    Expiration = DateTime.Now.AddMinutes(10)
                });

                // Gửi email chứa mã OTP
                await _emailService.SendEmailAsync(request.Email, "Đặt lại mật khẩu - SJOB",
                    $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                        <h2 style='color: #333; text-align: center;'>Đặt lại mật khẩu</h2>
                        <p>Xin chào,</p>
                        <p>Bạn đã yêu cầu đặt lại mật khẩu cho tài khoản SJOB của mình.</p>
                        <div style='background-color: #f8f9fa; padding: 20px; border-radius: 5px; text-align: center; margin: 20px 0;'>
                            <h3 style='color: #007bff; margin: 0;'>Mã xác nhận của bạn là:</h3>
                            <h1 style='color: #dc3545; font-size: 32px; letter-spacing: 5px; margin: 10px 0;'>{otp}</h1>
                            <p style='color: #6c757d; margin: 0;'>Mã này sẽ hết hạn sau 10 phút</p>
                        </div>
                        <p>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.</p>
                        <hr style='border: none; border-top: 1px solid #eee; margin: 20px 0;'>
                        <p style='color: #6c757d; font-size: 12px; text-align: center;'>
                            Email này được gửi tự động, vui lòng không trả lời.
                        </p>
                    </div>");

                return Ok(new ForgotPasswordResponseDto
                {
                    Success = true,
                    Message = "Mã xác nhận đã được gửi đến email của bạn."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ForgotPasswordResponseDto
                {
                    Success = false,
                    Message = "Đã xảy ra lỗi khi gửi email"
                });
            }
        }

        [HttpPost("verify-otp")]
        public async Task<ActionResult<ApiResponseDto>> VerifyOtp([FromBody] VerifyOtpDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponseDto
                    {
                        Success = false,
                        Message = "Dữ liệu không hợp lệ"
                    });
                }

                var resetRequest = _resetRequests.FirstOrDefault(r =>
                    r.Email == request.Email &&
                    r.OTP == request.Otp &&
                    r.Expiration > DateTime.Now);

                if (resetRequest == null)
                {
                    return BadRequest(new ApiResponseDto
                    {
                        Success = false,
                        Message = "Mã OTP không hợp lệ hoặc đã hết hạn"
                    });
                }

                return Ok(new ApiResponseDto
                {
                    Success = true,
                    Message = "Xác thực OTP thành công"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDto
                {
                    Success = false,
                    Message = "Đã xảy ra lỗi khi xác thực OTP"
                });
            }
        }

        [HttpPost("reset-password")]
        public async Task<ActionResult<ApiResponseDto>> ResetPassword([FromBody] ResetPasswordDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponseDto
                    {
                        Success = false,
                        Message = "Dữ liệu không hợp lệ"
                    });
                }

                // Kiểm tra user tồn tại
                var user = await _userRepository.GetUserByEmailAsync(request.Email);
                if (user == null)
                {
                    return BadRequest(new ApiResponseDto
                    {
                        Success = false,
                        Message = "Email không tồn tại trong hệ thống"
                    });
                }

                // Cập nhật mật khẩu
                var hashedPassword = HashPassword(request.NewPassword);
                var result = await _userRepository.UpdatePasswordByEmailAsync(request.Email, hashedPassword);

                if (result)
                {
                    // Xóa tất cả reset requests của email này sau khi đặt lại thành công
                    _resetRequests.RemoveAll(r => r.Email == request.Email);

                    return Ok(new ApiResponseDto
                    {
                        Success = true,
                        Message = "Mật khẩu đã được đặt lại thành công"
                    });
                }

                return BadRequest(new ApiResponseDto
                {
                    Success = false,
                    Message = "Không thể đặt lại mật khẩu"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDto
                {
                    Success = false,
                    Message = "Đã xảy ra lỗi khi đặt lại mật khẩu"
                });
            }
        }

        #region Private Methods
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                return Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(password)));
            }
        }

        private bool VerifyPassword(string password, string hashPassword)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                var computedHash = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(password)));
                return computedHash == hashPassword;
            }
        }

        private async Task<UserDto> MapToUserDtoAsync(User user)
        {
            var userDetail = await _userRepository.GetUserDetailByUserIdAsync(user.Id);
            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Username,
                Avatar = user.Avatar,
                Status = user.Status,
                RoleName = user.Role?.Name ?? "",
                FirstName = userDetail?.FirstName,
                LastName = userDetail?.LastName
            };
        }
        #endregion
    }
}
