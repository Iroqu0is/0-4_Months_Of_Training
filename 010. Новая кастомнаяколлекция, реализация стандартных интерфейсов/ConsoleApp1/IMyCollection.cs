namespace ConsoleApp1
{
    public interface IMyCollection<T>
    {
        T? FindFirst(Func<T, bool>? condition, CancellationToken token);
        T? FindLast(Func<T, bool>? condition, CancellationToken token);
        T[] FindAll(Func<T, bool>? condition, CancellationToken token);

        int IndexOf(T? item, CancellationToken token);
        int LastIndexOf(T? item, CancellationToken token);

        void RemoveAt(int index);
        int RemoveAll(Func<T, bool>? condition, CancellationToken token);

        void Fill(T? item, CancellationToken token);
        void Trim();
        void Reverse();
        void Sort();

    }
}