using BusinessObjects.DTOs;
using BusinessObjects.DTOs.Worker;

namespace BusinessObjects.ViewModels
{
    public class EmployerProfileViewModel
    {
        public UserDto User { get; set; }
        public UserDetailDto UserDetail { get; set; }
        public List<JobPostDto> JobPosts { get; set; }

        public EmployerProfileViewModel()
        {
            User = new UserDto();
            UserDetail = new UserDetailDto();
            JobPosts = new List<JobPostDto>();
        }
    }
}
