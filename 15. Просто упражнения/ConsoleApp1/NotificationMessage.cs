namespace ConsoleApp1
{
    public class NotificationMessage : EventArgs
    {
        private static int counter;
        static NotificationMessage()
        {
            counter = 0;
        }

        private readonly int id;
        private readonly string _message;

        public int RecordNumber { get { return id; } }
        public string Message { get { return _message; } }
        public int MessagesCreated { get { return counter; } }

        public NotificationMessage(string? message = null)
        {
            id = Interlocked.Increment(ref counter);
            _message = message ?? "Notification not recived";
        }

        public override bool Equals(object? obj)
        {
            return ReferenceEquals(this, obj);
        }
        public override int GetHashCode()
        {
            return id;
        }
        public override string ToString()
        {
            return _message;
        }

        public static bool operator ==(NotificationMessage nf1, NotificationMessage nf2)
        {
            return ReferenceEquals(nf1, nf2);
        }
        public static bool operator !=(NotificationMessage nf1, NotificationMessage nf2)
        {
            return !ReferenceEquals(nf1, nf2);
        }
    }
}