namespace ConsoleApp1
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            var ints = new int[] { 0, 2, 5, 3, 5, 8, 7, 5, 4, 2, 0, 5, 8, 5, 6, 6, 5, 2, 7, 0 };
            var sal = new Salvatory<int>(ints);
            var flag = sal.Contains(0);
            Console.WriteLine(flag);
            sal.RemoveAll(arg => arg == 0);
            sal.RemoveAll(arg => arg == 5);
            sal.Trim();
            foreach (var tmp in sal)
            {
                Console.WriteLine(tmp);
            }
            Console.WriteLine(sal);
            return 0;
        }
    }
}