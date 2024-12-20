namespace SimpleChatApp_BAL.DTO
{
    public class MessageDto
    {
        public required string AuthorAlias { get; set; } = string.Empty;
        public required string Content { get; set; }
        public DateTime SentAt { get; set; }
    }
}
