using Microsoft.AspNetCore.Identity;

namespace SimpleChatApp_DAL.Models
{
    public class User : IdentityUser
    {
        public bool IsAnonimous { get; set; }
        public UserProfile? Profile { get; set; }
        public List<User> FriendsObjects { get; } = [];
        public List<User> FriendsSubjects { get; } = [];
        public List<ChatRoom> ChatRooms { get; } = [];
    }
}
