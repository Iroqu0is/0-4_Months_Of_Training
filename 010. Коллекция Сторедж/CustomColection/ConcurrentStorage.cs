global using System.Collections;
global using static System.Math;
using System;

namespace CustomColection
{
    public sealed class ConcurrentStorage<T> : ICollection<T>, IEnumerable<T>, IEnumerable, IComparer<ConcurrentStorage<T>> where T : IComparable<T>, IEquatable<T>
    {
        private static int counter;
        static ConcurrentStorage()
        {
            counter = 0;
        }

        private readonly ReaderWriterLockSlim rw;
        private EventHandler<NotificationEventArgs>? handler;
        private int pollingCycles;
        private readonly int id;
        private bool isDisposed;
        private int capacity;
        private int extCount;
        private int ptr;
        private T[] arr;

        public T this[int index]
        {
            get
            {
                try
                {
                    rw.EnterReadLock();
                    IfDisposed();
                    if (index < 0 || index >= ptr) throw new IndexOutOfRangeException(nameof(index));
                    return arr[index];
                }
                finally
                {
                    rw.ExitReadLock();
                }
            }
        }

        public ReaderWriterLockSlim SynchRoot { get { return rw; } }
        public bool IsSynchronized { get { return true; } }
        public bool IsReadOnly { get { return false; } }
        public int Id { get { return id; } }
        public int ExtentionCount
        {
            get
            {
                try
                {
                    rw.EnterReadLock();
                    IfDisposed();
                    return extCount;
                }
                finally
                {
                    rw.ExitReadLock();
                }
            }
        }
        public int PollingCycles
        {
            get
            {
                try
                {
                    rw.EnterReadLock();
                    IfDisposed();
                    return pollingCycles;
                }
                finally
                {
                    rw.ExitReadLock();
                }
            }
            set
            {
                try
                {
                    rw.EnterWriteLock();
                    IfDisposed();
                    pollingCycles = Max(0, value);
                }
                finally
                {
                    rw.ExitWriteLock();
                }
            }
        }
        public bool IsDisposed
        {
            get
            {
                try
                {
                    rw.EnterReadLock();
                    IfDisposed();
                    return isDisposed;
                }
                finally
                {
                    rw.ExitReadLock();
                }
            }
        }
        public int Capacity
        {
            get
            {
                try
                {
                    rw.EnterReadLock();
                    IfDisposed();
                    return capacity;
                }
                finally
                {
                    rw.ExitReadLock();
                }
            }
        }
        public int Count
        {
            get
            {
                try
                {
                    rw.EnterReadLock();
                    IfDisposed();
                    return ptr;
                }
                finally
                {
                    rw.ExitReadLock();
                }
            }
        }


        public ConcurrentStorage() : this(0) { }
        public ConcurrentStorage(int size, EventHandler<NotificationEventArgs>? handler = default)
        {
            id = Interlocked.Increment(ref counter);
            rw = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
            capacity = Max(0, size);
            extCount = 0;
            ptr = 0;
            arr = new T[capacity];
            isDisposed = false;
            if (handler is null) return;
            this.handler = handler;
            pollingCycles = 10000;
        }
        public ConcurrentStorage(ConcurrentStorage<T>? storage) : this(storage?.arr ?? Array.Empty<T>()) { }
        public ConcurrentStorage(IEnumerable<T>? args, EventHandler<NotificationEventArgs>? handler = default)
        {
            id = Interlocked.Increment(ref counter);
            rw = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
            isDisposed = false;
            extCount = 0;
            if (args is null)
            {
                arr = Array.Empty<T>();
                capacity = 0;
                ptr = 0;
                return;
            }
            if (ReferenceEquals(args, Array.Empty<T>()))
            {
                arr = Array.Empty<T>();
                capacity = 0;
                ptr = 0;
                return;
            }
            arr = args.ToArray();
            ptr = arr.Length;
            capacity = ptr;
            if (handler is null) return;
            this.handler = handler;
            pollingCycles = 10000;
        }

