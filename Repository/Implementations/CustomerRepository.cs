using BusinessObjects.DTOs;
using BusinessObjects.DTOs.Customer;
using BusinessObjects.Models;
using DataAccess.DAO;
using Repository.Interfaces;

namespace Repository.Implementations
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly CustomerDAO _customerDAO;

        public CustomerRepository(CustomerDAO customerDAO)
        {
            _customerDAO = customerDAO;
        }

        public async Task<List<RoleDto>> GetAllRolesAsync()
        {
            return await _customerDAO.GetAllRolesAsync();
        }

        public async Task<bool> AssignRoleAsync(int userId, int roleId)
        {
            return await _customerDAO.AssignRoleAsync(userId, roleId);
        }

        public async Task<bool> UserHasRoleAsync(int userId)
        {
            return await _customerDAO.UserHasRoleAsync(userId);
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _customerDAO.GetUserByIdAsync(userId);
        }
    }
}
