namespace BusinessObjects.DTOs.Location
{
    public class ProvinceDto
    {
        public int Code { get; set; }
        public string Name { get; set; } = string.Empty;
        public string DivisionType { get; set; } = string.Empty;
        public string Codename { get; set; } = string.Empty;
        public int PhoneCode { get; set; }
    }
}
