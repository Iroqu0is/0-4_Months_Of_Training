namespace StorageLibrary
{
    public class ServiceMessage : EventArgs
    {
        private readonly string _message;
        public string Message { get { return _message; } }

        public ServiceMessage(string? message = null)
        {
            _message = message ?? "No message.";
        }
    }
}