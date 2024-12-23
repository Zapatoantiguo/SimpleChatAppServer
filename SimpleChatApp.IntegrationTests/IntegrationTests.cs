using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using SimpleChatApp.IntegrationTests.TestModels;
using SimpleChatApp_BAL.DTO;
using SimpleChatApp_BAL.DTO.Auth;
using SimpleChatApp_DAL;
using SimpleChatApp_DAL.Models;
using System.Net;
using System.Text.Json;
using Xunit.Abstractions;

namespace SimpleChatApp.IntegrationTests
{
    [TestCaseOrderer(
    "SimpleChatApp.IntegrationTests.AlphabeticalTestOrderer",
    "SimpleChatApp.IntegrationTests")]
    public class IntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _fixture;
        private readonly ITestOutputHelper _output;
        private string? accessToken;

        JsonSerializerOptions jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public IntegrationTests(CustomWebApplicationFactory fixture,
            ITestOutputHelper output)
        {
            _fixture = fixture;
            _output = output;
        }

        [Fact]
        public async Task _1_RegisterLogin()
        {
            HttpClient client = _fixture.CreateClient();

            foreach (var guy in _fixture.Guys)
            {
                if (guy is RegisteredUser registeredGuy)
                {
                    var response = await client.PostAsJsonAsync<RegisterDto>("/api/account/register", registeredGuy.GetRegisterData());
                    Assert.True(response.IsSuccessStatusCode);
                }
            }
        }

        [Fact]
        public async Task _2_Login()
        {
            HttpClient client = _fixture.CreateClient();
            HttpResponseMessage response = null;
            foreach (var guy in _fixture.Guys)
            {
                if (guy is RegisteredUser registeredGuy)
                {
                    response = await client.PostAsJsonAsync("/api/account/login", registeredGuy.GetLoginData());
                }
                if (guy is AnonUser anonGuy)
                {
                    response = await client.PostAsJsonAsync("/api/account/guestlogin", anonGuy.GetLoginData());
                }

                var content = await response.Content.ReadAsStringAsync();
                var contentDict = JsonSerializer.Deserialize<Dictionary<string, object>>(content);
                guy.AccessToken = contentDict?["accessToken"].ToString();
                Assert.True(response.IsSuccessStatusCode);
            }
        }

