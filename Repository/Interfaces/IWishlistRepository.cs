using BusinessObjects.Models;

namespace Repository.Interfaces
{
    public interface IWishlistRepository
    {
        Task<List<Wishlist>> GetWishlistByUserAsync(int userId);
        Task<List<int>> GetWishlistJobIdsByUserAsync(int userId);
        Task<bool> IsJobInWishlistAsync(int userId, int jobPostId);
        Task<Wishlist> AddToWishlistAsync(Wishlist wishlist);
        Task<bool> RemoveFromWishlistAsync(int userId, int jobPostId);
        Task<Wishlist?> GetWishlistItemAsync(int userId, int jobPostId);
    }
}
