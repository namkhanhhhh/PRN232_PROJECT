using BusinessObjects.DTOs.Customer;
using BusinessObjects.DTOs.Common;
using Microsoft.AspNetCore.Mvc;
using ProductManagementWebClient.Helpers;
using System.Security.Claims;
using BusinessObjects.DTOs.Authen;
using System.Text.Json;
using BusinessObjects.DTOs; // Add this for JSON deserialization

namespace ProductManagementWebClient.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ApiHelper _apiHelper;

        public CustomerController(ApiHelper apiHelper)
        {
            _apiHelper = apiHelper;
        }

        // Display role selection page for newly registered users
        [HttpGet]
        public async Task<IActionResult> SelectRole()
        {
            try
            {
                // Get user info from session (since they just logged in)
                var userId = HttpContext.Session.GetInt32("UserId");
                var username = HttpContext.Session.GetString("Username");
                var email = HttpContext.Session.GetString("Email");
                var currentRole = HttpContext.Session.GetString("Role");

                if (userId == null || string.IsNullOrEmpty(username))
                {
                    TempData["ErrorMessage"] = "Session expired. Please login again.";
                    return RedirectToAction("Index", "Login");
                }

                // Check user's current role from database to avoid redirect loop
                try
                {
                    var userRoleResponse = await _apiHelper.GetAsync<ApiResponse<string>>($"api/CustomerApi/user-role/{userId}");

                    if (userRoleResponse?.Success == true && !string.IsNullOrEmpty(userRoleResponse.Data))
                    {
                        var databaseRole = userRoleResponse.Data.ToLower();

                        // If user has been assigned Worker or Employer role in database, update session and redirect
                        if (databaseRole == "worker" || databaseRole == "employer")
                        {
                            // Update session with correct role from database
                            HttpContext.Session.SetString("Role", databaseRole);

                            return databaseRole switch
                            {
                                "worker" => RedirectToAction("Index", "Worker"),
                                "employer" => RedirectToAction("Index", "JobPostManagement"),
                                _ => ShowRoleSelectionView(userId.Value, username, email ?? "")
                            };
                        }

                        // If user has admin role, redirect to admin panel
                        if (databaseRole == "admin")
                        {
                            HttpContext.Session.SetString("Role", "admin");
                            return RedirectToAction("Index", "Account");
                        }

                        // If user has only "customer" role, show role selection
                        if (databaseRole == "customer")
                        {
                            return ShowRoleSelectionView(userId.Value, username, email ?? "");
                        }
                    }
                }
                catch (Exception ex)
                {
                    // If API call fails, continue to show role selection
                    ViewBag.ApiError = $"Could not check existing role status: {ex.Message}";
                }

                // Fallback: Show role selection for any unhandled cases
                return ShowRoleSelectionView(userId.Value, username, email ?? "");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Error loading role selection: {ex.Message}";
                return ShowRoleSelectionView(0, "", "");
            }
        }

        // Helper method to create role selection view using correct DTO
        private IActionResult ShowRoleSelectionView(int userId, string username, string email)
        {
            var model = new RoleSelectionDto
            {
                UserId = userId,
                Username = username,
                Email = email,
                AvailableRoles = new List<RoleDto>
                {
                    new RoleDto { Id = 4, Name = "Worker" },
                    new RoleDto { Id = 3, Name = "Employer" }
                }
            };
            return View(model);
        }

        // Handle role assignment for user
        [HttpPost]
        public async Task<IActionResult> SelectRole(string selectedRole)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");

                if (userId == null)
                {
                    TempData["ErrorMessage"] = "Session expired. Please login again.";
                    return RedirectToAction("Index", "Login");
                }

                if (string.IsNullOrEmpty(selectedRole))
                {
                    TempData["ErrorMessage"] = "Please select a role.";
                    return RedirectToAction("SelectRole");
                }

                // Validate selected role
                if (selectedRole.ToLower() != "worker" && selectedRole.ToLower() != "employer")
                {
                    TempData["ErrorMessage"] = "Invalid role selection.";
                    return RedirectToAction("SelectRole");
                }

                // Map role name to role ID
                int roleId = selectedRole.ToLower() switch
                {
                    "worker" => 4, // Assuming Worker role has ID 4
                    "employer" => 3, // Assuming Employer role has ID 3
                    _ => 4 // Default to Worker
                };

                var assignRoleDto = new AssignRoleDto
                {
                    UserId = userId.Value,
                    RoleId = roleId
                };

                try
                {
                    // Change the expected response type to ApiResponse<LoginResponseDto>
                    var response = await _apiHelper.PostAsync<ApiResponse<LoginResponseDto>>("api/CustomerApi/assign-role", assignRoleDto);

                    if (response?.Success == true && response.Data != null)
                    {
                        // Update session with new role information and the new token
                        HttpContext.Session.SetString("Token", response.Data.Token);
                        HttpContext.Session.SetString("Role", response.Data.User.RoleName.ToLower());
                        HttpContext.Session.SetInt32("UserId", response.Data.User.Id);
                        HttpContext.Session.SetString("Username", response.Data.User.Username);
                        HttpContext.Session.SetString("Email", response.Data.User.Email);
                        HttpContext.Session.SetString("RoleAssigned", "true");

                        TempData["SuccessMessage"] = $"Role {selectedRole} assigned successfully!";

                        // Redirect based on selected role
                        return selectedRole.ToLower() switch
                        {
                            "employer" => RedirectToAction("Index", "JobPostManagement"),
                            "worker" => RedirectToAction("Index", "Worker"),
                            _ => RedirectToAction("Index", "Home")
                        };
                    }
                    else
                    {
                        TempData["ErrorMessage"] = response?.Message ?? "Failed to assign role";
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Error assigning role: {ex.Message}";
                }

                return RedirectToAction("SelectRole");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error processing role selection: {ex.Message}";
                return RedirectToAction("SelectRole");
            }
        }

        // Check if user has role assigned (for AJAX calls)
        [HttpGet]
        public async Task<IActionResult> CheckRoleStatus()
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");

                if (userId == null)
                {
                    return Json(new { hasRole = false, redirectUrl = "/Login" });
                }

                // Check user's actual role from database
                var userRoleResponse = await _apiHelper.GetAsync<ApiResponse<string>>($"api/CustomerApi/user-role/{userId}");

                if (userRoleResponse?.Success == true && !string.IsNullOrEmpty(userRoleResponse.Data))
                {
                    var roleName = userRoleResponse.Data.ToLower();

                    if (roleName == "worker" || roleName == "employer" || roleName == "admin")
                    {
                        return Json(new
                        {
                            hasRole = true,
                            role = roleName,
                            redirectUrl = roleName switch
                            {
                                "worker" => "/Worker",
                                "employer" => "/JobPostManagement",
                                "admin" => "/Account",
                                _ => "/Home"
                            }
                        });
                    }
                }

                return Json(new { hasRole = false, redirectUrl = "/Customer/SelectRole" });
            }
            catch (Exception ex)
            {
                return Json(new { hasRole = false, error = ex.Message });
            }
        }

        // Get available roles for selection
        [HttpGet]
        public async Task<IActionResult> GetAvailableRoles()
        {
            try
            {
                var response = await _apiHelper.GetAsync<ApiResponse<List<RoleDto>>>("api/CustomerApi/roles");

                if (response?.Success == true && response.Data != null)
                {
                    return Json(new { success = true, roles = response.Data });
                }

                return Json(new { success = false, message = "Failed to load roles" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
