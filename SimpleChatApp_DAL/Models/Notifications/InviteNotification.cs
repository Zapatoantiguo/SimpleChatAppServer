namespace SimpleChatApp_DAL.Models.Notifications
{
    public class InviteNotification
    {
        public long Id { get; set; }
        public User TargetUser { get; set; } = null!;
        public string TargetId { get; set; } = null!;
        public string SourceUserName { get; set; } = null!;
        public string ChatRoomName { get; set; } = null!;
    }
}
