namespace ConsoleApp1
{
    public class ConcurrentCollection<T> : Counter, ICollection<T>, IEquatable<ConcurrentCollection<T>> where T : IComparable<T>, IEquatable<T>
    {
        private readonly ReaderWriterLockSlim rw;
        private readonly int instanceNumber;
        private bool isDisposed;
        private int capacity;
        private int extCount;
        private T[] values;
        private int ptr;

        public int InstanceNumber { get { return instanceNumber; } }
        public ReaderWriterLockSlim SyncRoot { get { return rw; } }
        public static int InstancesCreated { get { return counter; } }
        public int ExtensionsCount
        {
            get
            {
                try
                {
                    rw.EnterReadLock();
                    if (isDisposed) throw new ObjectDisposedException(nameof(rw));
                    return extCount;
                }
                finally
                {
                    rw.ExitReadLock();
                }
            }
        }
        public bool IsReadOnly { get { return false; } }
        public bool IsSynchronized { get { return true; } }
        public int Count
        {
            get
            {
                try
                {
                    rw.EnterReadLock();
                    if (isDisposed) throw new ObjectDisposedException(nameof(rw));
                    return ptr;
                }
                finally
                {
                    rw.ExitReadLock();
                }
            }
        }

        public ConcurrentCollection(IEnumerable<T>? args)
        {
            if (args is null) throw new ArgumentNullException(nameof(args));
            instanceNumber = Interlocked.Increment(ref counter);
            rw = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
            isDisposed = false;
            extCount = 0;
            if (args.Count() == 0)
            {
                values = Array.Empty<T>();
                capacity = 0;
                ptr = 0;
                return;
            }
            values = args.ToArray();
            capacity = values.Length;
            ptr = capacity;
        }
        public ConcurrentCollection(int size = 0)
        {
            instanceNumber = Interlocked.Increment(ref counter);
            rw = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
            isDisposed = false;
            extCount = 0;
            ptr = 0;
            capacity = Max(0, size);
            values = new T[capacity];
        }

