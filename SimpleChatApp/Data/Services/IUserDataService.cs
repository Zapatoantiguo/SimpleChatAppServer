using SimpleChatApp.Models;
using SimpleChatApp.Models.DTO;

namespace SimpleChatApp.Data.Services
{
    public interface IUserDataService
    {
        public Task<List<UserDto>> GetUsersViaFilterAsync(UserSearchDto pattern);
        public Task<List<UserDto>> GetAllUsersAsync();
        public Task<User?> GetUserByNameAsync(string userName);
        public Task<UserProfileDto> CreateUserProfileAsync(User user);
        public Task<UserProfileDto?> UpdateUserProfileAsync(User user, UserProfileDto profile);
        public Task<UserProfileDto?> GetUserProfileAsync(string userId);
        public Task<FriendDto?> AddFriendAsync(User user, FriendDto friend);
        public Task<FriendDto?> RemoveFriendAsync(User user, FriendDto friend);
        public Task<List<FriendDto>> GetAllFriendsAsync(User user);
        public Task<bool> IsUserExist(string userName);

    }
}
