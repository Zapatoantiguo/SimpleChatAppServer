namespace SimpleChatApp_BAL.ErrorHandling.ResultPattern
{
    public static class NotificationErrors
    {
        public static Error InvitationAlreadyExists() => Error.Conflict(
            "Notifications.Conflict", $"User is invited already");

        public static Error NotFound() => Error.NotFound(
            "Notifications.NotFound", $"User is not invited in chat");

        public static Error InvitationNotPermitted() => Error.Forbidden(
            "Invitations.Forbidden", $"You haven't permissions to invite specified user in chat room");
    }
}
