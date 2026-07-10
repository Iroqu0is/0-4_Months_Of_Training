namespace CustomColection
{
    public struct Enumerator<T> : IEnumerator<T>, IEnumerator where T : IComparable<T>, IEquatable<T>
    {
        private readonly int length;
        private readonly T[] arr;
        private int ptr;

        object IEnumerator.Current
        {
            get
            {
                if (ptr < 0 || ptr == length) throw new InvalidOperationException("Не вызывать свойство вне контекста итератора.");
                return arr[ptr];
            }
        }
        public T Current
        {
            get
            {
                if (ptr < 0 || ptr == length) throw new InvalidOperationException("Не вызывать свойство вне контекста итератора.");
                return arr[ptr];
            }

        }

        public Enumerator(T[] arg)
        {
            arr = arg;
            length = arr.Length;
            ptr = -1;
        }

        public bool MoveNext()
        {
            if (ptr < length - 1)
            {
                ptr++;
                return true;
            }
            return false;
        }
        public void Reset()
        {
            ptr = -1;
        }
        public void Dispose() { }
    }
}