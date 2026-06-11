namespace ConsoleApp1
{
    public interface IUseless<T> where T : struct, IComparable<T>, INumber<T>
    {
        string Name { get; }
        string Surname { get; }
        T Age { get; }
    }
}