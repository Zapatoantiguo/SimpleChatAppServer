namespace SimpleChatApp.Models.Requests.Auth
{
    public class RefreshRequest
    {
        public required string RefreshToken { get; init; }
    }
}
