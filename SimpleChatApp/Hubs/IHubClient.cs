using SimpleChatApp_BAL.DTO;

namespace SimpleChatApp.Hubs
{
    public interface IHubClient
    {
        Task OnInvited(string userName, string chatRoomName);
        Task OnUserJoinedChat(string userName, string chatRoomName);
        Task OnUserLeavedChat(string userName, string chatRoomName);
        Task OnMessageReceived(MessageDto message);
    }
}
