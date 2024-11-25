namespace SimpleChatApp.Models.DTO
{
    public class MessageDto
    {
        public required string AuthorAlias { get; set; }
        public required string Content { get; set; }
        public DateTime SentAt { get; set; }
    }
}
