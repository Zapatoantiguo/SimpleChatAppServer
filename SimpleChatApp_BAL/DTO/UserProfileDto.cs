using SimpleChatApp_DAL.Models;

namespace SimpleChatApp_BAL.DTO
{
    public class UserProfileDto
    {
        public string? Bio { get; set; }
        public ChatInventionOptions InventionOptions { get; set; }
        public required string Nickname { get; set; }
    }
}
