namespace ConsoleApp1
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            var ints = new int[] { 7, 3, 8, 9, 8, 4, 9, 3 };
            var sal = new Salvatory<int>(ints);
            foreach (var tmp in sal)
            {
                Console.WriteLine(tmp);
            }

            return 0;
        }
    }
}