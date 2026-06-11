namespace ConsoleApp2
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            var arr = new int[] { 2, 8, 5, 3, 7, 5, 4, 8, 2, 5, 7, 3, 5, 7, 8, 5, 4, 8, 5, 3, 5, 7, 5, 4 };
            var sal = new Salvatory<int>();
            Console.WriteLine(sal);
            sal.Add(1);
            sal.Add(2);
            sal.Add(3);
            sal.Add(4);
            foreach (var tmp in sal)
            {
                Console.WriteLine(tmp);
            }
            Console.WriteLine(sal);
            sal.Trim();
            Console.WriteLine(sal);
            return 0;
        }
    }
}