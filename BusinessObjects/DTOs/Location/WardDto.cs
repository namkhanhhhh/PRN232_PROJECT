namespace BusinessObjects.DTOs.Location
{
    public class WardDto
    {
        public int Code { get; set; }
        public string Name { get; set; } = string.Empty;
        public string DivisionType { get; set; } = string.Empty;
        public string Codename { get { return Name; } set { } } // Codename is often the same as Name for wards in this API
        public int DistrictCode { get; set; }
    }
}
