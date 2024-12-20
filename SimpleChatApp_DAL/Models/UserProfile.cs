namespace SimpleChatApp_DAL.Models
{
    public class UserProfile
    {
        public int UserProfileId { get; set; }
        public required string UserId { get; set; }
        public string? Bio {  get; set; }
        public ChatInventionOptions InventionOptions { get; set; }
        public required string Nickname { get; set; }
    }

    /// <summary>
    /// An enumeration specifying  which groups of users can invite this user to the chat room.
    /// </summary>
    public enum ChatInventionOptions
    {
        All = 0,
        ResidentsOnly = 1,
        FriendsOnly = 2
    }
}
