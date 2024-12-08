using Microsoft.EntityFrameworkCore;
using SimpleChatApp.Models;
using SimpleChatApp.Models.DTO;

namespace SimpleChatApp.Data.Services
{
    public class UserDataService : IUserDataService
    {
        AppDbContext _context;
        public UserDataService(AppDbContext context)
        {
            _context = context;
        }
        public async Task<FriendDto?> AddFriendAsync(string userId, FriendDto friend)
        {


            var friendId = await _context.Users
                           .Where(u => u.UserName == friend.UserName)
                           .Select(u => u.Id)
                           .SingleOrDefaultAsync();

            if (friendId == null)
                return null;

            if (userId == friendId)
                return null;

            var existingFriendship = await _context.Friendships
                .Where(fs => fs.SubjectId == userId && fs.ObjectId == friendId)
                .SingleOrDefaultAsync();

            if (existingFriendship != null)
                return null;

            _context.Friendships.Add(new Friendship
            {
                SubjectId = userId,
                ObjectId = friendId
            });

            await _context.SaveChangesAsync();
            return friend;
        }

        public async Task<UserProfileDto> CreateUserProfileAsync(User user)
        {
            var profile = new UserProfile
            {
                UserId = user.Id,
                Nickname = user.UserName!,
                Bio = "",
                InventionOptions = ChatInventionOptions.All
            };

            _context.Profiles.Add(profile);
            await _context.SaveChangesAsync();
            var result = new UserProfileDto
            {
                Nickname = profile.Nickname,
                Bio = profile.Bio,
                InventionOptions = profile.InventionOptions
            };
            return result;
        }

        public async Task<List<FriendDto>> GetAllFriendsAsync(string userId)
        {
            var friends = (await _context.Users
                          .Where(u => u.Id == userId)
                          .Include(u => u.FriendsObjects)
                          .Select(u => u.FriendsObjects.Select(fr => new FriendDto { UserName = fr.UserName! }))
                          .SingleOrDefaultAsync())?.ToList();

            return friends ?? new List<FriendDto>();
        }

        public async Task<User?> GetUserByNameAsync(string userName)
        {
            User? result = await _context.Users.SingleOrDefaultAsync(u => u.UserName == userName);
            return result;
        }

        public async Task<UserProfileDto?> GetUserProfileAsync(string userId)
        {
            var userIsAnon = await _context.Users
                .AnyAsync(u => u.Id == userId && u.IsAnonimous);
            if (userIsAnon) return null;

            var profile = await _context.Profiles.SingleOrDefaultAsync(p => p.UserId == userId);

            if (profile == null) return null;

            var result = new UserProfileDto
            {
                Nickname = profile.Nickname,
                Bio = profile.Bio,
                InventionOptions = profile.InventionOptions
            };

            return result;
        }

        public async Task<List<UserDto>> GetUsersViaFilterAsync(UserSearchDto filter)
        {
            List<UserDto> result = await _context.Users
                                   .Where(!string.IsNullOrEmpty(filter.NamePattern) ?
                                      u => u.UserName!.StartsWith(filter.NamePattern) : u => true)
                                   .Select(u => new UserDto { Name = u.UserName!, IsAnonimous = u.IsAnonimous })
                                   .ToListAsync();

            return result;
        }

        public async Task<List<UserDto>> GetAllUsersAsync()
        {
            // TODO: add pagination
            List<UserDto> result = await _context.Users
                                   .Select(u => new UserDto { Name = u.UserName!, IsAnonimous = u.IsAnonimous })
                                   .ToListAsync();
            return result;
        }

        public async Task<bool> IsUserExist(string userName)
        {
            return await _context.Users.AnyAsync(u => u.UserName == userName);
        }

        public async Task<FriendDto?> RemoveFriendAsync(string userId, FriendDto friend)
        {
            var friendId = await _context.Users
                .Where(u => u.UserName == friend.UserName)
                .Select(u => u.Id)
                .SingleOrDefaultAsync();

            if (friendId == null)
                return null;

            if (userId == friendId)
                return null;

            var friendship = await _context.Friendships
                .Where(fs => fs.SubjectId == userId && fs.ObjectId == friendId)
                .SingleOrDefaultAsync();

            if (friendship == null)
                return null;

            _context.Friendships.Remove(friendship);

            await _context.SaveChangesAsync();
            return friend;
        }

        public async Task<UserProfileDto?> UpdateUserProfileAsync(string userId, UserProfileDto profile)
        {
            var userIsAnon = await _context.Users
                .AnyAsync(u => u.Id == userId && u.IsAnonimous);

            if (userIsAnon) return null;

            var nickExists = _context.Profiles
                .Where(p => p.Nickname == profile.Nickname && p.UserId != userId)
                .Any();

            if (nickExists)
                return null;

            var newProfile = _context.Profiles.Single(up => up.UserId == userId);
            newProfile.Nickname = profile.Nickname;
            newProfile.Bio = profile.Bio;
            newProfile.InventionOptions = profile.InventionOptions;
            await _context.SaveChangesAsync();

            return profile;
        }

        public async Task<bool> CheckIsFriend(string friendSubjId, string friendObjId)
        {
            return await _context.Friendships
                .AnyAsync(fs => fs.SubjectId == friendSubjId && fs.ObjectId == friendObjId);
        }
    }
}