        [Fact]
        public async Task _3_CreateChat_Good_Path()
        {
            var user = _fixture.Tom;
            ChatRoomDto chatDto = new()
            {
                Name = "TomChat",
                Description = "Description of Tom's chat"
            };
            
            HttpClient client = _fixture.CreateClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {user.AccessToken}");
            var response = await client.PostAsJsonAsync("api/chats/createchat", chatDto);
         
            Assert.True(response.IsSuccessStatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            
            ChatRoomDto responseDto = JsonSerializer.Deserialize<ChatRoomDto>(responseContent, jsonOptions);
            Assert.NotNull(responseDto);
            Assert.True(chatDto.Name.Equals(responseDto.Name));

            using (var scope = _fixture.ServiceProvider.CreateScope())
            {
                var _dbContext = _fixture.ServiceProvider.GetRequiredService<AppDbContext>();
                ChatRoom chat = _dbContext.ChatRooms.SingleOrDefault(ch => ch.Name == chatDto.Name);
                Assert.NotNull(chat);
            }
        }

        [Fact]
        public async Task _4_UpdateProfile_GoodPath()
        {
            var pom = _fixture.Pom;
            var bom = _fixture.Bom;
            pom.ChatInventionOptions = ChatInventionOptions.ResidentsOnly;
            bom.ChatInventionOptions = ChatInventionOptions.FriendsOnly;

            HttpClient client = _fixture.CreateClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {pom.AccessToken}");
            var response = await client.PostAsJsonAsync("api/usersettings/UpdateProfile", pom.GetProfileData());
            Assert.True(response.IsSuccessStatusCode);

            var content = await response.Content.ReadAsStringAsync();
            var pomResponseDto = JsonSerializer.Deserialize<UserProfileDto>(content, jsonOptions);
            Assert.NotNull(pomResponseDto);
            Assert.Equal(pom.GetProfileData().InventionOptions, pomResponseDto.InventionOptions);

            using (var scope = _fixture.ServiceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetService<AppDbContext>();
                var profile = await dbContext.Profiles.SingleOrDefaultAsync(p => p.Nickname == pom.GetProfileData().Nickname);
                Assert.NotNull(profile);
            }

            client.DefaultRequestHeaders.Remove("Authorization");
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {bom.AccessToken}");
            response = await client.PostAsJsonAsync("api/usersettings/UpdateProfile", bom.GetProfileData());
            Assert.True(response.IsSuccessStatusCode);
        }

        [Fact]
        public async Task _5_AddFriend_GoodPath()
        {
            var pom = _fixture.Pom;
            var bom = _fixture.Bom;
            var bomFriendDto = new FriendDto() { UserName = pom.UserName };

            HttpClient client = _fixture.CreateClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {bom.AccessToken}");
            var response = await client.PostAsJsonAsync("api/society/AddFriend", bomFriendDto);
            Assert.True(response.IsSuccessStatusCode);

            using (var scope = _fixture.ServiceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetService<AppDbContext>();
                var pomInFriends = await dbContext.Users
                    .Include(u => u.FriendsObjects)
                    .Where(u => u.UserName == bom.UserName)
                    .Where(u => u.FriendsObjects.Any(fr => fr.UserName == pom.UserName))
                    .AnyAsync();
                    
                Assert.True(pomInFriends);
            }
        }

        [Fact]
        public async Task _6_InvitationSequence_GoodPath()
        {
            var tom = _fixture.Tom;
            var pom = _fixture.Pom;
            var bom = _fixture.Bom;
            var anon = _fixture.Anon;

            HttpClient client = _fixture.CreateClient();

            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {tom.AccessToken}");
            var response = await client.PostAsync("/api/Chats/InviteToChatRoom?targetUserName=Pom&chatRoomName=TomChat", null);
            Assert.True(response.IsSuccessStatusCode);

            response = await client.PostAsync("api/chats/InviteToChatRoom?targetUserName=Anon1&chatRoomName=TomChat", null);
            Assert.True(response.IsSuccessStatusCode);

            client.DefaultRequestHeaders.Remove("Authorization");
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {anon.AccessToken}");
            response = await client.PostAsync("api/Chats/RespondToInvite?chatRoomName=TomChat&accept=true", null);
            Assert.True(response.IsSuccessStatusCode);

            client.DefaultRequestHeaders.Remove("Authorization");
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {pom.AccessToken}");
            response = await client.PostAsync("api/Chats/RespondToInvite?chatRoomName=TomChat&accept=true", null);
            Assert.True(response.IsSuccessStatusCode);

            response = await client.PostAsync("api/chats/InviteToChatRoom?targetUserName=Bom&chatRoomName=TomChat", null);
            Assert.True(response.IsSuccessStatusCode);

            client.DefaultRequestHeaders.Remove("Authorization");
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {bom.AccessToken}");
            response = await client.PostAsync("api/chats/RespondToInvite?chatRoomName=TomChat&accept=true", null);
            Assert.True(response.IsSuccessStatusCode);

            using (var scope = _fixture.ServiceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetService<AppDbContext>();
                var bomInChat = await dbContext.ChatRooms
                    .Include(c => c.Users)
                    .Where(c => c.Name == "TomChat")
                    .Where(c => c.Users.Any(u => u.UserName == bom.UserName))
                    .AnyAsync();

                Assert.True(bomInChat);
            }
        }

        [Fact]
        public async Task _7_LeaveChat_GoodPath()
        {
            var bom = _fixture.Bom;
            var pom = _fixture.Pom;
            HttpClient client = _fixture.CreateClient();

            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {bom.AccessToken}");
            var response = await client.PostAsync("/api/Chats/LeaveChat?chatRoomName=TomChat", null);
            Assert.True(response.IsSuccessStatusCode);

            client.DefaultRequestHeaders.Remove("Authorization");
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {pom.AccessToken}");
            response = await client.PostAsync("/api/Chats/LeaveChat?chatRoomName=TomChat", null);
            Assert.True(response.IsSuccessStatusCode);
        }

        [Fact]
        public async Task _8_InvitationSequence_InvitationDeniedByRules()
        {
            var tom = _fixture.Tom;
            var pom = _fixture.Pom;
            var bom = _fixture.Bom;
            var anon = _fixture.Anon;

            HttpClient client = _fixture.CreateClient();

            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {tom.AccessToken}");
            var response = await client.PostAsync("/api/Chats/InviteToChatRoom?targetUserName=Bom&chatRoomName=TomChat", null);
            Assert.True(response.StatusCode == HttpStatusCode.Forbidden);

            client.DefaultRequestHeaders.Remove("Authorization");
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {anon.AccessToken}");
            response = await client.PostAsync("/api/Chats/InviteToChatRoom?targetUserName=Pom&chatRoomName=TomChat", null);
            Assert.True(response.StatusCode == HttpStatusCode.Forbidden);

        }

    }
}
