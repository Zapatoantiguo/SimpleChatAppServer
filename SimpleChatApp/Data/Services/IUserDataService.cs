using SimpleChatApp.ErrorHandling.ResultPattern;
using SimpleChatApp.Models;
using SimpleChatApp.Models.DTO;

namespace SimpleChatApp.Data.Services
{
    public interface IUserDataService
    {
        public Task<List<UserDto>> GetUsersViaFilterAsync(UserSearchDto pattern);
        public Task<List<UserDto>> GetAllUsersAsync();
        public Task<User?> GetUserByNameAsync(string userName);
        public Task<Result<UserProfileDto>> CreateUserProfileAsync(string userId);
        public Task<Result<UserProfileDto>> UpdateUserProfileAsync(string userId, UserProfileDto profile);
        public Task<Result<UserProfileDto>> GetUserProfileAsync(string userId);
        public Task<Result<FriendDto>> AddFriendAsync(string userId, FriendDto friend);
        public Task<Result<FriendDto>> RemoveFriendAsync(string userId, FriendDto friend);
        public Task<List<FriendDto>> GetAllFriendsAsync(string userId);
        public Task<bool> CheckIsFriend(string friendSubjId, string friendObjId);
        public Task<bool> IsUserExist(string userName);

    }
}
