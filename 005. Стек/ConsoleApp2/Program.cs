namespace ConsoleApp2
{
    public class Pile<T> : IEnumerable, IUseless<T> where T : struct // буду стараться всегда так разбивать программу на части
    {
        private static int counter; // счетчик объектов, статическая переменная
        static Pile()
        {
            counter = 1;
        }

        // группа переменных объекта
        private readonly int cancellationReminder;
        private readonly CancellationToken token;
        private readonly object stub; // флаг блокировки
        private int extcount;
        private int capacity;
        private string? name;
        private T[] arr;
        private int top;

        // индексатор, скрыт, функции временно не определены
        private T this[int idx]
        {
            get
            {
                if (CheckIndex(idx)) return arr[idx];
                throw new IndexOutOfRangeException();
            }
            set
            {
                if (!CheckIndex(idx)) throw new IndexOutOfRangeException();
                arr[idx] = value;
            }
        }

        // группа свойств для получения основной информации о стеке
        public string Name { get { return name!; } }
        public int Capacity { get { return capacity; } }
        public int ExtCount { get { return extcount; } }
        public int Top { get { return top; } }

        // группа конструкторов
        public Pile(string? instname) : this(default, default, instname) { }
        public Pile(int size, string? instname) : this(size, default, instname) { }
        public Pile(int size = default, CancellationToken tk = default, string? instname = "Stack") //основной конструктор
        {
            stub = new object();
            capacity = Max(0, size);
            extcount = 0;
            token = tk;
            top = 0;
            name = (instname ?? $"Stack") + $"({counter})";
            arr = new T[capacity];
            cancellationReminder = 250000;
            Interlocked.Increment(ref counter);
        }

        // группа служебных методов
        private void CancellationControl(int arg)
        {
            if ((arg % cancellationReminder) == 0) token.ThrowIfCancellationRequested();
        }
        private void Extender(int arg)
        {
            if (arg < top) throw new ExceptionInPile("The new size must not be less than the current stack fill level. // Method 'Extender()'");
            int newCapacity = arg;
            T[] newArr = new T[newCapacity];
            if (top > 10000000)
            {
                Array.Copy(arr, newArr, top); // можно оставить только этот метод
            }
            else
            {
                for (int i = 0; i < top; i++)
                {
                    CancellationControl(i);
                    newArr[i] = arr[i];
                }
            }
            Interlocked.Increment(ref extcount);
            capacity = newCapacity;
            arr = newArr;
        }
        private string FullInfo()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Summary information:");
            sb.AppendLine($"Instance name: {Name};");
            sb.AppendLine($"Capacity: {Capacity};");
            sb.AppendLine($"Fill level: {Top};");
            sb.AppendLine($"Extantion count: {ExtCount};");
            return sb.ToString();
        }
        private bool CheckIndex(int arg)
        {
            return arg >= 0 && arg < top;
        }

        // группа основных методов
        public void Push(T t)
        {
            lock (stub)
            {
                if ((top == 0) || ((capacity - top) < 3)) Extender((top + 5) * 2);
                arr[top] = t;
                top++;
            }
        }
        public void Resize(int arg)
        {
            lock (stub)
            {
                Extender(arg);
            }
        }
        public void Trim()
        {
            lock (stub)
            {
                Extender(top);
            }
        }
        public void Reset()
        {
            lock (stub)
            {
                if (top == 0) throw new ExceptionInPile("Stack is empty.");
                for (int i = top - 1; i >= 0; i--)
                {
                    arr[i] = default;
                }
                extcount = 0;
                top = 0;
            }
        }
        public T Pop()
        {
            lock (stub)
            {
                if (top == 0) throw new ExceptionInPile("Stack is empty.");
                T tmp = arr[top - 1];
                arr[top - 1] = default;
                top--;
                return tmp;
            }
        }
        public T Peek()
        {
            lock (stub)
            {
                if (top == 0) throw new ExceptionInPile("Stack is empty.");
                return arr[top - 1];
            }
        }

        // группа методов для получения суммарной информации
        public string GetInfo(Info inf = Info.Full)
        {
            if (inf == Info.Full) return FullInfo();
            return ToString();
        }
        public override string ToString()
        {
            return $"name: {Name}, capacity: {Capacity}, top: {Top}.";
        }

        // группа итераторов
        IEnumerator IEnumerable.GetEnumerator()
        {
            //for (int i = top - 1; i >= 0; i--)
            //{
            //    yield return arr[i];
            //}
            return GetEnumerator();
        }
        public IEnumerator<T> GetEnumerator()
        {
            for (int i = top - 1; i >= 0; i--)
            {
                yield return arr[i];
            }
        }
        public IEnumerable<T> GetData()
        {
            for (int i = top - 1; i >= 0; i--)
            {
                yield return arr[i];
            }
        }

        // статический метод для создания объекта
        public static Pile<T> Create(int _size, CancellationToken _token = default, string? _name = "Stack")
        {
            return new Pile<T>(_size, _token, _name);
        }
    }
    public class CancellationDirection
    {
        private readonly CancellationTokenSource controller;
        private readonly CancellationToken tracker;
        private ParallelOptions options;

        public CancellationTokenSource Controller { get { return controller; } }
        public CancellationToken Tracker { get { return tracker; } }
        public int ParallelismCount
        {
            get { return options.MaxDegreeOfParallelism; }
            set
            {
                options.MaxDegreeOfParallelism = ((value < 1) || (value > Environment.ProcessorCount - 2)) ? 1 : value;
            }
        }

        public CancellationDirection(int cpus = 1)
        {
            controller = new CancellationTokenSource();
            options = new ParallelOptions();
            tracker = controller.Token;
            options.CancellationToken = controller.Token;
            options.MaxDegreeOfParallelism = Min(cpus, Environment.ProcessorCount - 2);
        }
    }
    public interface IUseless<T>
    {
        string Name { get; }
        int Top { get; }
        int Capacity { get; }
        int ExtCount { get; }

        T Peek();
        T Pop();
        void Push(T t);
        void Reset();
        void Resize(int arg);
        void Trim();
    }
    public enum Info { Short = 0, Full = 2 }
    public class ExceptionInPile : Exception
    {
        public ExceptionInPile() : base() { }
        public ExceptionInPile(string str) : base(str) { }
        public ExceptionInPile(string str, Exception inner) : base(str, inner) { }
    }
    internal class Program
    {
        private static int Main(string[] args)
        {
            Console.WriteLine($"Method 'Main' started.\n");
            var cd = new CancellationDirection(12);
            var timer = Stopwatch.StartNew();
            try
            {
                cd.Controller.CancelAfter(50);
                var pile = Pile<int>.Create(100, cd.Tracker, "MyStack");
                for (int i = 0; i < 12000000; i++)
                {
                    pile.Push(Random.Shared.Next(1, 11));
                }
                pile.Trim();
                Console.WriteLine(pile.GetInfo(Info.Full));
                var selected = pile.GetData().Where(tmp => tmp > 3 && tmp < 9).ToArray();
                foreach (var tmp in selected)
                {
                    Console.WriteLine(tmp);
                }
            }
            catch (ExceptionInPile ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                timer.Stop();
                Console.WriteLine($"\nMethod 'Main' stopped in {timer.ElapsedMilliseconds} ms.");
                cd.Controller.Dispose();
            }
            return 0;
        }
    }
}