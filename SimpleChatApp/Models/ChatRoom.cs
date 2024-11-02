namespace SimpleChatApp.Models
{
    public class ChatRoom
    {
        public int ChatRoomId {  get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<User> Users { get; } = [];
        public List<Message> Messages { get; } = [];

    }
}