        public void Add(T? item)
        {
            try
            {
                rw.EnterWriteLock();
                if (isDisposed) throw new ObjectDisposedException(nameof(rw));
                if (item is null) return;
                if (capacity == 0 || (capacity - ptr) < 3) Extender((capacity + 4) * 2);
                values[ptr] = item;
                ptr++;
            }
            finally
            {
                rw.ExitWriteLock();
            }
        }
        public void Add(T? item, Action<NotificationMessage> action)
        {
            try
            {
                rw.EnterWriteLock();
                if (isDisposed) throw new ObjectDisposedException(nameof(rw));
                if (item is null) return;
                if (capacity == 0 || (capacity - ptr) < 3) Extender((capacity + 4) * 2);
                values[ptr] = item;
                ptr++;
            }
            finally
            {
                rw.ExitWriteLock();
            }
            action?.Invoke(new NotificationMessage("New value added"));
        }
        public void Clear()
        {
            try
            {
                rw.EnterWriteLock();
                if (isDisposed) throw new ObjectDisposedException(nameof(rw));
                for (int i = 0; i < ptr; i++)
                {
                    values[i] = default!;
                }
                ptr = 0;
                extCount = 0;
            }
            finally
            {
                rw.ExitWriteLock();
            }
        }
        public bool Contains(T? item)
        {
            try
            {
                rw.EnterReadLock();
                if (isDisposed) throw new ObjectDisposedException(nameof(rw));
                if (item is null || ptr == 0) return false;
                for (int i = 0; i < ptr; i++)
                {
                    if (values[i].CompareTo(item) == 0) return true;
                }
                return false;
            }
            finally
            {
                rw.ExitReadLock();
            }
        }
        public bool Contains(T? item, CancellationToken cancellationToken)
        {
            try
            {
                rw.EnterReadLock();
                if (isDisposed) throw new ObjectDisposedException(nameof(rw));
                if (item is null || ptr == 0) return false;
                for (int i = 0; i < ptr; i++)
                {
                    if (values[i].CompareTo(item) == 0) return true;
                    if (i % 1000 == 0) cancellationToken.ThrowIfCancellationRequested();
                }
                return false;
            }
            finally
            {
                rw.ExitReadLock();
            }
        }
        public bool Contains(Func<T?, bool>? condition, CancellationToken cancellationToken)
        {
            try
            {
                rw.EnterReadLock();
                if (isDisposed) throw new ObjectDisposedException(nameof(rw));
                if (condition is null || ptr == 0) return false;
                for (int i = 0; i < ptr; i++)
                {
                    if (condition.Invoke(values[i])) return true;
                    if (i % 1000 == 0) cancellationToken.ThrowIfCancellationRequested();
                }
                return false;
            }
            finally
            {
                rw.ExitReadLock();
            }
        }
        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array is null) throw new ArgumentNullException(nameof(array));
            if (arrayIndex < 0) throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            try
            {
                rw.EnterReadLock();
                if (isDisposed) throw new ObjectDisposedException(nameof(rw));
                if (array.Length - arrayIndex < ptr) throw new ArgumentException("В целевом массиве нет места для копирования всех элементов.");
                if (ptr < 1) return;
                Array.Copy(values, 0, array, arrayIndex, ptr);
            }
            finally
            {
                rw.ExitReadLock();
            }
        }
        public bool Remove(T? item)
        {
            try
            {
                rw.EnterWriteLock();
                if (ptr == 0 || item is null) return false;
                for (int i = 0; i < ptr; i++)
                {
                    if (values[i].CompareTo(item) == 0)
                    {
                        ShiftLeft(i);
                        return true;
                    }
                }
                return false;
            }
            finally
            {
                rw.ExitWriteLock();
            }
        }
        public bool Remove(T? item, CancellationToken cancellationToken)
        {
            try
            {
                rw.EnterWriteLock();
                if (ptr == 0 || item is null) return false;
                for (int i = 0; i < ptr; i++)
                {
                    if (values[i].CompareTo(item) == 0)
                    {
                        ShiftLeft(i);
                        return true;
                    }
                    if (i % 1000 == 0) cancellationToken.ThrowIfCancellationRequested();
                }
                return false;
            }
            finally
            {
                rw.ExitWriteLock();
            }
        }
        public bool Remove(Func<T?, bool> condition, CancellationToken cancellationToken)
        {
            try
            {
                rw.EnterWriteLock();
                if (ptr == 0 || condition is null) return false;
                for (int i = 0; i < ptr; i++)
                {
                    if (condition.Invoke(values[i]))
                    {
                        ShiftLeft(i);
                        return true;
                    }
                    if (i % 1000 == 0) cancellationToken.ThrowIfCancellationRequested();
                }
                return false;
            }
            finally
            {
                rw.ExitWriteLock();
            }
        }
        public void Trim()
        {
            try
            {
                rw.EnterWriteLock();
                if (isDisposed) throw new ObjectDisposedException(nameof(rw));
                if (ptr != 0) Extender(ptr);
                extCount--;
            }
            finally
            {
                rw.ExitWriteLock();
            }
        }
        public void Dispose()
        {
            if (isDisposed) return;
            rw.Dispose();
            isDisposed = true;
        }
        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < ptr; i++)
            {
                yield return values[i];
            }
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        private void Extender(int newSize)
        {
            var newValues = new T[newSize];
            Array.Copy(values, newValues, ptr);
            values = newValues;
            capacity = newSize;
            extCount++;
        }
        private void ShiftLeft(int start)
        {
            for (int i = start; i < ptr - 1; i++)
            {
                values[i] = values[i + 1];
            }
            values[ptr - 1] = default!;
            ptr--;
        }

        public bool Equals(ConcurrentCollection<T>? cc)
        {
            return ReferenceEquals(this, cc);
        }
        public override bool Equals(object? obj)
        {
            return ReferenceEquals(this, obj);
        }
        public override int GetHashCode()
        {
            return instanceNumber;
        }
        public override string ToString()
        {
            try
            {
                rw.EnterReadLock();
                return $"Record({instanceNumber}): Capacity - {capacity}, level - {ptr}, extensions - {extCount}, is read only - {IsReadOnly}, is synchronized - {IsSynchronized}.";
            }
            finally
            {
                rw.ExitReadLock();
            }
        }

        public static bool operator ==(ConcurrentCollection<T>? cc1, ConcurrentCollection<T>? cc2)
        {
            return Equals(cc1, cc2);
        }
        public static bool operator !=(ConcurrentCollection<T>? cc1, ConcurrentCollection<T>? cc2)
        {
            return !Equals(cc1, cc2);
        }
    }
}