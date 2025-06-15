using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.DAO
{
    public class WishlistDAO
    {
        private readonly SjobPContext _context;

        public WishlistDAO(SjobPContext context)
        {
            _context = context;
        }

        // Get wishlist by user
        public async Task<List<Wishlist>> GetWishlistByUserAsync(int userId)
        {
            return await _context.Wishlists
                .Include(w => w.JobPost)
                .ThenInclude(p => p.User)
                .ThenInclude(u => u.UserDetails)
                .Include(w => w.JobPost.JobPostCategories)
                .ThenInclude(pc => pc.Category)
                .Where(w => w.UserId == userId)
                .OrderByDescending(w => w.CreatedAt)
                .ToListAsync();
        }

        // Get wishlist job IDs by user
        public async Task<List<int>> GetWishlistJobIdsByUserAsync(int userId)
        {
            return await _context.Wishlists
                .Where(w => w.UserId == userId)
                .Select(w => w.JobPostId)
                .ToListAsync();
        }

        // Check if job is in wishlist
        public async Task<bool> IsJobInWishlistAsync(int userId, int jobPostId)
        {
            return await _context.Wishlists
                .AnyAsync(w => w.UserId == userId && w.JobPostId == jobPostId);
        }

        // Add to wishlist
        public async Task<Wishlist> AddToWishlistAsync(Wishlist wishlist)
        {
            _context.Wishlists.Add(wishlist);
            await _context.SaveChangesAsync();
            return wishlist;
        }

        // Remove from wishlist
        public async Task<bool> RemoveFromWishlistAsync(int userId, int jobPostId)
        {
            var wishlistItem = await _context.Wishlists
                .FirstOrDefaultAsync(w => w.UserId == userId && w.JobPostId == jobPostId);

            if (wishlistItem == null) return false;

            _context.Wishlists.Remove(wishlistItem);
            await _context.SaveChangesAsync();
            return true;
        }

        // Get wishlist item
        public async Task<Wishlist?> GetWishlistItemAsync(int userId, int jobPostId)
        {
            return await _context.Wishlists
                .Include(w => w.JobPost)
                .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(w => w.UserId == userId && w.JobPostId == jobPostId);
        }
    }
}
