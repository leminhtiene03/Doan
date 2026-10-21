using System;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using UnityEngine;

namespace Game.Online
{
    public static class AuthService
    {
        public const int MinPasswordLength = 6;

        private static Task<bool> initializeTask;

        public static bool IsReady { get; private set; }
        public static FirebaseUser CurrentUser => IsReady ? FirebaseAuth.DefaultInstance.CurrentUser : null;
        public static bool IsSignedIn => CurrentUser != null;
        public static bool IsGuest => CurrentUser != null && CurrentUser.IsAnonymous;
        public static string UserId => CurrentUser?.UserId;
        public static string DisplayName => CurrentUser?.DisplayName;

        private static FirebaseAuth Auth => FirebaseAuth.DefaultInstance;

        public static Task<bool> InitializeAsync() => initializeTask ??= InitializeInternalAsync();

        public static Task<AuthOutcome> SignInWithEmailAsync(string email, string password) => RunAsync(async () =>
        {
            AuthResult result = await Auth.SignInWithEmailAndPasswordAsync(email, password);
            await UserProfileService.EnsureProfileAsync(result.User.DisplayName);
        });

        public static Task<AuthOutcome> RegisterAsync(string email, string password, string displayName) => RunAsync(async () =>
        {
            AuthResult result = await Auth.CreateUserWithEmailAndPasswordAsync(email, password);
            await result.User.UpdateUserProfileAsync(new UserProfile { DisplayName = displayName });
            await UserProfileService.EnsureProfileAsync(displayName);
            _ = result.User.SendEmailVerificationAsync();
        });

        public static Task<AuthOutcome> SignInAsGuestAsync(string displayName) => RunAsync(async () =>
        {
            AuthResult result = await Auth.SignInAnonymouslyAsync();
            await result.User.UpdateUserProfileAsync(new UserProfile { DisplayName = displayName });
            await UserProfileService.EnsureProfileAsync(displayName);
        });

        public static Task<AuthOutcome> LinkGuestToEmailAsync(string email, string password) => RunAsync(async () =>
        {
            Credential credential = EmailAuthProvider.GetCredential(email, password);
            AuthResult result = await CurrentUser.LinkWithCredentialAsync(credential);
            _ = result.User.SendEmailVerificationAsync();
        }, AuthMessages.AccountLinked);

        public static Task<AuthOutcome> SendPasswordResetAsync(string email)
            => RunAsync(() => Auth.SendPasswordResetEmailAsync(email), AuthMessages.ResetEmailSent);

        public static void SignOut()
        {
            if (IsReady)
                Auth.SignOut();
        }

        public static async Task RefreshProfileAsync()
        {
            try
            {
                await UserProfileService.EnsureProfileAsync(DisplayName);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Profile sync skipped: {exception.Message}");
            }
        }

        private static async Task<bool> InitializeInternalAsync()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            FirebaseApp.LogLevel = Firebase.LogLevel.Verbose;
            Debug.Log("[Firebase] Checking dependencies...");
#endif
            DependencyStatus status = await FirebaseApp.CheckAndFixDependenciesAsync();
            IsReady = status == DependencyStatus.Available;
            if (!IsReady)
            {
                Debug.LogError($"Firebase is not available: {status}");
                return false;
            }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log("[Firebase] Dependencies OK. Creating Auth...");
            _ = FirebaseAuth.DefaultInstance;
            Debug.Log("[Firebase] Auth OK. Current user: " + (CurrentUser != null ? CurrentUser.UserId : "none"));
#endif
            return true;
        }

        private static async Task<AuthOutcome> RunAsync(Func<Task> action, string successMessage = "")
        {
            if (!await InitializeAsync())
                return AuthOutcome.Fail(AuthMessages.FirebaseUnavailable);

            try
            {
                await action();
                return AuthOutcome.Ok(successMessage);
            }
            catch (Exception exception)
            {
                return AuthOutcome.Fail(Describe(exception));
            }
        }

        private static string Describe(Exception exception)
        {
            Exception inner = exception is AggregateException aggregate ? aggregate.GetBaseException() : exception;
            if (inner is FirebaseException firebaseException)
                return AuthMessages.FromAuthError((AuthError)firebaseException.ErrorCode);

            Debug.LogException(inner);
            return AuthMessages.Unknown;
        }
    }
}
