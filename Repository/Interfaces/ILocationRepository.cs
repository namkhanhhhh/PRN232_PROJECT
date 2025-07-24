using BusinessObjects.DTOs.Location;

namespace Repository.Interfaces
{
    public interface ILocationRepository
    {
        Task<List<ProvinceDto>> GetProvincesAsync();
        Task<List<DistrictDto>> GetDistrictsByProvinceCodeAsync(int provinceCode);
        Task<List<WardDto>> GetWardsByDistrictCodeAsync(int districtCode);
    }
}
