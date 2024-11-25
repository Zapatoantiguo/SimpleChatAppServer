namespace SimpleChatApp.Models.DTO.Auth
{
    public class RegisterDto
    {
        public required string Username { get; init; }
        public required string Email { get; init; }
        public required string Password { get; init; }
    }
}
