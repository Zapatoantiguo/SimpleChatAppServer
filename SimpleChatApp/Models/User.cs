using Microsoft.AspNetCore.Identity;

namespace SimpleChatApp.Models
{
    public class User : IdentityUser
    {
        public bool IsAnonimous { get; set; }
        public UserProfile? Profile { get; set; }
        public List<User> Friends { get; } = [];
        public List<ChatRoom> ChatRooms { get; } = [];
    }
}
