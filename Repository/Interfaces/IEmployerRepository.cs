using BusinessObjects.DTOs.Employer;
using BusinessObjects.Models;

namespace Repository.Interfaces
{
    public interface IEmployerRepository
    {
        Task<EmployerProfileDto?> GetEmployerProfileAsync(int userId);
        Task<bool> UpdateEmployerProfileAsync(EditEmployerProfileDto dto);
        Task<EmployerHistoryDto> GetEmployerHistoryAsync(int userId);
        Task<bool> UpdateEmployerAvatarAsync(int userId, string avatarPath);
        Task<decimal> GetEmployerBalanceAsync(int userId);
    }
}
