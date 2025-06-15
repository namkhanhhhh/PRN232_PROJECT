using BusinessObjects.Models;

namespace Repository.Interfaces
{
    public interface IUserRepository
    {
        // User operations
        Task<User?> GetUserByEmailAsync(string email);
        Task<User?> GetUserByIdAsync(int id);
        Task<User?> GetUserByUsernameAsync(string username);
        Task<bool> EmailExistsAsync(string email);
        Task<bool> UsernameExistsAsync(string username);
        Task<User> CreateUserAsync(User user);
        Task<User> UpdateUserAsync(User user);
        Task<bool> DeleteUserAsync(int id);

        // Password operations
        Task<bool> UpdatePasswordAsync(int userId, string hashedPassword);
        Task<bool> UpdatePasswordByEmailAsync(string email, string hashedPassword);

        // Account Management operations
        Task<List<User>> GetAllUsersAsync(int page, int pageSize, string searchTerm = "", string sortBy = "Id", string sortOrder = "asc");
        Task<int> GetTotalUsersCountAsync(string searchTerm = "");
        Task<bool> ToggleUserStatusAsync(int userId);
        Task<bool> UpdateUserProfileAsync(int userId, string firstName, string lastName, string? phoneNumber, string? address);

        // Role operations
        Task<List<Role>> GetAllRolesAsync();
        Task<Role?> GetRoleByNameAsync(string roleName);
        Task<Role?> GetRoleByIdAsync(int roleId);

        // UserDetail operations
        Task<UserDetail> CreateUserDetailAsync(UserDetail userDetail);
        Task<UserDetail?> GetUserDetailByUserIdAsync(int userId);
        Task<UserDetail> UpdateUserDetailAsync(UserDetail userDetail);

        // UserPostCredit operations
        Task<UserPostCredit> CreateUserPostCreditAsync(UserPostCredit userPostCredit);
        Task<UserPostCredit?> GetUserPostCreditByUserIdAsync(int userId);
        Task<UserPostCredit> UpdateUserPostCreditAsync(UserPostCredit userPostCredit);

        // General operations
        Task SaveChangesAsync();
    }
}
