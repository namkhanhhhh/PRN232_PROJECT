using BusinessObjects.DTOs.CustomerService;
using BusinessObjects.Models;

namespace Repository.Interfaces
{
    public interface ICustomerServiceRepository
    {
        Task<CustomerServiceIndexDto> GetCustomerServiceIndexAsync(int userId);
        Task<BuyViewDto> GetBuyViewDataAsync(int userId, int id, string type);
        Task<bool> ProcessPurchaseAsync(int userId, PurchaseRequestDto request);
        Task<UserPackagesDto> GetUserPackagesAsync(int userId);
        Task<bool> SyncPurchaseHistoryAsync(int userId);
    }
}