        private static bool Equals(ConcurrentStorage<T>? s1, ConcurrentStorage<T>? s2)
        {
            return ReferenceEquals(s1, s2);
        }
        private static int CompareTo(ConcurrentStorage<T>? s1, ConcurrentStorage<T>? s2)
        {
            if (ReferenceEquals(s1, s2)) return 0;
            if (s1 is null) return -1;
            if (s2 is null) return 1;
            return s1.ptr.CompareTo(s2.ptr);
        }

        private T[] TakeSnapshot()
        {
            try
            {
                rw.EnterWriteLock();
                IfDisposed();
                var snap = new T[ptr];
                Array.Copy(arr, snap, ptr);
                return snap;
            }
            finally
            {
                rw.ExitWriteLock();
            }
        }
        private void IfDisposed()
        {
            if (isDisposed) throw new ObjectDisposedException($"Instance({id})");
        }
        private void Extend(int arg)
        {
            extCount++;
            capacity = arg;
            var newArr = new T[arg];
            Array.Copy(arr, newArr, ptr);
            arr = newArr;
        }
        private void ShiftLeft(int arg)
        {
            for (int i = arg; i < ptr; i++)
            {
                arr[i] = arr[i + 1];
            }
            arr[ptr - 1] = default!;
            ptr--;
        }

