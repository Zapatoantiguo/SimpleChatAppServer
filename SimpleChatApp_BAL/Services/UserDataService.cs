using Microsoft.EntityFrameworkCore;
using SimpleChatApp_BAL.DTO;
using SimpleChatApp_BAL.ErrorHandling.ResultPattern;
using SimpleChatApp_DAL.Models;
using SimpleChatApp_DAL;

namespace SimpleChatApp_BAL.Services
{
    public class UserDataService : IUserDataService
    {
        AppDbContext _context;
        public UserDataService(AppDbContext context)
        {
            _context = context;
        }
        public async Task<Result<FriendDto>> AddFriendAsync(string userId, FriendDto friend)
        {
            User? user = await _context.Users.SingleOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                throw new Exception($"User ID {userId} doesn't exist in DB");

            var friendId = await _context.Users
                           .Where(u => u.UserName == friend.UserName)
                           .Select(u => u.Id)
                           .SingleOrDefaultAsync();

            if (friendId == null)
                return Result<FriendDto>.Failure(UserErrors.NotFound(friend.UserName));

            if (userId == friendId)
                return Result<FriendDto>.Failure(UserErrors.SelfFriendship());

            var existingFriendship = await _context.Friendships
                .Where(fs => fs.SubjectId == userId && fs.ObjectId == friendId)
                .SingleOrDefaultAsync();

            if (existingFriendship != null)
                return Result<FriendDto>.Failure(UserErrors.IsFriendAlready());

            _context.Friendships.Add(new Friendship
            {
                SubjectId = userId,
                ObjectId = friendId
            });

            await _context.SaveChangesAsync();
            return Result<FriendDto>.Success(friend);
        }

        public async Task<Result<UserProfileDto>> CreateUserProfileAsync(string userId)
        {
            User? user = await _context.Users.SingleOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                throw new Exception($"User ID {userId} doesn't exist in DB");

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
            return Result<UserProfileDto>.Success(result);
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

        public async Task<Result<UserProfileDto>> GetUserProfileAsync(string userId)
        {
            var user = await _context.Users
                .Include(u => u.Profile)
                .SingleOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                throw new Exception($"User ID {userId} doesn't exist in DB");

            if (user.IsAnonimous)
                return Result<UserProfileDto>.Failure(Error
                    .Validation("Users.NotAllowed",
                    "Operation is not allowed for an anonimous users"));    // TODO: move anon sign check to authorization component

            var result = new UserProfileDto
            {
                Nickname = user.Profile!.Nickname,
                Bio = user.Profile.Bio,
                InventionOptions = user.Profile.InventionOptions
            };

            return Result<UserProfileDto>.Success(result);
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

        public async Task<Result<FriendDto>> RemoveFriendAsync(string userId, FriendDto friend)
        {
            User? user = await _context.Users.SingleOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                throw new Exception($"User ID {userId} doesn't exist in DB");

            var friendId = await _context.Users
                .Where(u => u.UserName == friend.UserName)
                .Select(u => u.Id)
                .SingleOrDefaultAsync();

            if (friendId == null)
                return Result<FriendDto>.Failure(UserErrors.NotFound(friend.UserName));

            if (userId == friendId)
                return Result<FriendDto>.Failure(UserErrors.SelfFriendship());

            var friendship = await _context.Friendships
                .Where(fs => fs.SubjectId == userId && fs.ObjectId == friendId)
                .SingleOrDefaultAsync();

            if (friendship == null)
                return Result<FriendDto>.Failure(UserErrors.IsNotFriend());

            _context.Friendships.Remove(friendship);

            await _context.SaveChangesAsync();
            return Result<FriendDto>.Success(friend);
        }

        public async Task<Result<UserProfileDto>> UpdateUserProfileAsync(string userId, UserProfileDto profile)
        {
            var userIsAnon = await _context.Users
                .AnyAsync(u => u.Id == userId && u.IsAnonimous);

            if (userIsAnon) 
                return Result<UserProfileDto>.Failure(Error
                    .Validation("Users.NotAllowed",
                    "Operation is not allowed for an anonimous users"));    // TODO: move anon sign check to authorization component

            var nickExists = await _context.Profiles
                .Where(p => p.Nickname == profile.Nickname && p.UserId != userId)
                .AnyAsync();

            if (nickExists)
                return Result<UserProfileDto>.Failure(UserErrors.NickExistsAlready());

            var newProfile = await _context.Profiles.SingleAsync(up => up.UserId == userId);
            newProfile.Nickname = profile.Nickname;
            newProfile.Bio = profile.Bio;
            newProfile.InventionOptions = profile.InventionOptions;
            await _context.SaveChangesAsync();

            return Result<UserProfileDto>.Success(profile);
        }

        public async Task<bool> CheckIsFriend(string friendSubjId, string friendObjId)
        {
            return await _context.Friendships
                .AnyAsync(fs => fs.SubjectId == friendSubjId && fs.ObjectId == friendObjId);
        }
    }
}
