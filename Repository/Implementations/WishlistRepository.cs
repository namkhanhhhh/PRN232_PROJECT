using BusinessObjects.Models;
using DataAccess.DAO;
using Repository.Interfaces;

namespace Repository.Implementations
{
    public class WishlistRepository : IWishlistRepository
    {
        private readonly WishlistDAO _wishlistDAO;

        public WishlistRepository(WishlistDAO wishlistDAO)
        {
            _wishlistDAO = wishlistDAO;
        }

        public async Task<List<Wishlist>> GetWishlistByUserAsync(int userId)
        {
            return await _wishlistDAO.GetWishlistByUserAsync(userId);
        }

        public async Task<List<int>> GetWishlistJobIdsByUserAsync(int userId)
        {
            return await _wishlistDAO.GetWishlistJobIdsByUserAsync(userId);
        }

        public async Task<bool> IsJobInWishlistAsync(int userId, int jobPostId)
        {
            return await _wishlistDAO.IsJobInWishlistAsync(userId, jobPostId);
        }

        public async Task<Wishlist> AddToWishlistAsync(Wishlist wishlist)
        {
            return await _wishlistDAO.AddToWishlistAsync(wishlist);
        }

        public async Task<bool> RemoveFromWishlistAsync(int userId, int jobPostId)
        {
            return await _wishlistDAO.RemoveFromWishlistAsync(userId, jobPostId);
        }

        public async Task<Wishlist?> GetWishlistItemAsync(int userId, int jobPostId)
        {
            return await _wishlistDAO.GetWishlistItemAsync(userId, jobPostId);
        }
    }
}
