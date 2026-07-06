namespace ConsoleApp1
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var methodName = MethodBase.GetCurrentMethod()?.Name ?? "Main";
            Console.WriteLine($"Method '{methodName}' get started.");
            var pile = Pile<Employee>.Create();
            var cc = CancellationControl.Build(2000, CpusLoad.Auto);
            var simpleLoger = SimpleLogger.Create();
            Person.Notify += ErrorHandler;
            pile.NotifyWhenDataChanges += ErrorHandler;
            var timer = Stopwatch.StartNew();


            pile.Push(Employee.Build("Tom", "Kukuruz", 55, "HollyWood").Employee);
            pile.Push(Employee.Build("Silvester", "Sostolovoy", 55, "HollyWood").Employee);
            pile.Push(Employee.Build("Rumplestiltskin", "Unknown", 89, "FarFarAway").Employee);
            pile.Trim();
            Console.WriteLine(pile);


            var someEmployee1 = pile.Pop();
            var someEmployee2 = pile.Pop();
            var someEmployee3 = pile.Pop();

            try
            {


                var dir = Directory.CreateDirectory(@"C:\Temp\DirectoryForTests");
                if (!Directory.Exists(@"C:\Temp\DirectoryForTests")) { throw new MyException("Directory not created."); }

                var task1 = File.WriteAllTextAsync($"{dir.FullName}\\{someEmployee1!.Name}.txt", Serialize(someEmployee1), cc.Tracker);
                var task2 = File.WriteAllTextAsync($"{dir.FullName}\\{someEmployee2!.Name}.txt", Serialize(someEmployee2), cc.Tracker);
                var task3 = File.WriteAllTextAsync($"{dir.FullName}\\{someEmployee3!.Name}.txt", Serialize(someEmployee3), cc.Tracker);

                await Task.WhenAll(task1, task2, task3);

                var task4 = File.ReadAllTextAsync($"{dir.FullName}\\{someEmployee1!.Name}.txt", cc.Tracker);
                var task5 = File.ReadAllTextAsync($"{dir.FullName}\\{someEmployee2!.Name}.txt", cc.Tracker);
                var task6 = File.ReadAllTextAsync($"{dir.FullName}\\{someEmployee3!.Name}.txt", cc.Tracker);

                var results = await Task.WhenAll(task4, task5, task6);

                foreach (var tmp in results)
                {
                    Console.WriteLine(tmp);
                    Console.WriteLine();
                    Console.WriteLine(Deserialize<Employee>(tmp));
                }
            }
            catch (MyException expc) when (true)// пока пусть будет.
            {
                simpleLoger.AddRecord(expc.Message);
            }
            catch (Exception ex)
            {
                simpleLoger.AddRecord(ex.Message);
            }
            finally
            {
                cc.Dispose();
                pile.Dispose();
                Person.Notify -= ErrorHandler;
                pile.NotifyWhenDataChanges -= ErrorHandler;
                timer.Stop();
                Console.WriteLine(simpleLoger.Report(true));
                Console.WriteLine($"Method '{methodName}' stopped in {timer.ElapsedMilliseconds} ms.");
            }
        }
        public static void ErrorHandler(object? sender, ErrorMessage? em)
        {
            if (sender is not null) Console.WriteLine($"\nHashCode: {sender.GetHashCode()}");
            if (em is not null) Console.WriteLine($"Message: {em.Message}\n");
        }
    }
}