namespace ConsoleApp1
{
    public enum CpusLoad : byte { Auto = 0, Half = 2, Optimal = 4, Full = 8 }
    public class CancellationControl : IDisposable
    {
        private const byte SOMEMAGICNUMBER = 5;

        private bool IsDisposed;
        private readonly byte cpus;
        private readonly CancellationTokenSource controller;
        private readonly ParallelOptions options;
        private CancellationToken tracker;

        public CancellationTokenSource Controller
        {
            get
            {
                ThrowIfDisposed(); //подсказано Джемини.
                return controller;
            }
        }
        public CancellationToken Tracker
        {
            get
            {
                ThrowIfDisposed();
                return tracker;
            }
        }
        public ParallelOptions Options
        {
            get
            {
                ThrowIfDisposed();
                return options;
            }
        }

        private CancellationControl(int miliseconds, CpusLoad load)
        {
            cpus = (byte)Environment.ProcessorCount;
            controller = new CancellationTokenSource(miliseconds);
            tracker = controller.Token;
            options = new ParallelOptions();
            options.CancellationToken = controller.Token;
            if (cpus < SOMEMAGICNUMBER) options.MaxDegreeOfParallelism = -1;// есть неопределенность, надо выяснить это значит без ограничений или на усмотрение ос(может планировщика).
            else
            {
                switch (load)
                {
                    case (CpusLoad.Auto): { options.MaxDegreeOfParallelism = -1; break; }
                    case (CpusLoad.Half): { options.MaxDegreeOfParallelism = cpus / 2; break; }
                    case (CpusLoad.Optimal): { options.MaxDegreeOfParallelism = cpus - 2; break; }
                    case (CpusLoad.Full): { options.MaxDegreeOfParallelism = cpus; break; }
                }
            }
        }
        public static CancellationControl Build(int miliseconds = 60000, CpusLoad load = CpusLoad.Auto)
        {
            return new CancellationControl(miliseconds, load);
        }

        private void ThrowIfDisposed()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(CancellationControl));
            }
        }
        public void Dispose()
        {
            if (IsDisposed) return;
            controller.Dispose();
            IsDisposed = true;
            return;
        }
    }
}