namespace SimpleChatApp_BAL.DTO.Auth
{
    public class RegisterDto
    {
        public required string Username { get; init; }
        public string? Email { get; init; }
        public required string Password { get; init; }
    }
}
