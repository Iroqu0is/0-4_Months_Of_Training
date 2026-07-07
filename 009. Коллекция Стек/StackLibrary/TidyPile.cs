using System.Diagnostics;
using System.Reflection;
using System.Text;
using static System.Console;
using static System.Math;

namespace StackLibrary
{
    public class TidyPile<T> : IEquatable<TidyPile<T>>
    {
        private const bool isSynchronized = true;
        private const bool isReadOnly = false;

        private static int counter;
        static TidyPile()
        {
            counter = 0;
        }

        private readonly object stub;
        private readonly int id;
        private int extCounter;
        private int capacity;
        private int ptr;
        private T[] arr;

        private bool isTrimmed;
        private bool isCleared;
        public bool IsTrimmed { get { return isTrimmed; } }
        public bool IsCleared { get { return isCleared; } }

        public bool IsSynchronized { get { return isSynchronized; } }
        public bool IsReadOnly { get { return isReadOnly; } }
        public object SyncRoot { get { return stub; } }

        public int ExtensionCount { get { return extCounter; } }
        public int Capacity { get { return capacity; } }
        public int Count { get { return ptr; } }

        public TidyPile() : this(0) { }
        public TidyPile(int size)
        {
            stub = new object();
            id = Interlocked.Increment(ref counter);
            capacity = Max(0, size);
            ptr = 0;
            extCounter = 0;
            arr = new T[capacity];
            isTrimmed = false;
            isCleared = false;
        }
        public TidyPile(IEnumerable<T>? args)
        {
            stub = new object();
            id = Interlocked.Increment(ref counter);
            isTrimmed = false;
            isCleared = false;
            extCounter = 0;
            if (args is null)
            {
                capacity = 0;
                ptr = 0;
                arr = Array.Empty<T>();
                return;
            }
            var temporary = args.ToArray();
            ptr = temporary.Length;
            capacity = ptr;
            arr = temporary;
        }

        public T Pop()
        {
            lock (stub)
            {
                if (ptr == 0) throw new InvalidOperationException("Stack is empty.");
                var tmp = arr[ptr - 1];
                arr[ptr - 1] = default!;
                ptr--;
                return tmp;
            }
        }
        public T Peek()
        {
            lock (stub)
            {
                if (ptr == 0) throw new InvalidOperationException("Stack is empty.");
                return arr[ptr - 1];
            }
        }
        public void Push(T? item)
        {
            lock (stub)
            {
                if (item is null) return;
                if ((capacity - ptr) < 3) Extend((capacity + 4) * 2);
                arr[ptr] = item;
                ptr++;
            }
        }
        public void Trim()
        {
            lock (stub)
            {
                if (ptr == 0) return;
                Extend(ptr);
                isTrimmed = true;
            }
        }
        public void Clear()
        {
            lock (stub)
            {
                if (ptr == 0) return;
                for (int i = 0; i < ptr; i++)
                {
                    arr[i] = default!;
                }
                ptr = 0;
                extCounter = 0;
                isTrimmed = false;
                isCleared = true;
            }
        }
        public bool Contains(Func<T, bool>? predicate)
        {
            lock (stub)
            {
                if (predicate is null) return true;
                if (ptr == 0) return false;
                for (int i = 0; i < ptr; i++)
                {
                    if (predicate(arr[i])) return true;
                }
                return false;
            }
        }

        private static bool Equals(TidyPile<T>? tp1, TidyPile<T>? tp2)
        {
            return ReferenceEquals(tp1, tp2);
        }
        private (T[] FreezeArr, int FreezePtr) SnapShot()
        {
            lock (stub)
            {
                if (ptr == 0) return (Array.Empty<T>(), 0);
                T[] freezeArr = new T[ptr];
                int freezePtr = ptr;
                Array.Copy(arr, freezeArr, ptr);
                return (freezeArr, freezePtr);
            }
        }
        private void Extend(int newSize)
        {
            var newArr = new T[newSize];
            Array.Copy(arr, newArr, ptr);
            capacity = newSize;
            arr = newArr;
            extCounter++;
        }

        public bool Equals(TidyPile<T>? tidyPile)
        {
            lock (stub) return TidyPile<T>.Equals(this, tidyPile);
        }
        public override bool Equals(object? obj)
        {
            lock (stub)
            {
                var tidyPile = obj as TidyPile<T>;
                return TidyPile<T>.Equals(this, tidyPile);
            }
        }
        public override int GetHashCode()
        {
            lock (stub) return HashCode.Combine<int>(id);
        }
        public override string ToString()
        {
            lock (stub) return $"Instance({id}): has capacity - {capacity} unit(s), fill level - {ptr} unit(s), extension count - {extCounter}, trimmed - {isTrimmed}, cleared - {isCleared}.";
        }
    }
}