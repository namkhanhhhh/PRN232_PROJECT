namespace BusinessObjects.DTOs.Location
{
    public class DistrictDto
    {
        public int Code { get; set; }
        public string Name { get; set; } = string.Empty;
        public string DivisionType { get; set; } = string.Empty;
        public string Codename { get; set; } = string.Empty;
        public int ProvinceCode { get; set; }
    }
}
