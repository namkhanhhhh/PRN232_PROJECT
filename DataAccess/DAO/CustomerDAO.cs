using BusinessObjects.DTOs.Customer;
using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.DAO
{
    public class CustomerDAO
    {
        private readonly SjobPContext _context;

        public CustomerDAO(SjobPContext context)
        {
            _context = context;
        }

        public async Task<List<RoleDto>> GetAllRolesAsync()
        {
            return await _context.Roles
                .Where(r => r.Name != "Admin") // Exclude admin role from selection
                .Select(r => new RoleDto
                {
                    Id = r.Id,
                    Name = r.Name
                })
                .ToListAsync();
        }

        public async Task<bool> AssignRoleAsync(int userId, int roleId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            user.RoleId = roleId;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UserHasRoleAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            return user != null && user.RoleId != null;
        }

        public async Task<string?> GetUserRoleNameAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

            return user?.Role?.Name;
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _context.Users
                .Include(u => u.Role)
                .Include(u => u.UserDetails)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }
    }
}
