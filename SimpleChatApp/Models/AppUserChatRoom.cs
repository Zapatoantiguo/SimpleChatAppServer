namespace SimpleChatApp.Models
{
    public class AppUserChatRoom
    {
        public required string AppUserId { get; set; }
        public required int ChatRoomId { get; set; }
        public DateTime JoinedAt { get; set; }
    }
}
