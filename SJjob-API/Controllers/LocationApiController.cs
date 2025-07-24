using BusinessObjects.DTOs.Location;
using BusinessObjects.DTOs.Authen; // Assuming ApiResponseDto is in Authen
using Microsoft.AspNetCore.Mvc;
using Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using BusinessObjects.DTOs;

namespace Sjob_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous] // Allow anonymous access for location data
    public class LocationApiController : ControllerBase
    {
        private readonly ILocationRepository _locationRepository;

        public LocationApiController(ILocationRepository locationRepository)
        {
            _locationRepository = locationRepository;
        }

        [HttpGet("provinces")]
        public async Task<ActionResult<ApiResponseDto<List<ProvinceDto>>>> GetProvinces()
        {
            try
            {
                var provinces = await _locationRepository.GetProvincesAsync();
                return Ok(new ApiResponseDto<List<ProvinceDto>>
                {
                    Success = true,
                    Data = provinces,
                    Message = "Provinces retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDto<List<ProvinceDto>>
                {
                    Success = false,
                    Message = $"Error retrieving provinces: {ex.Message}"
                });
            }
        }

        [HttpGet("districts/{provinceCode}")]
        public async Task<ActionResult<ApiResponseDto<List<DistrictDto>>>> GetDistricts(int provinceCode)
        {
            try
            {
                var districts = await _locationRepository.GetDistrictsByProvinceCodeAsync(provinceCode);
                return Ok(new ApiResponseDto<List<DistrictDto>>
                {
                    Success = true,
                    Data = districts,
                    Message = "Districts retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDto<List<DistrictDto>>
                {
                    Success = false,
                    Message = $"Error retrieving districts: {ex.Message}"
                });
            }
        }

        [HttpGet("wards/{districtCode}")]
        public async Task<ActionResult<ApiResponseDto<List<WardDto>>>> GetWards(int districtCode)
        {
            try
            {
                var wards = await _locationRepository.GetWardsByDistrictCodeAsync(districtCode);
                return Ok(new ApiResponseDto<List<WardDto>>
                {
                    Success = true,
                    Data = wards,
                    Message = "Wards retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDto<List<WardDto>>
                {
                    Success = false,
                    Message = $"Error retrieving wards: {ex.Message}"
                });
            }
        }
    }
}
