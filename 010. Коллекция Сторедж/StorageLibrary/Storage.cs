using System;
using System.Collections;
using System.Diagnostics;
using System.Numerics;
using System.Reflection;
using System.Text;
using static System.Console;
using static System.Math;

namespace StorageLibrary
{
    public sealed class Storage<T> : ICollection<T>, IEquatable<Storage<T>>, IComparable<Storage<T>> where T : IComparable<T>, IEquatable<T>
    {
        private EventHandler<ServiceMessage>? noty;

        private const int SOMEMAGICNUMBER = 10000;
        private static int counter;
        static Storage()
        {
            counter = 0;
        }

        private readonly ReaderWriterLockSlim rw;
        private int extentionsCounter;
        private readonly int id;
        private bool isDispose;
        private int capacity;
        private int ptr;
        private T[] arr;

        public T? this[int idx]
        {
            get
            {
                try
                {
                    rw.EnterReadLock();
                    IfDisposed();
                    if (idx < 0 || idx >= ptr) throw new IndexOutOfRangeException(nameof(idx));
                    if (capacity == 0) throw new InvalidOperationException("Collection is empty.");
                    if (ptr == 0) return default(T);
                    return arr[idx];
                }
                finally { rw.ExitReadLock(); }
            }
        }

        public int Id
        {
            get
            {
                try
                {
                    rw.EnterReadLock();
                    IfDisposed();
                    return id;
                }
                finally { rw.ExitReadLock(); }
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
                finally { rw.ExitReadLock(); }
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
                finally { rw.ExitReadLock(); }
            }
        }


        public bool IsReadOnly { get { return true; } }
        public bool IsSynhronized { get { return true; } }

        public Storage() : this(0) { }
        public Storage(int size, EventHandler<ServiceMessage>? notifier = null)
        {
            rw = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
            id = Interlocked.Increment(ref counter);
            capacity = Max(0, size);
            ptr = 0;
            arr = new T[capacity];
            extentionsCounter = 0;
            noty = notifier;
            isDispose = false;
        }
        public Storage(IEnumerable<T>? args, EventHandler<ServiceMessage>? condition = null)
        {
            rw = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
            id = Interlocked.Increment(ref counter);
            noty = condition;
            isDispose = false;
            if (args is null)
            {
                capacity = 0;
                ptr = 0;
                arr = new T[0];
                extentionsCounter = 0;
                return;
            }
            var temporary = args.ToArray();
            ptr = temporary.Length;
            capacity = ptr;
            extentionsCounter = 0;
            arr = temporary;
        }

        private static int CompareTo(Storage<T>? s1, Storage<T>? s2)
        {
            if (Storage<T>.Equals(s1, s2)) return 0;
            if (s1 is null) return -1;
            if (s2 is null) return 1;
            if (s1.ptr < s2.ptr) return -1;
            if (s1.ptr > s2.ptr) return 1;
            return 0;
        }
        private static bool Equals(Storage<T>? s1, Storage<T>? s2)
        {
            if (ReferenceEquals(s1, s2)) return true;
            if (s1 is null || s2 is null) return false;
            return s1.id == s2.id;
        }
        private void Extend(int arg)
        {
            var newCapacity = arg;
            var newArr = new T[newCapacity];
            Array.Copy(arr, newArr, ptr);
            capacity = newCapacity;
            arr = newArr;
            extentionsCounter++;
        }
        private void ShiftLeft(int arg)
        {
            for (int i = arg; i < ptr - 1; i++)
            {
                arr[i] = arr[i + 1];
            }
            arr[ptr - 1] = default(T)!;
            ptr--;
        }
        private void IfDisposed()
        {
            if (isDispose) throw new MyException("Уже был вызван метод 'Dispose'.");
        }

        public void Sort(bool descending = false)
        {
            try
            {
                rw.EnterWriteLock();
                IfDisposed();
                if (ptr == 0) return;
                Array.Sort(arr, 0, ptr);
                if (descending) Array.Reverse(arr, 0, ptr);
            }
            finally { rw.ExitWriteLock(); }
        }
        public void Reverse()
        {
            try
            {
                rw.EnterWriteLock();
                IfDisposed();
                if (ptr == 0) return;
                Array.Reverse(arr, 0, ptr);
            }
            finally { rw.ExitWriteLock(); }
        }

