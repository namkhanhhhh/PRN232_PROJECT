using BusinessObjects.Models;
using DataAccess.DAO;
using Repository.Interfaces;

namespace Repository.Implementations
{
    public class UserRepository : IUserRepository
    {
        private readonly UserDAO _userDAO;

        public UserRepository(UserDAO userDAO)
        {
            _userDAO = userDAO;
        }

        // User operations
        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _userDAO.GetUserByEmailAsync(email);
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _userDAO.GetUserByIdAsync(id);
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _userDAO.GetUserByUsernameAsync(username);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _userDAO.EmailExistsAsync(email);
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            return await _userDAO.UsernameExistsAsync(username);
        }

        public async Task<User> CreateUserAsync(User user)
        {
            return await _userDAO.CreateUserAsync(user);
        }

        public async Task<User> UpdateUserAsync(User user)
        {
            return await _userDAO.UpdateUserAsync(user);
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            return await _userDAO.DeleteUserAsync(id);
        }

        // Password operations
        public async Task<bool> UpdatePasswordAsync(int userId, string hashedPassword)
        {
            return await _userDAO.UpdatePasswordAsync(userId, hashedPassword);
        }

        public async Task<bool> UpdatePasswordByEmailAsync(string email, string hashedPassword)
        {
            return await _userDAO.UpdatePasswordByEmailAsync(email, hashedPassword);
        }

        // Account Management operations
        public async Task<List<User>> GetAllUsersAsync(int page, int pageSize, string searchTerm = "", string sortBy = "Id", string sortOrder = "asc")
        {
            return await _userDAO.GetAllUsersAsync(page, pageSize, searchTerm, sortBy, sortOrder);
        }

        public async Task<int> GetTotalUsersCountAsync(string searchTerm = "")
        {
            return await _userDAO.GetTotalUsersCountAsync(searchTerm);
        }

        public async Task<bool> ToggleUserStatusAsync(int userId)
        {
            return await _userDAO.ToggleUserStatusAsync(userId);
        }

        public async Task<bool> UpdateUserProfileAsync(int userId, string firstName, string lastName, string? phoneNumber, string? address)
        {
            return await _userDAO.UpdateUserProfileAsync(userId, firstName, lastName, phoneNumber, address);
        }

        // Role operations
        public async Task<List<Role>> GetAllRolesAsync()
        {
            return await _userDAO.GetAllRolesAsync();
        }

        public async Task<Role?> GetRoleByNameAsync(string roleName)
        {
            return await _userDAO.GetRoleByNameAsync(roleName);
        }

        public async Task<Role?> GetRoleByIdAsync(int roleId)
        {
            return await _userDAO.GetRoleByIdAsync(roleId);
        }

        // UserDetail operations
        public async Task<UserDetail> CreateUserDetailAsync(UserDetail userDetail)
        {
            return await _userDAO.CreateUserDetailAsync(userDetail);
        }

        public async Task<UserDetail?> GetUserDetailByUserIdAsync(int userId)
        {
            return await _userDAO.GetUserDetailByUserIdAsync(userId);
        }

        public async Task<UserDetail> UpdateUserDetailAsync(UserDetail userDetail)
        {
            return await _userDAO.UpdateUserDetailAsync(userDetail);
        }

        // UserPostCredit operations
        public async Task<UserPostCredit> CreateUserPostCreditAsync(UserPostCredit userPostCredit)
        {
            return await _userDAO.CreateUserPostCreditAsync(userPostCredit);
        }

        public async Task<UserPostCredit?> GetUserPostCreditByUserIdAsync(int userId)
        {
            return await _userDAO.GetUserPostCreditByUserIdAsync(userId);
        }

        public async Task<UserPostCredit> UpdateUserPostCreditAsync(UserPostCredit userPostCredit)
        {
            return await _userDAO.UpdateUserPostCreditAsync(userPostCredit);
        }

        // General operations
        public async Task SaveChangesAsync()
        {
            await _userDAO.SaveChangesAsync();
        }
    }
}
