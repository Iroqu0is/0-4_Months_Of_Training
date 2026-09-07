global using System.Collections;
global using System.Diagnostics;
global using System.Reflection;
global using System.Text;
global using static System.Math;

namespace ConsoleApp1
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            var cc = new ConcurrentCollection<int>();
            cc.Add(9);
            cc.Add(1);
            cc.Add(2);
            cc.Add(3);
            cc.Add(4);
            cc.Add(5);
            cc.Add(6);
            cc.Remove(9);
            cc.Trim();

            foreach (int i in cc)
            {
                Console.WriteLine(i);
            }

            return 0;
        }
    }
}