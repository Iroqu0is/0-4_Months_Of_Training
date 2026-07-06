using System.Diagnostics;
using System.Reflection;

namespace ConsoleApp1
{
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
                    Person.Build("Nikol","Kidman",33).Instance!,
                    Person.Build("Anton","Baton",29).Instance!,
                    Person.Build("Alice","Selezneva",11).Instance!,
                    Person.Build("Arnold","Shwartznegger",23).Instance!,
                    Person.Build("Buba","HighTower",35).Instance!,
                    Person.Build("Rumpel","Stiltskin",91).Instance!,
                    Person.Build("Bob","Marlya",33).Instance!,
                    Person.Build("Bob","Prihlop",23).Instance!,
                    Person.Build("Klara","Ukrala",16).Instance!,
                    Person.Build("Carl","Koralov",13).Instance!,
                    Person.Build("Pavlik","Morozov",14).Instance!,
                    Person.Build("Vova","Lenin",36).Instance!,
                    Person.Build("Bob","Marlya",33).Instance!,
                    Person.Build("Buba","Smith",45).Instance!,
                    Person.Build("Anton","Chehov",41).Instance!,
                    Person.Build("Bob","Kidman",11).Instance!,
                    Person.Build("Silvester","Sostolovoy",66).Instance!,
                    Person.Build("Boby","Kommy",53).Instance!
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
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
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