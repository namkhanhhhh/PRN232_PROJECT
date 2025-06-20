using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.DAO
{
    public class EmployerDAO
    {
        private readonly SjobPContext _context;

        public EmployerDAO(SjobPContext context)
        {
            _context = context;
        }

        // Get employer with details
        public async Task<User?> GetEmployerWithDetailsAsync(int userId)
        {
            return await _context.Users
                .Include(u => u.UserDetails)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        // Get job posts count
        public async Task<int> GetJobPostsCountAsync(int userId)
        {
            return await _context.JobPosts
                .Where(jp => jp.UserId == userId)
                .CountAsync();
        }

        // Get applications count for employer's job posts
        public async Task<int> GetApplicationsCountAsync(int userId)
        {
            return await _context.Applications
                .Where(a => a.JobPost.UserId == userId)
                .CountAsync();
        }

        // Get total views for employer's job posts
        public async Task<int> GetTotalViewsAsync(int userId)
        {
            return await _context.JobPosts
                .Where(jp => jp.UserId == userId)
                .SumAsync(jp => jp.ViewCount ?? 0);
        }

        // Get user post credits
        public async Task<UserPostCredit?> GetUserPostCreditsAsync(int userId)
        {
            return await _context.UserPostCredits
                .FirstOrDefaultAsync(upc => upc.UserId == userId);
        }

        // Get user balance
        public async Task<UserCredit?> GetUserCreditAsync(int userId)
        {
            try
            {
                using var context = new SjobPContext();
                var userCredit = await context.UserCredits
                    .FirstOrDefaultAsync(uc => uc.UserId == userId);

                // If no record exists, create one with 0 balance
                if (userCredit == null)
                {
                    userCredit = new UserCredit
                    {
                        UserId = userId,
                        Balance = 0,
                        LastUpdated = DateTime.Now
                    };
                    context.UserCredits.Add(userCredit);
                    await context.SaveChangesAsync();
                }

                return userCredit;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user credit for user {userId}: {ex.Message}");
                return null;
            }
        }

        public async Task<decimal> GetUserBalanceAsync(int userId)
        {
            try
            {
                var userCredit = await GetUserCreditAsync(userId);
                return userCredit?.Balance ?? 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting balance for user {userId}: {ex.Message}");
                return 0;
            }
        }

        // Update user information
        public async Task<bool> UpdateUserAsync(User user)
        {
            try
            {
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating user: {ex.Message}");
                return false;
            }
        }

        // Update or create user detail
        public async Task<bool> UpdateUserDetailAsync(UserDetail userDetail)
        {
            try
            {
                var existing = await _context.UserDetails
                    .FirstOrDefaultAsync(ud => ud.UserId == userDetail.UserId);

                if (existing == null)
                {
                    _context.UserDetails.Add(userDetail);
                }
                else
                {
                    existing.FirstName = userDetail.FirstName;
                    existing.LastName = userDetail.LastName;
                    existing.PhoneNumber = userDetail.PhoneNumber;
                    existing.Address = userDetail.Address;
                    _context.UserDetails.Update(existing);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating user detail: {ex.Message}");
                return false;
            }
        }

        // Get job post history
        public async Task<List<JobPost>> GetJobPostHistoryAsync(int userId, int take = 10)
        {
            return await _context.JobPosts
                .Include(jp => jp.Applications)
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .Take(take)
                .ToListAsync();
        }

        // Get service order history
        public async Task<List<ServiceOrder>> GetServiceOrderHistoryAsync(int userId, int take = 10)
        {
            return await _context.ServiceOrders
                .Include(s => s.Service)
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.CreatedAt)
                .Take(take)
                .ToListAsync();
        }

        // Update user avatar
        public async Task<bool> UpdateUserAvatarAsync(int userId, string avatarPath)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null) return false;

                user.Avatar = avatarPath;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating avatar: {ex.Message}");
                return false;
            }
        }

        // Update user password
        public async Task<bool> UpdateUserPasswordAsync(int userId, string hashedPassword)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null) return false;

                user.Password = hashedPassword;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating password: {ex.Message}");
                return false;
            }
        }

        // Get user by id for password verification
        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _context.Users.FindAsync(userId);
        }

        // Get user detail by user id
        public async Task<UserDetail?> GetUserDetailByUserIdAsync(int userId)
        {
            return await _context.UserDetails
                .FirstOrDefaultAsync(ud => ud.UserId == userId);
        }

        // Get credit transaction history
        public async Task<List<CreditTransaction>> GetCreditTransactionHistoryAsync(int userId, int take = 10)
        {
            return await _context.CreditTransactions
                .Where(ct => ct.UserId == userId)
                .OrderByDescending(ct => ct.CreatedAt)
                .Take(take)
                .ToListAsync();
        }
    }
}
