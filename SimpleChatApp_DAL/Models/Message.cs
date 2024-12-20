namespace SimpleChatApp_DAL.Models
{
    public class Message
    {
        public long MessageId { get; set; }
        public int ChatRoomId { get; set; }
        public string? UserId { get; set; }
        public required string AuthorAlias {  get; set; }
        public required string Content { get; set; }
        public DateTime SentAt { get; set; }
        public User? Author { get; set; }
    }
}
