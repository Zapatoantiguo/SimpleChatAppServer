namespace SimpleChatApp.ErrorHandling.ResultPattern
{
    public static class ChatErrors
    {
        public static Error NotFound(string chatName) => Error.NotFound(
            "ChatRooms.NotFound", $"ChatRoom with name {chatName} was not found");
        public static Error NameIsNotUnique() => Error.Conflict(
            "ChatRooms.Conflict", $"Provided chat room name is not unique");
        public static Error UserInChatAlready() => Error.Conflict(
            "ChatRooms.Conflict", $"Specified user is in chat room already");
        public static Error UserIsNotInChat() => Error.Validation(
            "ChatRooms.Validation", $"Specified user is not in chat room");
    }
}
