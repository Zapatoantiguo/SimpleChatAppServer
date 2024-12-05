namespace SimpleChatApp.Models
{
    public class ChatRoom
    {
        public int ChatRoomId {  get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public List<User> Users { get; set; } = [];
        public List<Message> Messages { get; } = [];
        public List<UserChatRoom> UserChatRoom { get; set; } = [];
    }
}
