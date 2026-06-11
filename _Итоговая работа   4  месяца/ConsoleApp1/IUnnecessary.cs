namespace ConsoleApp1
{
    public interface IUnnecessary<T>
    {
        void Push(T? arg);
        T? Pop();
        T? Peek();
        void Trim();
    }
}
