namespace ConsoleApp1
{
    public class Salvatory<T> : IMyCollection<T>, ICollection<T>, ICollection, IEquatable<Salvatory<T>>, ICloneable where T : IComparable<T>, IEquatable<T>
    {
        private static int counter;
        static Salvatory()
        {
            counter = 0;
        }

        private const bool ISREADONLY = true;
        private const bool ISSINСHRONIZED = true;

        private readonly object stub;
        private int resizeCount;
        private int capacity;
        private int ptr;
        private T[] arr;
        private int id;

        public int Count { get { lock (stub) return ptr; } }
        public int Capacity { get { lock (stub) return capacity; } }
        public bool IsReadOnly { get { lock (stub) return ISREADONLY; } }
        public bool IsSynchronized { get { lock (stub) return ISSINСHRONIZED; } }
        public object SyncRoot { get { lock (stub) return stub; } }// я понял, теперь можно синхронизировать с внешними объектами

        public T this[int idx]
        {
            get
            {
                lock (stub)
                {
                    if (CheckIndex(idx)) return arr[idx];
                    throw new IndexOutOfRangeException();
                }
            }
        }

        public Salvatory() : this(0) { }
        public Salvatory(int size)
        {
            stub = new object();
            id = Interlocked.Increment(ref counter);
            capacity = Max(0, size);
            ptr = 0;
            arr = new T[capacity];
            resizeCount = 0;
        }
        public Salvatory(IEnumerable<T> args)
        {
            stub = new object();
            id = Interlocked.Increment(ref counter);
            var prepareArray = args.ToArray();
            ptr = prepareArray.Length;
            capacity = ptr;
            arr = new T[capacity];
            Array.Copy(prepareArray, arr, ptr);
            resizeCount = 0;
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // эти методы описаны в моем кастомном интерфейсе IMyCollection<T>

        public T? FindFirst(Func<T, bool>? condition, CancellationToken token = default)
        {
            lock (stub)
            {
                if (capacity == 0 || ptr == 0) return default;
                if (condition is null) return arr[0]; // если условие пустое(null) вернем первый элемент (что бы не выбрасывать исключения)
                for (int i = 0; i < ptr; i++)
                {
                    if (i % 10000 == 0) token.ThrowIfCancellationRequested();
                    if (condition(arr[i])) return arr[i];
                }
                return default;
            }
        }
        public T? FindLast(Func<T, bool>? condition, CancellationToken token = default)
        {
            lock (stub)
            {
                if (capacity == 0 || ptr == 0) return default;
                if (condition is null) return arr[^1];
                for (int i = ptr - 1; i >= 0; i--)
                {
                    if (i % 10000 == 0) token.ThrowIfCancellationRequested();
                    if (condition(arr[i])) return arr[i];
                }
                return default;
            }
        }
        public T[] FindAll(Func<T, bool>? condition, CancellationToken token = default)
        {
            lock (stub)
            {
                if (capacity == 0 || ptr == 0) return Array.Empty<T>();
                if (condition is null) return arr; // если условие пустое(null) вернем весь массив (условий отбора ведь не поставили, значит все подходит)
                var tmp = new Salvatory<T>();
                for (int i = 0; i < ptr; i++)
                {
                    if (i % 10000 == 0) token.ThrowIfCancellationRequested();
                    if (condition(arr[i])) tmp.Add(arr[i]);
                }
                if (tmp.ptr == 0) return Array.Empty<T>();// подсказка от вс2022 не создавать new T[0]
                tmp.Extender(tmp.ptr);// обрезать в размер
                return tmp.arr;
            }
        }

        public int IndexOf(T? item, CancellationToken token = default)
        {
            lock (stub)
            {
                if (capacity == 0 || ptr == 0 || item is null) return -1; // по условию, коллекция не содержит null
                for (int i = 0; i < ptr; i++)
                {
                    if (i % 10000 == 0) token.ThrowIfCancellationRequested();
                    if (arr[i].CompareTo(item) == 0) return i;
                }
                return -1; // если ничего не найдено
            }
        }
        public int LastIndexOf(T? item, CancellationToken token)
        {
            lock (stub)
            {
                if (capacity == 0 || ptr == 0 || item is null) return -1; // по условию, коллекция не содержит null или коллекция пустая
                for (int i = ptr - 1; i >= 0; i--)
                {
                    if (i % 10000 == 0) token.ThrowIfCancellationRequested();
                    if (arr[i].CompareTo(item) == 0) return i;
                }
                return -1; // если ничего не найдено}
            }
        }

        public void RemoveAt(int index)
        {
            lock (stub)
            {
                if (capacity == 0 || ptr == 0) return;
                if (CheckIndex(index)) throw new ArgumentOutOfRangeException(nameof(index));// здесь лучше выбросить исключение, я так думаю
                arr[index] = default!;
                ShiftLeft(index);
            }
        }
        public int RemoveAll(Func<T, bool>? condition, CancellationToken token = default)// вернет -1 если коллекция пустая или нет совпадений
        {
            lock (stub)
            {
                if (capacity == 0 || ptr == 0 || condition is null) return -1;
                int count = 0;
                for (int i = ptr - 1; i >= 0; i--)
                {
                    if (i % 10000 == 0) token.ThrowIfCancellationRequested();
                    if (condition(arr[i]))
                    {
                        arr[i] = default!;
                        ShiftLeft(i);
                        ptr--;
                        count++;
                    }
                }
                return count;
            }
        }

        public void Reverse()
        {
            lock (stub)
            {
                if (capacity == 0 || ptr == 0) return;
                var tmp = arr.Reverse().ToArray();
                arr = tmp;
            }
        }
        public void Sort()
        {
            lock (stub)
            {
                if (capacity == 0 || ptr == 0) return;
                Array.Sort(arr, 0, ptr);
            }
        }
        public void Trim()
        {
            lock (stub)
            {
                if (capacity == 0 || ptr == 0) return;
                Extender(ptr);
            }
        }
        public void Fill(T? item, CancellationToken token = default)
        {
            lock (stub)
            {
                if (capacity == 0 || ptr == 0) return;
                if (item is null) return;
                token.ThrowIfCancellationRequested();
                for (int i = 0; i < ptr; i++)
                {
                    arr[i] = item;
                }
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------
        void ICollection.CopyTo(Array? array, int arrayIndex)
        {
            lock (stub)
            {
                if (capacity == 0 || ptr == 0) return;
                if (array is null) throw new ArgumentNullException(nameof(array));
                Array.Copy(arr, 0, array, arrayIndex, ptr); // сам выбрасит нужное исключение если что то не поместится
            }
        }
        public void CopyTo(T[]? array, int arrayIndex)
        {
            lock (stub)
            {
                if (capacity == 0 || ptr == 0) return;
                if (array is null) throw new ArgumentNullException(nameof(array));
                Array.Copy(arr, 0, array, arrayIndex, ptr); // сам выбрасит нужное исключение если что то не поместится
            }
        }
        public void Add(T? item)
        {
            lock (stub)
            {
                if (item is null) return; // будет просто пропущено
                if (capacity == 0 || (capacity - ptr) < 2) Extender((capacity + 4) * 2);
                arr[ptr] = item;
                ptr++;
            }
        }
        public bool Remove(T? item)
        {
            lock (stub)
            {
                if (capacity == 0 || ptr == 0 || item is null) return false;
                int i = 0;
                for (; i < ptr; i++)
                {
                    if (arr[i].CompareTo(item) == 0)
                    {
                        arr[i] = default!;
                    }
                }
                ShiftLeft(i);
                return false;
            }
        }
        public bool Contains(T? item, CancellationToken token)
        {
            lock (stub)
            {
                if (capacity == 0 || ptr == 0 || (item is null)) return false;
                for (int i = 0; i < ptr; i++)
                {
                    if ((i % 10000) == 0) token.ThrowIfCancellationRequested();
                    if (arr[i].CompareTo(item) == 0) return true;
                }
                return false;
            }
        }
        public bool Contains(T? item)
        {
            lock (stub)
            {
                if (capacity == 0 || ptr == 0 || item is null) return false;
                for (int i = 0; i < ptr; i++)
                {
                    if (arr[i].CompareTo(item) == 0) return true;
                }
                return false;
            }
        }
        public void Clear()
        {
            lock (stub)
            {
                if (capacity == 0 || ptr == 0) return;
                for (int i = 0; i < ptr; i++)
                {
                    arr[i] = default!;
                }
                ptr = 0;
                resizeCount = 0;
            }
        }

        public IEnumerable<T> GetValues()
        {
            return this;
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        public IEnumerator<T> GetEnumerator()
        {
            var tmp = this.Clone() as Salvatory<T>;
            if (tmp is null) throw new InvalidOperationException();
            for (int i = 0; i < tmp.ptr; i++)
            {
                yield return tmp.arr[i];
            }
        }

        public bool Equals(Salvatory<T>? salvatory)
        {
            lock (stub) return ReferenceEquals(this, salvatory);
        }
        public override bool Equals(object? obj)
        {
            lock (stub)
            {
                var salvatory = obj as Salvatory<T>;
                return ReferenceEquals(this, salvatory);
            }
        }
        public override int GetHashCode()
        {
            lock (stub) return HashCode.Combine(id);
        }
        public override string ToString()
        {
            lock (stub) return $"Instance({id}) has capacity - {capacity} unit(s), count - {ptr} unit(s), extentions count - {resizeCount}.";
        }
        public object Clone()
        {
            lock (stub) return new Salvatory<T>(this.GetValues());
        }

        private void ShiftLeft(int idx)
        {
            for (int i = idx; i < ptr - 1; i++)
            {
                arr[i] = arr[i + 1];
            }
            arr[ptr - 1] = default!;
        }
        private bool CheckIndex(int idx)
        {
            return idx >= 0 && idx < ptr;
        }
        private bool Extender(int newSize)
        {
            if (newSize < ptr) return false;
            T[] newArr = new T[newSize];
            Array.Copy(arr, newArr, ptr);
            capacity = newSize;
            arr = newArr;
            resizeCount++;
            return true;
        }
    }
}