﻿using Microsoft.EntityFrameworkCore;
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
        public async Task<FriendDto?> AddFriendAsync(User user, FriendDto friend)
        {
            if (user.UserName == friend.UserName)
                return null;

            var friendId = await _context.Users
                           .Where(u => u.UserName == friend.UserName)
                           .Select(u => u.Id)
                           .SingleOrDefaultAsync();

            if (friendId == null)
                return null;

            var existingFriendship = await _context.Friendships
                .Where(fs => fs.SubjectId == user.Id && fs.ObjectId == friendId)
                .SingleOrDefaultAsync();

            if (existingFriendship != null)
                return null;

            _context.Friendships.Add(new Friendship
            {
                SubjectId = user.Id,
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

        public async Task<List<FriendDto>> GetAllFriendsAsync(User user)
        {
            var friends = (await _context.Users
                          .Where(u => u.UserName == user.UserName)
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
            var profile = await _context.Profiles.SingleOrDefaultAsync(p => p.UserId == userId);
            UserProfileDto? result = null;
            if (profile != null)
            {
                result = new UserProfileDto
                {
                    Nickname = profile.Nickname,
                    Bio = profile.Bio,
                    InventionOptions = profile.InventionOptions
                };
            }

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

        public async Task<FriendDto?> RemoveFriendAsync(User user, FriendDto friend)
        {
            if (user.UserName == friend.UserName)
                return null;

            var friendId = await _context.Users
                .Where(u => u.UserName == friend.UserName)
                .Select(u => u.Id)
                .SingleOrDefaultAsync();

            if (friendId == null)
                return null;

            var friendship = await _context.Friendships
                .Where(fs => fs.SubjectId == user.Id && fs.ObjectId == friendId)
                .SingleOrDefaultAsync();

            if (friendship == null)
                return null;

            _context.Friendships.Remove(friendship);

            await _context.SaveChangesAsync();
            return friend;
        }

        public async Task<UserProfileDto?> UpdateUserProfileAsync(User user, UserProfileDto profile)
        {
            var nickExists = _context.Profiles
                .Where(p => p.Nickname == profile.Nickname && p.UserId != user.Id)
                .Any();

            if (nickExists)
                return null;

            var newProfile = _context.Profiles.Single(up => up.UserId == user.Id);
            newProfile.Nickname = profile.Nickname;
            newProfile.Bio = profile.Bio;
            newProfile.InventionOptions = profile.InventionOptions;
            await _context.SaveChangesAsync();

            return profile;
        }
    }
}
