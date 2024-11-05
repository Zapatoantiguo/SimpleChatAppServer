namespace SimpleChatApp.Models.Requests.Auth
{
    public class RegisterRequest
    {
        public required string Username { get; init; }
        public required string Email { get; init; }
        public required string Password { get; init; }
    }
}
