namespace ConsoleApp1
{
    public class ErrorMessage : EventArgs
    {
        private readonly string message;
        public string Message { get { return message; } }
        public ErrorMessage(string? str)
        {
            message = str ?? "N/A";
        }
    }
}