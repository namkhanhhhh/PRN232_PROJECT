using BusinessObjects.DTOs.Location;
using Newtonsoft.Json;

namespace DataAccess.DAO
{
    public class LocationDAO
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://provinces.open-api.vn/api/";

        public LocationDAO(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<ProvinceDto>> GetProvincesAsync()
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}p/");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<ProvinceDto>>(content) ?? new List<ProvinceDto>();
        }

        public async Task<List<DistrictDto>> GetDistrictsByProvinceCodeAsync(int provinceCode)
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}p/{provinceCode}?depth=2");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var provinceDetail = JsonConvert.DeserializeObject<ProvinceDetailResponseDto>(content);
            return provinceDetail?.Districts ?? new List<DistrictDto>();
        }

        public async Task<List<WardDto>> GetWardsByDistrictCodeAsync(int districtCode)
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}d/{districtCode}?depth=2");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var districtDetail = JsonConvert.DeserializeObject<DistrictDetailResponseDto>(content);
            return districtDetail?.Wards ?? new List<WardDto>();
        }

        // Helper DTOs to deserialize nested responses from the API
        private class ProvinceDetailResponseDto : ProvinceDto
        {
            public List<DistrictDto> Districts { get; set; } = new List<DistrictDto>();
        }

        private class DistrictDetailResponseDto : DistrictDto
        {
            public List<WardDto> Wards { get; set; } = new List<WardDto>();
        }
    }
}
