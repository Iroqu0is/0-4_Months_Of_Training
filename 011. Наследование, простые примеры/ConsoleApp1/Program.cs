using System.Diagnostics;
using System.Reflection;

namespace ConsoleApp1
{
    public class MyException : Exception
    {
        private readonly object? _value;
        public object? Value { get { return _value; } }
        public MyException(object? obj = null) : base() { _value = obj; }
        public MyException(string? str, object? obj = null) : base(str) { _value = obj; }
        public MyException(string? str, Exception? inner, object? obj = null) : base(str, inner) { _value = obj; }

        public override string ToString()
        {
            if (_value is null) return Message;
            return $"Main error message: {Message}\nAdditional data: {_value}.";
        }
    }
    internal class Program
    {
        private static int Main(string[] args)
        {
            var methodName = MethodBase.GetCurrentMethod()?.Name;
            Console.WriteLine($"Method '{methodName ?? "Main"}' started.\n");
            var timer = Stopwatch.StartNew();
            try
            {
                var persons = new Person[]
                {
                    Person.Build("Tom","Kukuruz",55).Instance!,
                    Person.Build("Silvester","Sostolovoy",66).Instance!,
                    Person.Build("Tom","Bambadil",99).Instance!,
                    Person.Build("Anton","Baton",29).Instance!,
                    Person.Build("Alice","Selezneva",11).Instance!,
                    Person.Build("Arnold","Shwartznegger",23).Instance!,
                    Person.Build("Rumpel","Stiltskin",91).Instance!,
                    Person.Build("Klara","Ukrala",16).Instance!,
                    Person.Build("Carl","Koralov",13).Instance!,
                    Person.Build("Buba","Smith",45).Instance!,
                    Person.Build("Anton","Chehov",41).Instance!
                };


                var select = persons.Where(arg => arg is not null)
                                    .Distinct()
                                    .OrderBy(arg => arg)
                                    .GroupBy(arg => arg.Name);

                foreach (var tmp in select)
                {
                    Console.WriteLine(tmp.Key);
                    foreach (var item in tmp)
                    {
                        Console.WriteLine(item);
                    }
                    Console.WriteLine();
                }
                throw new MyException("Test !!!!", "One more message (если вдруг одного было мало)");
            }
            catch (MyException ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                Console.WriteLine($"\nMethod '{methodName ?? "Main"}' stopped in {timer.ElapsedMilliseconds}ms.");
                GC.Collect();
            }
            return 0;
        }
    }
}