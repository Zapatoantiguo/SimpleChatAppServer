namespace SimpleChatApp.Models.Requests.Auth
{
    public class LoginRequestModel
    {
        public required string Username { get; init; }
        public required string Password { get; init; }
    }
}
