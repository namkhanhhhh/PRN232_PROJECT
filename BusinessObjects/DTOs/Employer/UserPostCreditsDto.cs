namespace BusinessObjects.DTOs.Employer
{
    public class UserPostCreditsDto
    {
        public int SilverPostsAvailable { get; set; }
        public int GoldPostsAvailable { get; set; }
        public int DiamondPostsAvailable { get; set; }
        public int AuthenLogoAvailable { get; set; }
        public int PushToTopAvailable { get; set; }
        public DateTime? LastUpdated { get; set; }
    }
}
