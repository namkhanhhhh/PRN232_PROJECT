using BusinessObjects.DTOs.Location;
using DataAccess.DAO;
using Repository.Interfaces;

namespace Repository.Implementations
{
    public class LocationRepository : ILocationRepository
    {
        private readonly LocationDAO _locationDAO;

        public LocationRepository(LocationDAO locationDAO)
        {
            _locationDAO = locationDAO;
        }

        public async Task<List<ProvinceDto>> GetProvincesAsync()
        {
            return await _locationDAO.GetProvincesAsync();
        }

        public async Task<List<DistrictDto>> GetDistrictsByProvinceCodeAsync(int provinceCode)
        {
            return await _locationDAO.GetDistrictsByProvinceCodeAsync(provinceCode);
        }

        public async Task<List<WardDto>> GetWardsByDistrictCodeAsync(int districtCode)
        {
            return await _locationDAO.GetWardsByDistrictCodeAsync(districtCode);
        }
    }
}
