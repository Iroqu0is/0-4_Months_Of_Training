namespace CustomColection
{
    public sealed class NotificationEventArgs : EventArgs
    {
        private readonly string _message;
        public string Message { get { return _message; } }
        public NotificationEventArgs(string? message = null)
        {
            _message = message ?? "N/A";
        }
        public override string ToString()
        {
            return _message;
        }
    }
}