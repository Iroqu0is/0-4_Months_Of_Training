namespace ConsoleApp1
{
    public class MyThread
    {
        private static int counter;
        static MyThread()
        {
            counter = 0;
        }

        private string mtname;
        private Thread thread;
        private bool IsParameterized;
        private readonly object instanceStub;

        public string MTName
        {
            get { return thread.Name; }
            set { thread.Name = value; }
        }
        public System.Threading.ThreadState MTState
        {
            get { return thread.ThreadState; }
        }
        public bool IsWorking
        {
            get { return thread.IsAlive; }
        }
        public bool Background
        {
            get { return thread.IsBackground; }
            set { thread.IsBackground = value; }
        }
        public int MTID
        {
            get { return thread.ManagedThreadId; }
        }
        public ThreadPriority MTPriority
        {
            get { return thread.Priority; }
            set { thread.Priority = value; }
        }

        public MyThread(ThreadStart? start, string? name = "Thread", bool background = false)
        {
            int currentId = Interlocked.Increment(ref counter);
            instanceStub = new object();
            if (start == null) throw new ArgumentNullException(nameof(start));
            thread = new Thread(start);
            mtname = name ?? "Thread";
            MTName = $"{mtname}_{currentId}";
            IsParameterized = false;
            Background = background;
        }
        public MyThread(ParameterizedThreadStart? start, string? name = "Thread", bool background = false)
        {
            if (start == null) throw new ArgumentNullException(nameof(start));
            int currentId = Interlocked.Increment(ref counter);
            instanceStub = new object();
            thread = new Thread(start);
            mtname = name ?? "Thread";
            MTName = $"{mtname}_{currentId}";
            IsParameterized = true;
            Background = background;
        }

        public void Run()
        {
            if (IsParameterized) throw new MyException("Use method with parameter.");
            if (thread.ThreadState == System.Threading.ThreadState.Unstarted) thread.Start();
        }
        public void Run(object obj)
        {
            if (!IsParameterized) throw new MyException("Use method with out parameter.");
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            if (thread.ThreadState == System.Threading.ThreadState.Unstarted) thread.Start(obj);
        }
        public void Wait()
        {
            if (thread.IsAlive) thread.Join();
        }
        public override string ToString()
        {
            return $"Name: {MTName}, ID: {MTID}, priority: {MTPriority}, state: {MTState.ToString()}.";
        }

        public static void QuickStart(ThreadStart? start)
        {
            new MyThread(start).Run();
        }
        public static void QuickStart(ParameterizedThreadStart? start, object? obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            new MyThread(start).Run(obj);
        }
    }

    public class MyException : Exception
    {
        public MyException() : base() { }
        public MyException(string str) : base(str) { }
        public MyException(string str, Exception inner) : base(str, inner) { }
    }
    internal class Program
    {
        private static int Main(string[] args)
        {
            var thread = new MyThread[16];
            for (int i = 0; i < thread.Length; i++)
            {
                thread[i] = new MyThread(ParameterizedTask);
                thread[i].Run(4);
            }
            for (int i = 0; i < thread.Length; i++)
            {
                thread[i].Wait();
            }
            return 0;
        }
        static void SimpleTask()
        {
            Console.WriteLine($"{Thread.CurrentThread.Name} - start....");
            var timer = new Stopwatch();
            timer.Start();
            for (long i = 0, j = 1; i < 5000000000L; i++)
            {
                j *= 1;
                j += 1;
                j -= 1;
            }
            timer.Stop();
            Console.WriteLine($"{Thread.CurrentThread.Name} - stop....{timer.ElapsedMilliseconds} ms.");
        }
        static void ParameterizedTask(object? obj)
        {
            if (obj == null) throw new NullReferenceException(nameof(obj));
            bool flag = int.TryParse(obj.ToString(), out int iteration);
            if (!flag || (iteration > 5) || (iteration < 1)) throw new MyException($"Not a number or out of range.");
            Console.WriteLine($"{Thread.CurrentThread.Name} - start");
            var timer = new Stopwatch();
            timer.Start();
            for (long i = 0, j = 1; i < 500000000L * iteration; i++)
            {
                j *= 1;
                j += 1;
                j -= 1;
            }
            timer.Stop();
            Console.WriteLine($"{Thread.CurrentThread.Name} - stop....{timer.ElapsedMilliseconds} ms.");

        }
    }
}