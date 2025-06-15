using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.DAO
{
    public class UserDAO
    {
        private readonly SjobPContext _context;

        public UserDAO(SjobPContext context)
        {
            _context = context;
        }

        // User CRUD operations
        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users
                .Include(u => u.Role)
                .Include(u => u.UserDetails)
                .Include(u => u.UserCredits)
                .FirstOrDefaultAsync(u => u.Email.Equals(email));
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _context.Users
                .Include(u => u.Role)
                .Include(u => u.UserDetails)
                .Include(u => u.UserCredits)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _context.Users
                .Include(u => u.Role)
                .Include(u => u.UserDetails)
                .Include(u => u.UserCredits)
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            return await _context.Users.AnyAsync(u => u.Username == username);
        }

        public async Task<User> CreateUserAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User> UpdateUserAsync(User user)
        {
            try
            {
                Console.WriteLine($"Updating User: {user.Id}");
                _context.Users.Update(user);
                var result = await _context.SaveChangesAsync();
                Console.WriteLine($"SaveChanges result: {result} rows affected");
                return user;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating User: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        // Password operations
        public async Task<bool> UpdatePasswordAsync(int userId, string hashedPassword)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            user.Password = hashedPassword;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdatePasswordByEmailAsync(string email, string hashedPassword)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return false;

            user.Password = hashedPassword;
            await _context.SaveChangesAsync();
            return true;
        }

        // Account Management operations
        public async Task<List<User>> GetAllUsersAsync(int page, int pageSize, string searchTerm = "", string sortBy = "Id", string sortOrder = "asc")
        {
            var query = _context.Users
                .Include(u => u.Role)
                .Include(u => u.UserDetails)
                .Where(u => u.Role.Name != "Admin")
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(u =>
                    u.Username.Contains(searchTerm) ||
                    u.Email.Contains(searchTerm) ||
                    u.UserDetails.Any(ud =>
                        ud.FirstName.Contains(searchTerm) ||
                        ud.LastName.Contains(searchTerm)));
            }

            // Apply sorting
            query = sortBy.ToLower() switch
            {
                "username" => sortOrder == "desc" ? query.OrderByDescending(u => u.Username) : query.OrderBy(u => u.Username),
                "email" => sortOrder == "desc" ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email),
                "role" => sortOrder == "desc" ? query.OrderByDescending(u => u.Role.Name) : query.OrderBy(u => u.Role.Name),
                "status" => sortOrder == "desc" ? query.OrderByDescending(u => u.Status) : query.OrderBy(u => u.Status),
                _ => sortOrder == "desc" ? query.OrderByDescending(u => u.Id) : query.OrderBy(u => u.Id)
            };

            return await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetTotalUsersCountAsync(string searchTerm = "")
        {
            var query = _context.Users
                .Where(u => u.Role.Name != "Admin")
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(u =>
                    u.Username.Contains(searchTerm) ||
                    u.Email.Contains(searchTerm) ||
                    u.UserDetails.Any(ud =>
                        ud.FirstName.Contains(searchTerm) ||
                        ud.LastName.Contains(searchTerm)));
            }

            return await query.CountAsync();
        }

        public async Task<bool> ToggleUserStatusAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            user.Status = !user.Status;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateUserProfileAsync(int userId, string firstName, string lastName, string? phoneNumber, string? address)
        {
            var userDetail = await _context.UserDetails.FirstOrDefaultAsync(ud => ud.UserId == userId);
            if (userDetail == null) return false;

            userDetail.FirstName = firstName;
            userDetail.LastName = lastName;
            userDetail.PhoneNumber = phoneNumber;
            userDetail.Address = address;

            await _context.SaveChangesAsync();
            return true;
        }

        // Role operations
        public async Task<List<Role>> GetAllRolesAsync()
        {
            return await _context.Roles.Where(r => r.Name != "Admin").ToListAsync();
        }

        public async Task<Role?> GetRoleByNameAsync(string roleName)
        {
            return await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
        }

        public async Task<Role?> GetRoleByIdAsync(int roleId)
        {
            return await _context.Roles.FindAsync(roleId);
        }

        // UserDetail operations
        public async Task<UserDetail> CreateUserDetailAsync(UserDetail userDetail)
        {
            try
            {
                Console.WriteLine($"Creating UserDetail for UserId: {userDetail.UserId}");
                _context.UserDetails.Add(userDetail);
                var result = await _context.SaveChangesAsync();
                Console.WriteLine($"SaveChanges result: {result} rows affected");
                return userDetail;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating UserDetail: {ex.Message}");
                throw;
            }
        }

        public async Task<UserDetail?> GetUserDetailByUserIdAsync(int userId)
        {
            return await _context.UserDetails.FirstOrDefaultAsync(ud => ud.UserId == userId);
        }

        public async Task<UserDetail> UpdateUserDetailAsync(UserDetail userDetail)
        {
            try
            {
                Console.WriteLine($"Updating UserDetail for UserId: {userDetail.UserId}");
                _context.UserDetails.Update(userDetail);
                var result = await _context.SaveChangesAsync();
                Console.WriteLine($"SaveChanges result: {result} rows affected");
                return userDetail;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating UserDetail: {ex.Message}");
                throw;
            }
        }

        // UserPostCredit operations
        public async Task<UserPostCredit> CreateUserPostCreditAsync(UserPostCredit userPostCredit)
        {
            _context.UserPostCredits.Add(userPostCredit);
            await _context.SaveChangesAsync();
            return userPostCredit;
        }

        public async Task<UserPostCredit?> GetUserPostCreditByUserIdAsync(int userId)
        {
            return await _context.UserPostCredits.FirstOrDefaultAsync(upc => upc.UserId == userId);
        }

        public async Task<UserPostCredit> UpdateUserPostCreditAsync(UserPostCredit userPostCredit)
        {
            _context.UserPostCredits.Update(userPostCredit);
            await _context.SaveChangesAsync();
            return userPostCredit;
        }

        // General operations
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
