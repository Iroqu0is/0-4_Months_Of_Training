namespace StorageLibrary
{
    public class SimpleLogger
    {
        private static int logID;
        static SimpleLogger()
        {
            logID = 0;
        }

        private int counter;
        private readonly object stub;
        private readonly StringBuilder log;

        public SimpleLogger()
        {
            logID = Interlocked.Increment(ref logID);
            stub = new object();
            counter = 0;
            log = new StringBuilder();
        }

        public void AddRecord(string? message)
        {
            lock (stub)
            {
                counter++;
                log.AppendLine($"Record(#{counter}):");
                log.AppendLine($"{message ?? "N/A"}");
                log.AppendLine($"\n---------------------------------------------------------\n");
            }
        }
        public void AddRecord(MyException? mex)
        {
            lock (stub)
            {
                if (mex is null) return;
                counter++;
                log.AppendLine($"Record(#{counter}):");
                log.AppendLine($"Main error message: {mex.Message ?? "N/A"}");
                log.AppendLine($"Source: {mex.Source ?? "N/A"}");
                log.AppendLine($"Stack trace: {mex.StackTrace ?? "N/A"}");
                if (mex.Value is not null) log.AppendLine($"Additional info: {mex.Value}");
                log.AppendLine($"\n---------------------------------------------------------\n");
            }
        }
        public void AddRecord(ServiceMessage message)
        {
            lock (stub)
            {
                counter++;
                log.AppendLine($"Record(#{counter}):");
                log.AppendLine($"{message.Message}");
                log.AppendLine($"\n---------------------------------------------------------\n");
            }
        }
        public void Report()
        {
            lock (stub) Console.WriteLine(log);
        }
        public string GetLog()
        {
            lock (stub) return log.ToString();

        }
        public void Clear()
        {
            lock (stub) log.Clear();
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(logID);
        }
        public override string ToString()
        {
            return $"Logger has {counter} record(s).";
        }
        public override bool Equals(object? obj)
        {
            var logger = obj as SimpleLogger;
            return ReferenceEquals(this, logger);
        }
    }
}