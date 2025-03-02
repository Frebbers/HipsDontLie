namespace GameTogetherAPI.DTO
{
    public class CreateSessionRequestDTO
    {
        public string Title { get; set; }
        public bool IsVisible { get; set; }
        public string AgeRange { get; set; }
        public string Description { get; set; }
        public List<string> Tags { get; set; } = new();
    }
}
