namespace ConsoleApp2
{
    public interface IRepositable<T> where T : IComparable<T>, IEquatable<T>
    {
        int Id { get; }
        int Count { get; }
        int Capacity { get; }

        T this[int index] { get; }

        bool TryFind(Func<T, bool>? condition, out T? result);
        bool TryFindLast(Func<T, bool>? condition, out T? result);
        T[] FindAll(Func<T, bool>? condition);

        void Sort();
        void Revers();
        void Resize(int newSize);
        void Join(T[]? array);
        void Add(T? item);
        void Remove(T? item);
        void RemoveAll(T? item);
        void RemoveAt(int index);
        bool Contains(T? item);
        void Clear();
        void Trim();
        T[] GetRange(int index, int count);
        int IndexOf(T? item);
        int LastIndexOf(T? item);
    }
}