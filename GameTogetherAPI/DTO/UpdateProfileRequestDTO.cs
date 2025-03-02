namespace GameTogetherAPI.DTO
{
    public class UpdateProfileRequestDTO
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public string? ProfilePicture { get; set; }
        public string? Description { get; set; }
        public string? Region { get; set; }
        public List<string> Tags { get; set; } = new();
    }

}
