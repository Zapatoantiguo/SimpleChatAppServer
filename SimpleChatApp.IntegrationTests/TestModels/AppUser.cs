using SimpleChatApp_BAL.DTO;
using SimpleChatApp_BAL.DTO.Auth;
using SimpleChatApp_DAL.Models;

namespace SimpleChatApp.IntegrationTests.TestModels
{
    public class AppUser
    {
        public string AccessToken { get; set; } = null!;
        public string UserName { get; set; } = null!;
    }
    public class AnonUser : AppUser
    {
        public GuestLoginDto GetLoginData()
        {
            return new GuestLoginDto { Username = UserName };
        }
    }
    public class RegisteredUser : AppUser
    {
        public string Password { get; set; } = null!;
        public string Email { get; set; } = null!;
        public ChatInventionOptions ChatInventionOptions { get; set; }
        public RegisterDto GetRegisterData()
        {
            return new RegisterDto
            {
                Username = UserName,
                Password = Password,
                Email = Email
            };
        }
        public LoginDto GetLoginData()
        {
            return new LoginDto
            {
                Username = UserName,
                Password = Password
            };
        }
        public UserProfileDto GetProfileData()
        {
            return new UserProfileDto
            {
                Nickname = $"{UserName}_Nickname",
                Bio = $"{UserName} bio",
                InventionOptions = ChatInventionOptions
            };
        }
        
    }

}
