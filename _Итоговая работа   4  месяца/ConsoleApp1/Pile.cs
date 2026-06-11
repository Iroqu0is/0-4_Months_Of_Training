namespace ConsoleApp1
{
    public enum Info : byte { Short = 0, Full = 2 }
    public class Pile<T> : IDisposable, IUnnecessary<T>
    {
        private static int counter;
        static Pile()
        {
            counter = 0;
        }

        public event EventHandler<ErrorMessage>? NotifyWhenDataChanges;

        private readonly ReaderWriterLockSlim rw;
        private readonly string nameInst;
        private uint size;
        private uint ptr;
        private T[] arr;
        private bool IsDisposed;
        private uint extCount;

        public string Name { get { return nameInst; } }
        public uint Capacity { get { return size; } }
        public uint Count { get { return ptr; } }

        private Pile(uint arg)
        {
            rw = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
            IsDisposed = false;
            size = arg;
            ptr = 0;
            arr = new T[size];
            extCount = 0;
            nameInst = $"Instance[{++counter}]";
        }
        public static Pile<T> Create(uint arg = 0)
        {
            return new Pile<T>(arg);
        }

        public void Push(T? arg)
        {
            rw.EnterWriteLock();
            try
            {
                ThrowIfDisposed();
                if ((size == 0) || (size - ptr) <= 2) Extender((size + 4) * 2);
                arr[ptr] = arg!;
                ptr++;
                NotifyWhenDataChanges?.Invoke(this, new ErrorMessage("Новое значение добавлено."));//не очень то и нужно, но пусть будет.
            }
            finally
            {
                rw.ExitWriteLock();
            }
        }

        public T? Pop()
        {
            rw.EnterWriteLock();
            try
            {
                ThrowIfDisposed();
                if (ptr == 0) throw new MyException("Stack is empty.", $"Count: {ptr}");
                var tmp = arr[ptr - 1];
                arr[ptr - 1] = default(T)!;
                ptr--;
                return tmp;
            }
            finally
            {
                rw.ExitWriteLock();
            }
        }
        public bool TryPop(out T? arg)
        {
            rw.EnterWriteLock();
            try
            {
                ThrowIfDisposed();
                if (ptr == 0)
                {
                    arg = default!;
                    return false;
                }
                var tmp = arr[ptr - 1];
                arr[ptr - 1] = default(T)!;
                ptr--;
                arg = tmp;
                return true;
            }
            finally
            {
                rw.ExitWriteLock();
            }
        }
        public T? Peek()
        {
            rw.EnterReadLock();
            try
            {
                ThrowIfDisposed();
                if (ptr == 0) throw new MyException("Stack is empty.", $"Count: {ptr}");
                return arr[ptr - 1];
            }
            finally
            {
                rw.ExitReadLock();
            }
        }
        public bool TryPeek(out T? arg)// может лучше вернуть кортеж ? (T? t, bool IsDone)
        {
            rw.EnterReadLock();
            try
            {
                ThrowIfDisposed();
                if (ptr == 0)
                {
                    arg = default(T);
                    return false;
                }
                arg = arr[ptr - 1];
                return true;
            }
            finally
            {
                rw.ExitReadLock();
            }
        }
        public void Trim()
        {
            rw.EnterWriteLock();
            try
            {
                ThrowIfDisposed();
                Extender(ptr);
            }
            finally
            {
                rw.ExitWriteLock();
            }
        }

        private void Extender(uint newSize)
        {
            var newArr = new T[newSize];
            if (size != 0)
            {
                Array.Copy(arr, newArr, ptr);
            }
            extCount++;
            arr = newArr;
            size = newSize;
        }
        private static bool Equals(Pile<T>? p1, Pile<T>? p2)
        {
            return ReferenceEquals(p1, p2);
        }

        public override bool Equals(object? obj)
        {
            var pile = obj as Pile<T>;
            return Pile<T>.Equals(this, pile);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(Name);
        }
        public override string ToString()
        {
            return $"{Name} has capacity: {Capacity}.";
        }

        public string GetInfo(Info inf = Info.Full)
        {
            rw.EnterReadLock();
            try
            {
                ThrowIfDisposed();
                if (inf == Info.Short) return this.ToString();
                var sb = new StringBuilder();
                sb.AppendLine($"Report:");
                sb.AppendLine($"Instance name: {Name}");
                sb.AppendLine($"Capacity: {Capacity}");
                sb.AppendLine($"Current fill level: {Count}");
                sb.AppendLine($"Extensions count: {extCount}");
                return sb.ToString();
            }
            finally
            {
                rw.ExitReadLock();
            }
        }

        private void ThrowIfDisposed()
        {
            if (IsDisposed) throw new ObjectDisposedException(nameInst);
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