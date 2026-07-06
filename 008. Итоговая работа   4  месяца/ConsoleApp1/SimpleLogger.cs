namespace ConsoleApp1
{
    internal class SimpleLogger : IDisposable
    {
        private int counter;

        private readonly ReaderWriterLockSlim rw;
        private StringBuilder log;
        private bool IsDisposed;

        private SimpleLogger()
        {
            counter = 0;
            IsDisposed = false;
            rw = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
            log = new StringBuilder();
        }
        public static SimpleLogger Create()
        {
            return new SimpleLogger();
        }

        public void AddRecord(string? str)
        {
            rw.EnterWriteLock();
            try
            {
                ThrowIfDisposed();
                counter++;
                log.AppendLine($"\nRecord({counter}):");
                log.AppendLine($"Error message: {str ?? "N/A"}\n");
            }
            finally
            {
                rw.ExitWriteLock();
            }
        }
        public string Report(bool clear = true)
        {
            rw.EnterWriteLock();
            string? newLog = string.Empty;
            try
            {
                ThrowIfDisposed();
                if (log.Length == 0)
                {
                    return "No records.";
                }
                newLog = log.ToString();
                if (clear) log.Clear();
            }
            finally
            {
                rw.ExitWriteLock();
            }
            return newLog;
        }


        private void ThrowIfDisposed()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(CancellationControl));
            }
        }
        public void Dispose()
        {
            if (IsDisposed) return;
            rw.Dispose();
            IsDisposed = true;
            return;
        }
    }
}