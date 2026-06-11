namespace ConsoleApp3
{
    internal class AgeInfo
    {
        public int Age { get; private set; }
        public string? ID { get; private set; }

        public AgeInfo(int? age = default, string id = "000AA")
        {
            Age = age ?? default;
            ID = id ?? default;
        }
        public override string ToString()
        {
            return $"Age: {Age}, ID: {ID}";
        }
        public static AgeInfo Add(int? age = default, string id = "000AA")
        {
            return new AgeInfo(age, id);
        }
    }
    internal class Account
    {
        private static int counter;
        static Account()
        {
            counter = 0;
        }

        public int Record { get; private set; }
        public string? FirstName { get; private set; }
        public string? LastName { get; private set; }
        public string? ID { get; private set; }
        public double Balance { get; private set; }

        private readonly object stub;

        public Account(string fn, string ln, string id, double? bal)
        {
            stub = new object();
            FirstName = fn ?? "Unknown";
            LastName = ln ?? "Unknown";
            ID = id ?? "000AA";
            Balance = bal ?? 0;
            Record = counter;
            Interlocked.Increment(ref counter);
        }
        public override string ToString()
        {
            lock (stub) return $"Record #{Record} - First name: {FirstName}, last name: {LastName}, ID: {ID}, balance: {Balance}.";
        }
        public static Account Create(string fn = "Unknown", string ln = "Unknown", string id = "000AA", double? bal = default)
        {
            return new Account(fn, ln, id, bal);
        }
    }
    internal class Program
    {
        private static int Main()
        {
            string str = string.Empty;
            var account = new Account[]
            {
                Account.Create("Ann","Moss","671OL", 4523),
                Account.Create("Tom","Kukuruz","012AD", 7234589),
                Account.Create("Tom","Hanks","459GF", 454512),
                Account.Create("Arnold","Shwarznugger","771AS", 111111),
                Account.Create("Gandalf","Gray","001WX", 100),
                Account.Create("Aragorn","Aratornson","998KL", 20987),
                Account.Create("Sarra","Smith","651BN",98567 ),
                Account.Create("Bubba","Smith","111FT", 9097),
                Account.Create("Ben","Gun","993JH", 1223),
                Account.Create("Demon","Hill","236KL", 18911),
                Account.Create("Stephan","Moss","133ZQ", 19)
            };
            var ageinfo = new AgeInfo[]
            {
                AgeInfo.Add(44,"671OL"),
                AgeInfo.Add(55,"012AD"),
                AgeInfo.Add(12,"459GF"),
                AgeInfo.Add(23,"771AS"),
                AgeInfo.Add(32,"001WX"),
                AgeInfo.Add(35,"998KL"),
                AgeInfo.Add(41,"651BN" ),
                AgeInfo.Add(65,"111FT"),
                AgeInfo.Add(28,"993JH"),
                AgeInfo.Add(51,"236KL"),
                AgeInfo.Add(19,"133ZQ")
            };

            //var someselected = account.Where(tmp => tmp != null)
            //                        .GroupBy(tmp => tmp.LastName);
            //foreach (var group in someselected)
            //{
            //    Console.WriteLine($"Group by: {group.Key}");
            //    foreach (var person in group)
            //    {
            //        Console.WriteLine($"Person: {person}");
            //    }
            //    Console.WriteLine();
            //}

            //var result = account.Where(tmp => tmp != null)
            //                    .Select(tmp => new { FullName = $"{tmp.FirstName} {tmp.LastName}", ID = $"{tmp.ID}" });
            //foreach (var tmp in result)
            //{
            //    Console.WriteLine(tmp.ToString());
            //}

            //var reach = account.Where(tmp => tmp != null)
            //                 .OrderByDescending(tmp => tmp.Balance)
            //                 .Take(0..3);
            //foreach (var tmp in reach)
            //{
            //    Console.WriteLine(tmp);
            //}

            //var sorted = account.Where(tmp => tmp != null)
            //                    .OrderBy(tmp => tmp.FirstName)
            //                    .ThenBy(tmp => tmp.LastName)
            //                    .ThenBy(tmp => tmp.ID)
            //                    .ThenBy(tmp => tmp.Balance);
            //foreach (var tmp in sorted)
            //{
            //    Console.WriteLine(tmp);
            //}

            //var sorted = account.Where(tmp => tmp != null)
            //                    .OrderBy(tmp => tmp.FirstName)
            //                    .ThenBy(tmp => tmp.Record)
            //                    .Skip(2);
            //foreach (var tmp in sorted)
            //{
            //    Console.WriteLine(tmp);
            //}

            var selected = account.Where(tmp => tmp != null)
                                  .Join(
                                         ageinfo,
                                         tmp1 => tmp1.ID,
                                         tmp2 => tmp2.ID,
                                         (tmp1, tmp2) => new { FullName = $"{tmp1.FirstName} {tmp1.LastName}", Age = tmp2.Age });

            Console.WriteLine(selected.ToArray().Length);
            foreach (var tmp in selected)
            {
                Console.WriteLine(tmp);
            }

            return 0;
        }
    }
}