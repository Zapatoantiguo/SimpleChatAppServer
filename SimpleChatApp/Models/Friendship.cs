namespace SimpleChatApp.Models
{
    public class Friendship
    {
        public required string SubjectId { get; set; }
        public required string ObjectId { get; set; }
        //public User Subject { get; set; } = new();
        //public User Object { get; set; } = new();
    }
}