        public void Clear()
        {
            try
            {
                rw.EnterWriteLock();
                IfDisposed();
                if (ptr != 0)
                {
                    for (int i = 0; i < ptr; i++)
                    {
                        arr[i] = default(T)!;
                    }
                    ptr = 0;
                }
                extentionsCounter = 0;
                noty?.Invoke(this, new ServiceMessage("Cleared."));
            }
            finally { rw.ExitWriteLock(); }
        }

        public T GetFirst()
        {
            try
            {
                rw.EnterReadLock();
                IfDisposed();
                if (ptr == 0) throw new InvalidOperationException("Collection is empty.");
                return arr[0];
            }
            finally { rw.ExitReadLock(); }
        }
        public T GetLast()
        {
            try
            {
                rw.EnterReadLock();
                IfDisposed();
                if (ptr == 0) throw new InvalidOperationException("Collection is empty.");
                return arr[ptr - 1];
            }
            finally { rw.ExitReadLock(); }
        }
        public void Add(T? item)
        {
            try
            {
                rw.EnterWriteLock();
                IfDisposed();
                if (item is null) return;
                if ((capacity - ptr) < 2) Extend((capacity + 4) * 2);
                arr[ptr] = item;
                ptr++;
                noty?.Invoke(this, new ServiceMessage("New record added."));
                return;
            }
            finally { rw.ExitWriteLock(); }
        }
        public void Trim()
        {
            try
            {
                rw.EnterWriteLock();
                IfDisposed();
                if (ptr == 0) return;
                Extend(ptr);
            }
            finally { rw.ExitWriteLock(); }
        }
        public bool Remove(T? item)
        {
            try
            {
                rw.EnterWriteLock();
                IfDisposed();
                if (ptr == 0 || item is null) return false;
                for (int i = ptr - 1; i >= 0; i--)
                {
                    if (item.CompareTo(arr[i]) == 0)
                    {
                        RemoveAt(i);
                        return true;
                    }
                }
                return false;
            }
            finally { rw.ExitWriteLock(); }
        }
        public bool Remove(T? item, CancellationToken token)
        {
            try
            {
                rw.EnterWriteLock();
                IfDisposed();
                if (ptr == 0 || item is null) return false;
                for (int i = ptr - 1; i >= 0; i--)
                {
                    if (i % SOMEMAGICNUMBER == 0) token.ThrowIfCancellationRequested();
                    if (item.CompareTo(arr[i]) == 0)
                    {
                        arr[i] = default(T)!;
                        ShiftLeft(i);
                        return true;
                    }
                }
                return false;
            }
            finally { rw.ExitWriteLock(); }
        }
        public void RemoveAt(int index)
        {
            try
            {
                rw.EnterWriteLock();
                IfDisposed();
                if (ptr == 0 || index >= ptr || index < 0) return;
                arr[index] = default(T)!;
                ShiftLeft(index);
            }
            finally { rw.ExitWriteLock(); }
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
                    if (i % SOMEMAGICNUMBER == 0) token.ThrowIfCancellationRequested();
                    if (condition(arr[i]))
                    {
                        RemoveAt(i);
                        count++;
                    }
                }
                return count;
            }
            finally { rw.ExitWriteLock(); }
        }
        public void CopyTo(T[]? array, int arrayIndex)
        {
            try
            {
                rw.EnterWriteLock();
                IfDisposed();
                if (array is null) return;
                if (arrayIndex < 0) arrayIndex = 0;
                if (arrayIndex >= ptr) arrayIndex = ptr;
                if (ptr == 0)
                {
                    arr = array;
                    ptr = array.Length;
                    capacity = ptr;
                    return;
                }
                Array.Copy(array, 0, arr, arrayIndex, array.Length);
            }
            finally { rw.ExitWriteLock(); }
        }


        public int IndexOf(T? item, CancellationToken token = default)
        {
            try
            {
                rw.EnterReadLock();
                IfDisposed();
                if (ptr == 0 || item is null) return -1;
                int index = -1;
                for (int i = 0; i < ptr; i++)
                {
                    if (i % SOMEMAGICNUMBER == 0) token.ThrowIfCancellationRequested();
                    if (item.CompareTo(arr[i]) == 0)
                    {
                        index = i;
                        break;
                    }
                }
                return index;
            }
            finally { rw.ExitReadLock(); }
        }
        public int LastIndexOf(T? item, CancellationToken token = default)
        {
            try
            {
                rw.EnterReadLock();
                IfDisposed();
                if (ptr == 0 || item is null) return -1;
                int index = -1;
                for (int i = ptr - 1; i >= 0; i--)
                {
                    if (i % SOMEMAGICNUMBER == 0) token.ThrowIfCancellationRequested();
                    if (item.CompareTo(arr[i]) == 0)
                    {
                        index = i;
                        break;
                    }
                }
                return index;
            }
            finally { rw.ExitReadLock(); }
        }
        public bool Contains(T? item, CancellationToken token = default)
        {
            try
            {
                rw.EnterReadLock();
                IfDisposed();
                if (ptr == 0 || item is null) return false;
                for (int i = 0; i < ptr; i++)
                {
                    if (i % SOMEMAGICNUMBER == 0) token.ThrowIfCancellationRequested();
                    if (item.CompareTo(arr[i]) == 0) return true;
                }
                return false;
            }
            finally { rw.ExitReadLock(); }
        }
        public bool Contains(T? item)
        {
            try
            {
                rw.EnterReadLock();
                IfDisposed();
                if (ptr == 0 || item is null) return false;
                for (int i = 0; i < ptr; i++)
                {
                    if (item.CompareTo(arr[i]) == 0) return true;
                }
                return false;
            }
            finally { rw.ExitReadLock(); }
        }
        public T[] FindAll(Func<T, bool>? condition, CancellationToken token = default)
        {
            var str = new Storage<T>(0);
            try
            {
                rw.EnterWriteLock();// - потому что есть метод Extend
                IfDisposed();
                if (ptr == 0 || condition is null) return Array.Empty<T>();
                for (int i = 0; i < ptr; i++)
                {
                    if (i % SOMEMAGICNUMBER == 0) token.ThrowIfCancellationRequested();
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
                if (ptr == 0 || condition is null) return false;
                for (int i = 0; i < ptr; i++)
                {
                    if (i % SOMEMAGICNUMBER == 0) token.ThrowIfCancellationRequested();
                    if (condition(arr[i])) return true;
                }
                return false;
            }
            finally { rw.ExitReadLock(); }
        }

        public void Dispose()// пока пусть будет так, надо выяснить как правильно это реализовать
        {
            if (isDispose) return;
            try { }
            finally
            {
                rw.Dispose();
                noty = null;
                isDispose = true;
                GC.Collect();
            }
        }

        public int CompareTo(Storage<T>? storage)
        {
            return Storage<T>.CompareTo(this, storage);
        }
        public bool Equals(Storage<T>? storage)
        {
            return Storage<T>.Equals(this, storage);
        }
        public override bool Equals(object? obj)
        {
            var storage = obj as Storage<T>;
            return Storage<T>.Equals(this, storage);
        }
        public override int GetHashCode()
        {
            try
            {
                rw.EnterReadLock();
                return HashCode.Combine(id);
            }
            finally { rw.ExitReadLock(); }
        }
        public override string ToString()
        {
            try
            {
                rw.EnterReadLock();
                IfDisposed();
                return $"Instance({id}) has: capacity - {capacity}, fill level - {ptr}, extensions - {extentionsCounter}.";
            }
            finally { rw.ExitReadLock(); }
        }

        public static bool operator ==(Storage<T>? s1, Storage<T>? s2)
        {
            return Storage<T>.Equals(s1, s2);
        }
        public static bool operator !=(Storage<T>? s1, Storage<T>? s2)
        {
            return !Storage<T>.Equals(s1, s2);
        }
        public static bool operator true(Storage<T>? storage)
        {
            return storage is not null && storage.ptr != 0;// тут над условием еще надо подумать
        }
        public static bool operator false(Storage<T>? storage)
        {
            return storage is null;
        }

        //______________________________________________________________________
        // Блок итерратора

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator<T>(this);
        }
        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator<T>(this);
        }
    }

}
