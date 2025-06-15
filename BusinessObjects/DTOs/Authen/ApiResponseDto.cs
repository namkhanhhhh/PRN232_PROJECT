namespace BusinessObjects.DTOs
{
    public class ApiResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public Dictionary<string, string>? Errors { get; set; }
    }

    public class ApiResponseDto<T> : ApiResponseDto
    {
        public T? Data { get; set; }
    }
}
