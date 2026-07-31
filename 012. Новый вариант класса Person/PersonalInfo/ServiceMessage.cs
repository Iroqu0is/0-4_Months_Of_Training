namespace PersonalInfo
{
    public class ServiceMessage : EventArgs
    {
        private readonly string _message;

        public string Message { get { return _message; } }

        public ServiceMessage(string? message = "No message")
        {
            _message = message ?? "No message";
        }
        public override string ToString()
        {
            return $"{_message}";
        }
    }
}