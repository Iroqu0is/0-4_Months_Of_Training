using System.Collections;
using System.Diagnostics;
using System.Numerics;
using System.Reflection;
using System.Text;
using static System.Console;
using static System.Math;

namespace StorageLibrary
{
    public sealed class Storage<T> : ICloneable, ICollection<T>, IEquatable<Storage<T>>, IComparable<Storage<T>> where T : IComparable<T>, IEquatable<T>
    {
        private EventHandler<ServiceMessage>? noty;

        private const int SOMEMAGICNUMBER = 10000;
        private const bool ISSYNCHRONIZED = true;
        private const bool ISREADONLY = false;
        private static int counter;
        static Storage()
        {
            counter = 0;
        }

        private int extentionsCounter;
        private readonly object stub;
        private readonly int id;
        private bool isDispose;
        private int capacity;
        private int ptr;
        private T[] arr;

        public T? this[int idx]
        {
            get
            {
                lock (stub)
                {
                    if (idx < 0 || idx > ptr) throw new IndexOutOfRangeException(nameof(idx));
                    if (capacity == 0) throw new InvalidOperationException("Collection is empty.");
                    if (ptr == 0) return default(T);
                    return arr[idx];
                }
            }
        }

        public int Id { get { lock (stub) return id; } }
        public int Count { get { lock (stub) return ptr; } }
        public object SyncRoot { get { lock (stub) return stub; } }
        public int Capacity { get { lock (stub) return capacity; } }


        public bool IsReadOnly { get { lock (stub) return ISREADONLY; } }
        public bool IsSynhronized { get { lock (stub) return ISSYNCHRONIZED; } }

