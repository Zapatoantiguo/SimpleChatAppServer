namespace SimpleChatApp.Models
{
    public class Message
    {
        public long MessageId { get; set; }
        public int ChatRoomId { get; set; }
        public string? UserId { get; set; }
        public required string AuthorName {  get; set; }
        public required string Content { get; set; }
        public DateTime SentAt { get; set; }
    }
}
