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
        public Task<UserProfileDto?> UpdateUserProfileAsync(string userId, UserProfileDto profile);
        public Task<UserProfileDto?> GetUserProfileAsync(string userId);
        public Task<FriendDto?> AddFriendAsync(string userId, FriendDto friend);
        public Task<FriendDto?> RemoveFriendAsync(string userId, FriendDto friend);
        public Task<List<FriendDto>> GetAllFriendsAsync(string userId);
        public Task<bool> CheckIsFriend(string friendSubjId, string friendObjId);
        public Task<bool> IsUserExist(string userName);

    }
}