        public T[] FindAll(Func<T, bool>? condition, CancellationToken token = default)
        {
            var str = new ConcurrentStorage<T>(0);
            try
            {
                rw.EnterWriteLock();// - потому что есть метод Extend
                IfDisposed();
                if (ptr == 0 || condition is null) return Array.Empty<T>();
                for (int i = 0; i < ptr; i++)
                {
                    if (i % pollingCycles == 0) token.ThrowIfCancellationRequested();
                    if (condition(arr[i])) str.Add(arr[i]);
                }
                if (str.Count == 0) return Array.Empty<T>();
                Extend(str.Count);
                return str.arr;
            }
            finally
            {
                rw.ExitWriteLock();
            }
        }
        public bool HasAny(Func<T, bool>? condition, CancellationToken token = default)
        {
            try
            {
                rw.EnterReadLock();
                IfDisposed();
                if (ptr < 1 || condition is null) return false;
                for (int i = 0; i < ptr; i++)
                {
                    if (i % pollingCycles == 0) token.ThrowIfCancellationRequested();
                    if (condition(arr[i])) return true;
                }
                return false;
            }
            finally { rw.ExitReadLock(); }
        }
        public int IndexOf(T? item, CancellationToken token = default)
        {
            try
            {
                rw.EnterReadLock();
                IfDisposed();
                if (ptr < 1 || item is null) return -1;
                int index = -1;
                for (int i = 0; i < ptr; i++)
                {
                    if (i % pollingCycles == 0) token.ThrowIfCancellationRequested();
                    if (item.CompareTo(arr[i]) == 0)
                    {
                        index = i;
                        break;
                    }
                }
                return index;
            }
            finally
            {
                rw.ExitReadLock();
            }
        }
        public int LastIndexOf(T? item, CancellationToken token = default)
        {
            try
            {
                rw.EnterReadLock();
                IfDisposed();
                if (ptr < 1 || item is null) return -1;
                int index = -1;
                for (int i = ptr - 1; i >= 0; i--)
                {
                    if (i % pollingCycles == 0) token.ThrowIfCancellationRequested();
                    if (item.CompareTo(arr[i]) == 0)
                    {
                        index = i;
                        break;
                    }
                }
                return index;
            }
            finally
            {
                rw.ExitReadLock();
            }
        }
        public void Sort(bool descending = false)
        {
            try
            {
                rw.EnterWriteLock();
                IfDisposed();
                if (ptr < 1) return;
                Array.Sort(arr, 0, ptr);
                if (descending) Array.Reverse(arr, 0, ptr);
            }
            finally
            {
                rw.ExitWriteLock();
            }
        }
        public void Reverse()
        {
            try
            {
                rw.EnterWriteLock();
                IfDisposed();
                if (ptr < 1) return;
                Array.Reverse(arr, 0, ptr);
            }
            finally
            {
                rw.ExitWriteLock();
            }
        }
        public void CopyTo(T[] array, int index)
        {
            try
            {
                rw.EnterWriteLock();
                IfDisposed();
                if (array is null) return;
                if (index < 0) index = 0;
                if (index >= ptr) index = ptr;
                if (ptr == 0)
                {
                    arr = array;
                    ptr = array.Length;
                    capacity = ptr;
                    return;
                }
                Array.Copy(array, 0, arr, index, array.Length);
            }
            finally
            {
                rw.ExitWriteLock();
            }
        }
        public bool Remove(T? item)
        {
            try
            {
                rw.EnterWriteLock();
                IfDisposed();
                if (ptr < 1 || item is null) return false;
                for (int i = 0; i < ptr; i++)
                {
                    if (arr[i].CompareTo(item) == 0)
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
        public void RemoveAt(int index)
        {
            try
            {
                rw.EnterWriteLock();
                IfDisposed();
                if (ptr < 1 || index >= ptr || index < 0) return;
                arr[index] = default(T)!;
                ShiftLeft(index);
            }
            finally
            {
                rw.ExitWriteLock();
            }
        }
        public bool Remove(T? item, CancellationToken token = default)
        {
            try
            {
                rw.EnterWriteLock();
                IfDisposed();
                if (ptr < 1 || item is null) return false;
                for (int i = 0; i < ptr; i++)
                {
                    if (i % pollingCycles == 0) token.ThrowIfCancellationRequested();
                    if (arr[i].CompareTo(item) == 0)
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
        public int RemoveAll(Func<T, bool>? condition, CancellationToken token = default)
        {
            try
            {
                rw.EnterWriteLock();
                IfDisposed();
                if (ptr == 0 || condition is null) return 0;
                int count = 0;
                for (int i = ptr - 1; i >= 0; i--)
                {
                    if (i % pollingCycles == 0) token.ThrowIfCancellationRequested();
                    if (condition(arr[i]))
                    {
                        arr[i] = default(T)!;
                        ShiftLeft(i);
                        count++;
                    }
                }
                return count;
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
                IfDisposed();
                if (ptr < 1 || item is null) return false;
                for (int i = 0; i < ptr; i++)
                {
                    if (arr[i].CompareTo(item) == 0) return true;
                }
                return false;
            }
            finally
            {
                rw.ExitReadLock();
            }

        }
        public bool Contains(T? item, CancellationToken token = default)
        {
            try
            {
                rw.EnterReadLock();
                IfDisposed();
                if (ptr < 1 || item is null) return false;
                for (int i = 0; i < ptr; i++)
                {
                    if (i % pollingCycles == 0) token.ThrowIfCancellationRequested();
                    if (arr[i].CompareTo(item) == 0) return true;
                }
                return false;
            }
            finally
            {
                rw.ExitReadLock();
            }

        }
        public T? FirstOrDefault()
        {
            try
            {
                rw.EnterReadLock();
                IfDisposed();
                if (ptr < 1) return default;
                return arr[0];
            }
            finally
            {
                rw.ExitReadLock();
            }
        }
        public T? FirstOrDefault(Func<T, bool>? condition, CancellationToken token = default)
        {
            try
            {
                rw.EnterReadLock();
                IfDisposed();
                if (ptr < 1 || condition is null) return default;
                for (int i = 0; i < ptr; i++)
                {
                    if (i % pollingCycles == 0) token.ThrowIfCancellationRequested();
                    if (condition(arr[i])) return arr[i];
                }
                return default!;
            }
            finally
            {
                rw.ExitReadLock();
            }
        }
        public T? LastOrDefault()
        {
            try
            {
                rw.EnterReadLock();
                IfDisposed();
                if (ptr < 1) return default;
                return arr[ptr - 1];
            }
            finally
            {
                rw.ExitReadLock();
            }
        }
        public T? LastOrDefault(Func<T, bool>? condition, CancellationToken token = default)
        {
            try
            {
                rw.EnterReadLock();
                IfDisposed();
                if (ptr < 1 || condition is null) return default;
                for (int i = ptr - 1; i <= 0; i--)
                {
                    if (i % pollingCycles == 0) token.ThrowIfCancellationRequested();
                    if (condition(arr[i])) return arr[i];
                }
                return default!;
            }
            finally
            {
                rw.ExitReadLock();
            }
        }
        public void Add(T? item)
        {
            try
            {
                rw.EnterWriteLock();
                IfDisposed();
                if (item is null) return;
                if (ptr < 1) Extend((ptr + 4) * 2);
                arr[ptr] = item;
                handler?.Invoke(this, new NotificationEventArgs("Item added."));
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
                IfDisposed();
                if (ptr < 1) return;
                Extend(ptr);
            }
            finally
            {
                rw.ExitWriteLock();
            }

        }
        public void Clear()
        {
            try
            {
                rw.EnterWriteLock();
                IfDisposed();
                if (ptr < 1) return;
                for (int i = ptr - 1; i >= 0; i--)
                {
                    arr[i] = default!;
                }
                ptr = 0;
                handler?.Invoke(this, new NotificationEventArgs("Cleared."));
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
            handler = null;
            isDisposed = true;
            GC.SuppressFinalize(this);
            handler?.Invoke(this, new NotificationEventArgs("Disposed."));
        }
        public int Compare(ConcurrentStorage<T>? s1, ConcurrentStorage<T>? s2)
        {
            return ConcurrentStorage<T>.CompareTo(s1, s2);
        }
        public int CompareTo(ConcurrentStorage<T>? storage)
        {
            return ConcurrentStorage<T>.CompareTo(this, storage);
        }
        public bool Equals(ConcurrentStorage<T>? storage)
        {
            return ConcurrentStorage<T>.Equals(this, storage);
        }
        public override bool Equals(object? obj)
        {
            var storage = obj as ConcurrentStorage<T>;
            return ConcurrentStorage<T>.Equals(this, storage);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(id);
        }
        public override string ToString()
        {
            try
            {
                rw.EnterReadLock();
                IfDisposed();
                return $"Instance({id}) has capacity: {capacity} unit(s), fill level: {ptr} unit(s), extension count: {extCount}.";
            }
            finally
            {
                rw.ExitReadLock();
            }
        }

        public static bool operator <(ConcurrentStorage<T>? s1, ConcurrentStorage<T>? s2)
        {
            return ConcurrentStorage<T>.CompareTo(s1, s2) < 0;
        }
        public static bool operator >(ConcurrentStorage<T>? s1, ConcurrentStorage<T>? s2)
        {
            return ConcurrentStorage<T>.CompareTo(s1, s2) > 0;
        }
        public static bool operator <=(ConcurrentStorage<T>? s1, ConcurrentStorage<T>? s2)
        {
            return ConcurrentStorage<T>.CompareTo(s1, s2) <= 0;
        }
        public static bool operator >=(ConcurrentStorage<T>? s1, ConcurrentStorage<T>? s2)
        {
            return ConcurrentStorage<T>.CompareTo(s1, s2) >= 0;
        }
        public static bool operator ==(ConcurrentStorage<T>? s1, ConcurrentStorage<T>? s2)
        {
            return ConcurrentStorage<T>.Equals(s1, s2);
        }
        public static bool operator !=(ConcurrentStorage<T>? s1, ConcurrentStorage<T>? s2)
        {
            return !ConcurrentStorage<T>.Equals(s1, s2);
        }
        public static bool operator true(ConcurrentStorage<T>? storage)
        {
            return storage is not null && storage.ptr != 0;
        }
        public static bool operator false(ConcurrentStorage<T>? storage)
        {
            return storage is null;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator<T>(TakeSnapshot());
        }
    }
}