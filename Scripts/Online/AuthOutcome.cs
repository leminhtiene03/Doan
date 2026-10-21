namespace Game.Online
{
    public readonly struct AuthOutcome
    {
        private AuthOutcome(bool success, string message)
        {
            Success = success;
            Message = message;
        }

        public bool Success { get; }
        public string Message { get; }

        public static AuthOutcome Ok(string message = "") => new(true, message);

        public static AuthOutcome Fail(string message) => new(false, message);
    }
}
