namespace ConsoleApp1
{
    public class Test<T> : IEnumerable, IEnumerable<T>
    {
        private readonly T[] arr;
        private readonly int capacity;

        public Test(int size = 0)
        {
            capacity = (size < 0) ? 0 : size;
            arr = new T[capacity];
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
            for (int i = 0; i < capacity; i++)
            {
                yield return arr[i];
            }
        }
    }

    internal class Program
    {
        private static int Main(string[] args)
        {
            return 0;
        }
    }
}