        public Storage() : this(0) { }
        public Storage(int size, EventHandler<ServiceMessage>? notifier = null)
        {
            stub = new object();
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
            stub = new object();
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
        private (T[] FreezeArr, int FreezePtr) Snapshot()
        {
            lock (stub)
            {
                var snapshot = new T[ptr];
                var stopped = ptr;
                Array.Copy(arr, snapshot, ptr);
                return (snapshot, stopped);
            }
        }

        public void Sort(bool descending = false)
        {
            lock (stub)
            {
                if (ptr == 0) return;
                Array.Sort(arr, 0, ptr);
                if (descending) Array.Reverse(arr, 0, ptr);
            }
        }
        public void Reverse()
        {
            lock (stub)
            {
                if (ptr == 0) return;
                Array.Reverse(arr, 0, ptr);
            }
        }

        public void Clear()
        {
            lock (stub)
            {
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
        }

        public T? GetFirst()
        {
            lock (stub)
            {
                if (capacity == 0) throw new InvalidOperationException("Collection is empty.");
                if (ptr == 0) return default(T);
                return arr[0];
            }
        }
        public T? GetLast()
        {
            lock (stub)
            {
                if (capacity == 0) throw new InvalidOperationException("Collection is empty.");
                if (ptr == 0) return default(T);
                return arr[ptr - 1];
            }
        }
        public void Add(T? item)
        {
            lock (stub)
            {
                noty?.Invoke(this, new ServiceMessage("New record added."));
                if (item is null) return;
                if ((capacity - ptr) < 2) Extend((capacity + 4) * 2);
                arr[ptr] = item;
                ptr++;
                return;
            }
        }
        public void Trim()
        {
            lock (stub)
            {
                if (ptr == 0) return;
                Extend(ptr);
            }
        }
        public bool Remove(T? item)
        {
            lock (stub)
            {
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
        }
        public bool Remove(T? item, CancellationToken token)
        {
            lock (stub)
            {
                if (ptr == 0 || item is null) return false;
                for (int i = ptr - 1; i >= 0; i--)
                {
                    if (i % SOMEMAGICNUMBER == 0) token.ThrowIfCancellationRequested();
                    if (item.CompareTo(arr[i]) == 0)
                    {
                        RemoveAt(i);
                        return true;
                    }
                }
                return false;
            }
        }
        public void RemoveAt(int index)
        {
            lock (stub)
            {
                if (ptr == 0 || index >= ptr || ptr < 0) return;
                arr[index] = default(T)!;
                ShiftLeft(index);
            }
        }
        public int RemoveAll(Func<T, bool>? condition, CancellationToken token = default)
        {
            lock (stub)
            {
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
        }
        public void CopyTo(T[]? array, int arrayIndex)
        {
            lock (stub)
            {
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
        }


        public int IndexOf(T? item, CancellationToken token = default)
        {
            if (ptr == 0 || item is null) return -1;
            var tmpArr = Snapshot().FreezeArr;
            var tmpPtr = Snapshot().FreezePtr;
            int index = -1;
            for (int i = 0; i < tmpPtr; i++)
            {
                if (i % SOMEMAGICNUMBER == 0) token.ThrowIfCancellationRequested();
                if (item.CompareTo(tmpArr[i]) == 0)
                {
                    index = i;
                    break;
                }
            }
            return index;
        }
        public int LastIndexOf(T? item, CancellationToken token = default)
        {
            if (ptr == 0 || item is null) return -1;
            var tmpArr = Snapshot().FreezeArr;
            var tmpPtr = Snapshot().FreezePtr;
            int index = -1;
            for (int i = tmpPtr - 1; i >= 0; i--)
            {
                if (i % SOMEMAGICNUMBER == 0) token.ThrowIfCancellationRequested();
                if (item.CompareTo(tmpArr[i]) == 0)
                {
                    index = i;
                    break;
                }
            }
            return index;
        }
        public bool Contains(T? item, CancellationToken token = default)
        {
            if (ptr == 0 || item is null) return false;
            var tmpArr = Snapshot().FreezeArr;
            var tmpPtr = Snapshot().FreezePtr;
            for (int i = 0; i < tmpPtr; i++)
            {
                if (i % SOMEMAGICNUMBER == 0) token.ThrowIfCancellationRequested();
                if (item.CompareTo(tmpArr[i]) == 0) return true;
            }
            return false;
        }
        public bool Contains(T? item)
        {
            if (ptr == 0 || item is null) return false;
            var tmpArr = Snapshot().FreezeArr;
            var tmpPtr = Snapshot().FreezePtr;
            for (int i = 0; i < tmpPtr; i++)
            {
                if (item.CompareTo(tmpArr[i]) == 0) return true;
            }
            return false;
        }
        public T[] FindAll(Func<T, bool>? condition, CancellationToken token = default)
        {
            if (ptr == 0 || condition is null) return Array.Empty<T>();
            var tmpArr = Snapshot().FreezeArr;
            var tmpPtr = Snapshot().FreezePtr;
            var str = new Storage<T>(0);
            for (int i = 0; i < tmpPtr; i++)
            {
                if (i % SOMEMAGICNUMBER == 0) token.ThrowIfCancellationRequested();
                if (condition(tmpArr[i])) str.Add(tmpArr[i]);
            }
            if (str.Count == 0) return Array.Empty<T>();
            str.Trim();
            return str.arr;
        }
        public bool HasAny(Func<T, bool>? condition, CancellationToken token = default)
        {
            if (ptr == 0 || condition is null) return false;
            var tmpArr = Snapshot().FreezeArr;
            var tmpPtr = Snapshot().FreezePtr;
            for (int i = 0; i < tmpPtr; i++)
            {
                if (i % SOMEMAGICNUMBER == 0) token.ThrowIfCancellationRequested();
                if (condition(tmpArr[i])) return true;
            }
            return false;
        }

        public void Dispose()// пока пусть будет так, надо выяснить как правильно это реализовать
        {
            lock (stub)
            {
                if (isDispose) return;
                noty = null;
                isDispose = true;
                GC.Collect();
                return;
            }
        }
        public object Clone()
        {
            return new Storage<T>(Snapshot().FreezeArr);
        }
        public int CompareTo(Storage<T>? storage)
        {
            lock (stub) return Storage<T>.CompareTo(this, storage);
        }
        public bool Equals(Storage<T>? storage)
        {
            lock (stub) return Storage<T>.Equals(this, storage);
        }
        public override bool Equals(object? obj)
        {
            lock (stub)
            {
                var storage = obj as Storage<T>;
                return Storage<T>.Equals(this, storage);
            }
        }
        public override int GetHashCode()
        {
            lock (stub) return HashCode.Combine(id);
        }
        public override string ToString()
        {
            lock (stub) return $"Instance({id}) has: capacity - {capacity}, fill level - {ptr}, extensions - {extentionsCounter}.";
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