namespace SimpleChatApp.Models.DTO
{
    public class UserProfileDto
    {
        public string? Bio { get; set; }
        public ChatInventionOptions InventionOptions { get; set; }
        public required string Nickname { get; set; }
    }
}
