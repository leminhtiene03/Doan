using Firebase.Auth;

namespace Game.Online
{
    public static class AuthMessages
    {
        public const string Connecting = "Connecting...";
        public const string FirebaseUnavailable = "Cannot connect to the server. Check your internet connection and restart the game.";
        public const string Working = "Please wait...";
        public const string EnterEmail = "Please enter your email.";
        public const string EnterPassword = "Please enter your password.";
        public const string PasswordTooShort = "Password must be at least 6 characters.";
        public const string PasswordsDoNotMatch = "Passwords do not match.";
        public const string NameLength = "Name must be 3 to 16 characters.";
        public const string ResetEmailSent = "A password reset email has been sent. Check your inbox.";
        public const string AccountLinked = "Account saved. A verification email has been sent.";
        public const string NotSignedIn = "You are not signed in.";
        public const string Unknown = "Something went wrong. Please try again.";
        public const string WrongCredentials = "Wrong email or password.";
        public const string EnterCurrentPassword = "Please enter your current password.";
        public const string WrongCurrentPassword = "Current password is wrong.";
        public const string SamePassword = "New password must be different from the current one.";
        public const string PasswordChanged = "Password changed.";

        public static string FromAuthError(AuthError error) => error switch
        {
            AuthError.InvalidEmail => "Invalid email address.",
            AuthError.MissingEmail => EnterEmail,
            AuthError.MissingPassword => EnterPassword,
            AuthError.WeakPassword => PasswordTooShort,
            AuthError.WrongPassword => WrongCredentials,
            AuthError.UserNotFound => WrongCredentials,
            AuthError.InvalidCredential => WrongCredentials,
            AuthError.EmailAlreadyInUse => "This email is already registered.",
            AuthError.CredentialAlreadyInUse => "This email is already linked to another account.",
            AuthError.ProviderAlreadyLinked => "This account already has an email.",
            AuthError.UserDisabled => "This account has been disabled.",
            AuthError.TooManyRequests => "Too many attempts. Please wait and try again.",
            AuthError.NetworkRequestFailed => "No internet connection.",
            AuthError.RequiresRecentLogin => "Please sign in again and retry.",
            _ => Unknown
        };
    }
}
