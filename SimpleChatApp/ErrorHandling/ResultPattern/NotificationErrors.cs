namespace SimpleChatApp.ErrorHandling.ResultPattern
{
    public static class NotificationErrors
    {
        public static Error InvitationAlreadyExists() => Error.Conflict(
            "Notifications.Conflict", $"User is invited already");

        public static Error NotFound() => Error.NotFound(
            "Notifications.NotFound", $"User is not invited in chat");
    }
}
