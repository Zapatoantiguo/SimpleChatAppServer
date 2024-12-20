namespace SimpleChatApp_DAL.Models
{
    public class UserChatRoom
    {
        public required string UserId { get; set; }
        public required int ChatRoomId { get; set; }
        public DateTime JoinedAt { get; set; }
    }
}
