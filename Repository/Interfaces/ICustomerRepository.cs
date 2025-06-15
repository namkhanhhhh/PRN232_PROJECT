using BusinessObjects.DTOs.Customer;
using BusinessObjects.Models;

namespace Repository.Interfaces
{
    public interface ICustomerRepository
    {
        Task<List<RoleDto>> GetAllRolesAsync();
        Task<bool> AssignRoleAsync(int userId, int roleId);
        Task<bool> UserHasRoleAsync(int userId);
        Task<User?> GetUserByIdAsync(int userId);
    }
}
