namespace StorageLibrary
{
    public struct Enumerator<T> : IEnumerator<T> where T : IComparable<T>, IEquatable<T>
    {
        private readonly T[] arr;
        private int ptr;

        public Enumerator(Storage<T> arg)
        {
            ptr = -1;
            arr = new T[arg.Count];
            for (int i = 0; i < arg.Count; i++)
            {
                arr[i] = arg[i]!;
            }
        }
        object IEnumerator.Current
        {
            get
            {
                if (ptr < 0) throw new MyException("Не вызывать это свойство вне контекста итератора.", $"Current value {nameof(ptr)}: {ptr}");
                return arr[ptr];
            }
        }
        public T Current
        {
            get
            {
                if (ptr < 0) throw new MyException("Не вызывать это свойство вне контекста итератора.", $"Current value {nameof(ptr)}: {ptr}");
                return arr[ptr];
            }
        }
        public bool MoveNext()
        {
            if (ptr < arr.Length - 1)
            {
                ptr++;
                return true;
            }
            Reset();
            return false;
        }
        public void Reset()
        {
            ptr = -1;
        }
        public void Dispose() { }
    }
}