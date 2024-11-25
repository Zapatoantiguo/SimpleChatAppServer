using Microsoft.EntityFrameworkCore;
using SimpleChatApp.Models;
using SimpleChatApp.Models.DTO;
using System.Linq;

namespace SimpleChatApp.Data.Services
{
    public class DbDataService : IDbDataService
    {
        AppDbContext _context;
        public DbDataService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ChatRoomDto> CreateChatAsync(User creator, ChatRoomDto chatDto)
        {
            ArgumentNullException.ThrowIfNull(creator);
            ArgumentNullException.ThrowIfNull(chatDto);

            var chat = new ChatRoom
            {
                Name = chatDto.Name,
                Description = chatDto.Description,
                Users = new List<User> { creator }
            };

            _context.ChatRooms.Add(chat);
            await _context.SaveChangesAsync();

            // TODO: implement join table update without extra db roundtrip
            UserChatRoom joinTable = chat.UserChatRoom?.SingleOrDefault(uc => uc.ChatRoomId == chat.ChatRoomId);
            if (joinTable == null) { throw new NullReferenceException(nameof(joinTable)); }
            joinTable.JoinedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return new ChatRoomDto
            {
                Name = chat.Name,
                Description = chat.Description
            };
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
        public async Task<UserProfileDto> UpdateUserProfileAsync(User user, UserProfileDto profile)
        {
            var nickExists = _context.Profiles.Where(p => p.Nickname == profile.Nickname).Any();
            if (nickExists)
            {
                throw new InvalidOperationException($"Attemption to set User Profile Nickname which exists already: {profile.Nickname}");
            }
            var newProfile = _context.Profiles.Single(up => up.UserId == user.Id);
            newProfile.Nickname = profile.Nickname;
            newProfile.Bio = profile.Bio;
            newProfile.InventionOptions = profile.InventionOptions;
            await _context.SaveChangesAsync();

            return profile;
        }

        public async Task<List<UserDto>> GetUsersByPatternAsync(UserSearchDto pattern)
        {
            Task<List<UserDto>> result = _context.Users
                                   .Where(!string.IsNullOrEmpty(pattern.NamePattern) ?
                                      u => u.UserName!.StartsWith(pattern.NamePattern) : u => true)
                                   .Select(u => new UserDto { Name = u.UserName! })
                                   .ToListAsync();

            return await result;
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

        public async Task<FriendDto?> AddFriendAsync(User user, FriendDto friend)
        {
            if (user.UserName == friend.UserName)
                return null;

            var friendUser = await _context.Users
                .Where(u => u.UserName == friend.UserName)
                .SingleOrDefaultAsync();

            if (friendUser == null)
                return null;

            var currentFriendsNames = (await _context.Users
                          .Where(u => u.UserName == user.UserName)
                          .Include(u => u.Friends)
                          .Select(u => u.Friends.Select(fr => new { UserName = fr.UserName! }))
                          .SingleOrDefaultAsync())?.ToList();

            if (currentFriendsNames != null && currentFriendsNames.Any(fr => fr.UserName == friend.UserName))
                return null;    // TODO: find a more flexible way to return undesirable result

            user.Friends.Add(friendUser);
            await _context.SaveChangesAsync();
            return friend;
        }

        public async Task<FriendDto?> RemoveFriendAsync(User user, FriendDto friend)
        {
            if (user.UserName == friend.UserName)
                return null;

            var friendUser = await _context.Users
                .Where(u => u.UserName == friend.UserName)
                .SingleOrDefaultAsync();

            if (friendUser == null)
                return null;

            var currentFriendsNames = (await _context.Users
                          .Where(u => u.UserName == user.UserName)
                          .Include(u => u.Friends)
                          .Select(u => u.Friends.Select(fr => new { UserName = fr.UserName! }))
                          .SingleOrDefaultAsync())?.ToList();

            if (currentFriendsNames != null && currentFriendsNames.Any(fr => fr.UserName == friend.UserName))
                return null;    // TODO: find a more flexible way to return undesirable result

            user.Friends.Remove(friendUser);
            await _context.SaveChangesAsync();
            return friend;
        }

        public async Task<List<FriendDto>> GetAllFriendsAsync(User user)
        {
            var friends = (await _context.Users
                          .Where(u => u.UserName == user.UserName)
                          .Include(u => u.Friends)
                          .Select(u => u.Friends.Select(fr => new FriendDto { UserName = fr.UserName! }))
                          .SingleOrDefaultAsync())?.ToList();

            return friends ?? new List<FriendDto>();
        }

        public async Task<List<ChatRoomDto>> GetUserChats(User user)
        {
            var userChats = (await _context.Users
                .Where(u => u.UserName == user.UserName)
                .Include(u => u.ChatRooms)
                .Select(u => u.ChatRooms.Select(chat => new ChatRoomDto { Name = chat.Name, Description = chat.Description}))
                .SingleOrDefaultAsync())?.ToList();

            return userChats ?? new List<ChatRoomDto>();
        }

        public async Task<List<UserDto>?> GetChatMembers(User user, string chatRoomName)
        {
            // TODO: create a separate method for chat existence and user membership checking? (M1)
            var chat = await _context.ChatRooms
                .Include(ch => ch.Users)
                .SingleOrDefaultAsync(ch => ch.Name == chatRoomName);

            if (chat == null || !chat.Users.Any(u => u.UserName == user.UserName)) 
                return null;

            var result = chat.Users
                .Select(u => new UserDto { Name = u.UserName!, IsAnonimous = u.IsAnonimous })
                .ToList();

            return result;
        }

        public async Task<List<MessageDto>?> GetLastMessages(User user, string chatRoomName, uint pageNumber, uint pageSize)
        {
            // TODO: create a separate method for chat existence and user membership checking? (M1)
            var chat = await _context.ChatRooms
                .Include(ch => ch.Users)
                .SingleOrDefaultAsync(ch => ch.Name == chatRoomName);

            if (chat == null || !chat.Users.Any(u => u.UserName == user.UserName))
                return null;

            int startIndex = (int)(pageNumber * pageSize);
            int endIndex = startIndex + (int)pageSize;

            var messages = await _context.Messages
                .Where(msg => msg.ChatRoomId == chat.ChatRoomId)
                .OrderByDescending(msg => msg.SentAt)
                .Take(startIndex..endIndex)
                .Select(msg => new MessageDto
                {
                    AuthorAlias = msg.AuthorAlias,
                    Content = msg.Content,
                    SentAt = msg.SentAt
                })
                .ToListAsync();

            return messages;
        }
    }
}
