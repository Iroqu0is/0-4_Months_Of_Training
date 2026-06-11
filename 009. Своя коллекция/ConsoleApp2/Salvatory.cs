namespace ConsoleApp2
{
    public sealed class Salvatory<T> : IRepositable<T>, IDisposable, IEquatable<Salvatory<T>>, IEnumerable<T> where T : IComparable<T>, IEquatable<T>
    {
        private static int counter;
        static Salvatory()
        {
            counter = 0;
        }

        private readonly ReaderWriterLockSlim rw;
        private bool isDisposed;
        private int capacity;
        private int resize;
        private int ptr;
        private int id;

        private T[] arr;

        public T? this[int index]
        {
            get
            {
                rw.EnterReadLock();
                try
                {
                    ThrowIfDisposed();
                    if (CheckIndex(index)) return arr[index];
                    throw new IndexOutOfRangeException();
                }
                finally
                {
                    rw.ExitReadLock();
                }
            }
        }

        public int Id { get { return id; } }
        public int Count { get { return ptr; } }
        public int Capacity { get { return capacity; } }

        public Salvatory() : this(0) { }
        public Salvatory(int size)
        {
            id = Interlocked.Increment(ref counter);
            rw = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
            ptr = 0;
            capacity = Max(0, size);
            arr = new T[capacity];
            resize = 0;
        }
        public Salvatory(IEnumerable<T> arg)
        {
            id = Interlocked.Increment(ref counter);
            rw = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
            ptr = arg.Count();
            capacity = ptr;
            arr = new T[capacity];
            Array.Copy(arg.ToArray(), arr, ptr);
            resize = 0;
        }

        public bool TryFind(Func<T, bool>? condition, out T? result)
        {
            rw.EnterReadLock();
            try
            {
                ThrowIfDisposed();
                if (condition is null) throw new ArgumentNullException(nameof(condition));
                result = default(T);
                if (ptr == 0 || capacity == 0) return false;
                for (int i = 0; i < ptr; i++)
                {
                    if (condition(arr[i]))
                    {
                        result = arr[i];
                        return true;
                    }
                }
                return false;
            }
            finally
            {
                rw.ExitReadLock();
            }
        }
        public bool TryFind(Func<T, bool>? condition, out T? result, CancellationToken token)
        {
            rw.EnterReadLock();
            try
            {
                ThrowIfDisposed();
                token.ThrowIfCancellationRequested();
                if (condition is null) throw new ArgumentNullException(nameof(condition));
                result = default(T);
                if (ptr == 0 || capacity == 0) return false;
                for (int i = 0; i < ptr; i++)
                {
                    if ((i % 10000) == 0) token.ThrowIfCancellationRequested();
                    if (condition(arr[i]))
                    {
                        result = arr[i];
                        return true;
                    }
                }
                return false;
            }
            finally
            {
                rw.ExitReadLock();
            }
        }
        public bool TryFindLast(Func<T, bool>? condition, out T? result)
        {
            rw.EnterReadLock();
            try
            {
                ThrowIfDisposed();
                if (condition is null) throw new ArgumentNullException(nameof(condition));
                result = default(T)!;
                if (ptr == 0 || capacity == 0) return false;
                for (int i = ptr - 1; i >= 0; i--)
                {
                    if (condition(arr[i]))
                    {
                        result = arr[i];
                        return true;
                    }
                }
                return false;
            }
            finally
            {
                rw.ExitReadLock();
            }
        }
        public bool TryFindLast(Func<T, bool>? condition, out T? result, CancellationToken token)
        {
            rw.EnterReadLock();
            try
            {
                ThrowIfDisposed();
                token.ThrowIfCancellationRequested();
                if (condition is null) throw new ArgumentNullException(nameof(condition));
                result = default(T)!;
                if (ptr == 0 || capacity == 0) return false;
                for (int i = ptr - 1; i >= 0; i--)
                {
                    if ((i % 10000) == 0) token.ThrowIfCancellationRequested();
                    if (condition(arr[i]))
                    {
                        result = arr[i];
                        return true;
                    }
                }
                return false;
            }
            finally
            {
                rw.ExitReadLock();
            }
        }
        public T[] FindAll(Func<T, bool>? condition)
        {
            rw.EnterReadLock();
            try
            {
                ThrowIfDisposed();
                if ((ptr == 0) || (capacity == 0)) return Array.Empty<T>();
                if (condition is null) throw new ArgumentNullException(nameof(condition));
                var tmp = new Salvatory<T>(0);
                for (int i = 0; i < ptr; i++)
                {
                    if (condition(arr[i])) tmp.Add(arr[i]);
                }
                if (tmp.Count == 0) return new T[0];
                var result = new T[tmp.Count];
                Array.Copy(tmp.arr, result, tmp.Count);
                return result;
            }
            finally
            {
                rw.ExitReadLock();
            }
        }
        public T[] FindAll(Func<T, bool>? condition, CancellationToken token)
        {
            rw.EnterReadLock();
            try
            {
                ThrowIfDisposed();
                token.ThrowIfCancellationRequested();
                if ((ptr == 0) || (capacity == 0)) return Array.Empty<T>();
                if (condition is null) throw new ArgumentNullException(nameof(condition));
                var tmp = new Salvatory<T>(0);
                for (int i = 0; i < ptr; i++)
                {
                    if ((i % 10000) == 0 && token.IsCancellationRequested)
                    {
                        return Array.Empty<T>();
                    }
                    if (condition(arr[i])) tmp.Add(arr[i]);
                }
                if (tmp.Count == 0) return new T[0];
                var result = new T[tmp.Count];
                Array.Copy(tmp.arr, result, tmp.Count);
                return result;
            }
            finally
            {
                rw.ExitReadLock();
            }
        }
        public void RemoveAll(T? arg)
        {
            rw.EnterWriteLock();
            try
            {
                ThrowIfDisposed();
                if ((ptr == 0) || (capacity == 0)) return;
                if (arg is null) throw new ArgumentNullException(nameof(arg));
                for (int i =ptr-1; i >=0; i--)
                {
                    if (arg.CompareTo(arr[i]) == 0)
                    {
                        arr[i] = default(T)!;
                        ShiftLeft(i);
                        ptr--;
                    }
                }
            }
            finally
            {
                rw.ExitWriteLock();
            }
        }
        public void RemoveAll(T? arg, CancellationToken token)
        {
            rw.EnterWriteLock();
            try
            {
                ThrowIfDisposed();
                token.ThrowIfCancellationRequested();
                if ((ptr == 0) || (capacity == 0)) return;
                if (arg is null) throw new ArgumentNullException(nameof(arg));
                for (int i = ptr-1; i >=0; i--)
                {
                    if ((i % 10000) == 0) token.ThrowIfCancellationRequested();
                    if (arg.CompareTo(arr[i]) == 0)
                    {
                        arr[i] = default(T)!;
                        ShiftLeft(i);
                        ptr--;
                    }
                }
            }
            finally
            {
                rw.ExitWriteLock();
            }
        }
        public void Remove(T? arg)
        {
            rw.EnterWriteLock();
            try
            {
                ThrowIfDisposed();
                if ((ptr == 0) || (capacity == 0)) return;
                if (arg is null) throw new ArgumentNullException(nameof(arg));
                for (int i = 0; i < ptr; i++)
                {
                    if (arg.CompareTo(arr[i]) == 0)
                    {
                        arr[i] = default(T)!;
                        ShiftLeft(i);
                        ptr--;
                        return;
                    }
                }
            }
            finally
            {
                rw.ExitWriteLock();
            }
        }
        public void Remove(T? arg, CancellationToken token)
        {
            rw.EnterWriteLock();
            try
            {
                ThrowIfDisposed();
                token.ThrowIfCancellationRequested();
                if ((ptr == 0) || (capacity == 0)) return;
                if (arg is null) throw new ArgumentNullException(nameof(arg));
                for (int i = 0; i < ptr; i++)
                {
                    if ((i % 10000) == 0) token.ThrowIfCancellationRequested();
                    if (arg.CompareTo(arr[i]) == 0)
                    {
                        arr[i] = default(T)!;
                        ShiftLeft(i);
                        ptr--;
                        return;
                    }
                }
            }
            finally
            {
                rw.ExitWriteLock();
            }
        }
        public void RemoveAt(int idx)
        {
            rw.EnterWriteLock();
            try
            {
                ThrowIfDisposed();
                if ((ptr == 0) || (capacity == 0)) return;
                if (!CheckIndex(idx)) throw new IndexOutOfRangeException(nameof(idx));
                arr[idx] = default(T)!;
                ShiftLeft(idx);
                ptr--;
            }
            finally
            {
                rw.ExitWriteLock();
            }
        }
        public void RemoveAt(int idx, CancellationToken token)
        {
            rw.EnterWriteLock();
            try
            {
                ThrowIfDisposed();
                token.ThrowIfCancellationRequested();
                if ((ptr == 0) || (capacity == 0)) return;
                if (!CheckIndex(idx)) throw new IndexOutOfRangeException(nameof(idx));
                arr[idx] = default(T)!;
                ShiftLeft(idx);
                ptr--;
            }
            finally
            {
                rw.ExitWriteLock();
            }
        }
        public void Resize(int newSize)
        {
            rw.EnterWriteLock();
            try
            {
                ThrowIfDisposed();
                if (newSize <= ptr) return;
                Extender(newSize);
            }
            finally
            {
                rw.ExitWriteLock();
            }
        }
        public void Resize(int newSize, CancellationToken token)
        {
            rw.EnterWriteLock();
            try
            {
                ThrowIfDisposed();
                token.ThrowIfCancellationRequested();
                if (newSize <= ptr) return;
                Extender(newSize);
            }
            finally
            {
                rw.ExitWriteLock();
            }
        }
        public void Join(T[]? array)
        {
            rw.EnterWriteLock();
            try
            {
                ThrowIfDisposed();
                if (array is null) return;
                int newSize = ptr;
                checked { newSize += array.Length; }
                Extender(newSize);
                Array.Copy(array, 0, arr, ptr, array.Length);
            }
            finally
            {
                rw.ExitWriteLock();
            }
        }
        public bool Contains(T? arg)
        {
            rw.EnterReadLock();
            try
            {
                ThrowIfDisposed();
                if ((ptr == 0) || (capacity == 0)) return false;
                if (arg is null) throw new ArgumentNullException(nameof(arg));
                for (int i = 0; i < ptr; i++)
                {
                    if (arg.CompareTo(arr[i]) == 0) return true;
                }
                return false;
            }
            finally
            {
                rw.ExitReadLock();
            }
        }
        public bool Contains(T? arg, CancellationToken token)
        {
            rw.EnterReadLock();
            try
            {
                ThrowIfDisposed();
                token.ThrowIfCancellationRequested();
                if ((ptr == 0) || (capacity == 0)) return false;
                if (arg is null) throw new ArgumentNullException(nameof(arg));
                for (int i = 0; i < ptr; i++)
                {
                    if ((i % 10000) == 0) token.ThrowIfCancellationRequested();
                    if (arg.CompareTo(arr[i]) == 0) return true;
                }
                return false;
            }
            finally
            {
                rw.ExitReadLock();
            }
        }
        public void Add(T? arg)
        {
            rw.EnterWriteLock();
            try
            {
                ThrowIfDisposed();
                if (arg is null) throw new ArgumentNullException(nameof(arg));
                if ((capacity == 0) || (capacity - ptr) < 2) Extender((capacity + 4) * 2);
                arr[ptr] = arg;
                ptr++;
            }
            finally
            {
                rw.ExitWriteLock();
            }
        }
        public void Clear()
        {
            rw.EnterWriteLock();
            try
            {
                ThrowIfDisposed();
                if (ptr == 0 || capacity == 0) { resize = 0; return; }
                for (int i = 0; i < ptr; i++)
                {
                    arr[i] = default(T)!;
                }
                ptr = 0;
                resize = 0;
            }
            finally
            {
                rw.ExitWriteLock();
            }
        }
        public void Trim()
        {
            rw.EnterWriteLock();
            try
            {
                ThrowIfDisposed();
                if (ptr < capacity) Extender(ptr);
            }
            finally
            {
                rw.ExitWriteLock();
            }
        }
        public void Sort()
        {
            rw.EnterWriteLock();
            try
            {
                ThrowIfDisposed();
                if (ptr == 0 || capacity == 0) return;
                Array.Sort(arr, 0, ptr);
            }
            finally
            {
                rw.ExitWriteLock();
            }
        }
        public void Revers()
        {
            rw.EnterWriteLock();
            try
            {
                ThrowIfDisposed();
                if (ptr == 0 || capacity == 0) return;
                arr = arr.Reverse().ToArray();
            }
            finally
            {
                rw.ExitWriteLock();
            }
        }
        public int IndexOf(T? arg)
        {
            rw.EnterReadLock();
            try
            {
                ThrowIfDisposed();
                if ((ptr == 0) || (capacity == 0)) return -1;
                if (arg is null) throw new ArgumentNullException(nameof(arg));
                for (int i = 0; i < ptr; i++)
                {
                    if (arg.CompareTo(arr[i]) == 0) return i;
                }
                return -1;
            }
            finally
            {
                rw.ExitReadLock();
            }
        }
        public int LastIndexOf(T? arg)
        {
            rw.EnterReadLock();
            try
            {
                ThrowIfDisposed();
                if ((ptr == 0) || (capacity == 0)) return -1;
                if (arg is null) throw new ArgumentNullException(nameof(arg));
                for (int i = ptr - 1; i >= 0; i--)
                {
                    if (arg.CompareTo(arr[i]) == 0) return i;
                }
                return -1;
            }
            finally
            {
                rw.ExitReadLock();
            }
        }
        public T[] GetRange(int index, int count)
        {
            rw.EnterReadLock();
            try
            {
                ThrowIfDisposed();
                var tmp = new Salvatory<T>();
                if ((ptr == 0) || (capacity == 0)) return Array.Empty<T>();
                if (!CheckIndex(index)) throw new ArgumentOutOfRangeException(nameof(index));
                int limit = ((index + count) > ptr) ? ptr : index + count;
                for (int i = index; i < limit; i++)
                {
                    tmp.Add(arr[i]);
                }
                var newArray = new T[tmp.Count];
                if (newArray.Length == 0) return Array.Empty<T>();
                Array.Copy(tmp.arr, newArray, tmp.Count);
                return newArray;
            }
            finally
            {
                rw.ExitReadLock();
            }
        }

        // для такого метода (public T[] GetRange(int index, int count,CancellationToken token)) надо что то придумать,
        // если его прервать останется неоконченый массив (будет доступна ссылка), на мой взгляд пока его можно тихо прервать,
        // а вообще нужно делать уведомление.
        public T[] GetRange(int index, int count, CancellationToken token)
        {
            rw.EnterReadLock();
            try
            {
                ThrowIfDisposed();
                token.ThrowIfCancellationRequested();
                var tmp = new Salvatory<T>();
                if ((ptr == 0) || (capacity == 0)) return Array.Empty<T>();
                if (!CheckIndex(index)) throw new ArgumentOutOfRangeException(nameof(index));
                int limit = ((index + count) > ptr) ? ptr : index + count;
                for (int i = index; i < limit; i++)
                {
                    if ((i % 10000) == 0 && token.IsCancellationRequested)
                    {
                        // какое то уведомление
                        return Array.Empty<T>();
                    }
                    tmp.Add(arr[i]);
                }
                var newArray = new T[tmp.Count];
                if (newArray.Length == 0) return Array.Empty<T>();
                Array.Copy(tmp.arr, newArray, tmp.Count);
                return newArray;
            }
            finally
            {
                rw.ExitReadLock();
            }
        }

        public bool Equals(Salvatory<T>? salvatory)
        {
            rw.EnterReadLock();
            try
            {
                ThrowIfDisposed();
                return ReferenceEquals(this, salvatory);
            }
            finally
            {
                rw.ExitReadLock();
            }
        }
        public void Dispose()
        {
            if (isDisposed) return;
            rw.Dispose();
            isDisposed = true;
            return;
        }

        public override bool Equals(object? obj)
        {
            rw.EnterReadLock();
            try
            {
                ThrowIfDisposed();
                var salvatory = obj as Salvatory<T>;
                return ReferenceEquals(this, salvatory);
            }
            finally
            {
                rw.ExitReadLock();
            }
        }
        public override int GetHashCode()
        {
            rw.EnterReadLock();
            try
            {
                ThrowIfDisposed();
                return HashCode.Combine(id);
            }
            finally
            {
                rw.ExitReadLock();
            }
        }
        public override string ToString()
        {
            rw.EnterReadLock();
            try
            {
                ThrowIfDisposed();
                return $"Salvatory({id}) has capacity - {capacity} unit(s), current fill level - {ptr} unit(s), resize - {resize}.";
            }
            finally
            {
                rw.ExitReadLock();
            }
        }

        private ref T GetReference(int index)
        {
            if (CheckIndex(index)) return ref arr[index];
            throw new IndexOutOfRangeException();
        }// узнал что так можно, но нарушает инкапсуляцию если сделать открытым
        private void Extender(int newCapacity)
        {
            if (newCapacity < ptr) throw new MyException("Новый размер ниже текущего уровня заполнения.", nameof(newCapacity));
            var newArr = new T[newCapacity];
            if (ptr == 0)
            {
                capacity = newCapacity;
                arr = newArr;
                resize++;
                return;
            }
            Array.Copy(arr, newArr, ptr);
            capacity = newCapacity;
            arr = newArr;
            resize++;
        }
        private void ThrowIfDisposed()
        {
            if (isDisposed) throw new ObjectDisposedException(nameof(Salvatory<T>));
        }
        private bool CheckIndex(int idx)
        {
            return idx >= 0 && idx < capacity;
        }
        private void ShiftLeft(int idx)
        {
            if (ptr == 0 || capacity == 0) return;
            for (int i = idx; i < ptr; i++)
            {
                if (i == capacity - 1) { arr[capacity - 1] = default(T)!; break; }
                arr[i] = arr[i + 1];
            }
        }

        public IEnumerable<T> GetValues()
        {
            foreach (var tmp in arr)
            {
                yield return tmp;
            }
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < ptr; i++)
            {
                yield return arr[i];
            }
        }
    }
}
