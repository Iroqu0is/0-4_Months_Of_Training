namespace ConsoleApp1
{
    internal class Test
    {
        int a;
        int b;

        public Test(int arg1 = 0, int arg2 = 0)
        {
            a = arg1;
            b = arg2;
        }

        public static Test operator *(Test t1, Test t2)
        {
            return new Test(t1.a * t2.a, t1.b * t2.b);
        }
        public static Test operator *(Test t1, int i)
        {
            return new Test(t1.a * i, t1.b * i);
        }
        public static Test operator *(int i, Test t1)
        {
            return new Test(t1.a * i, t1.b * i);
        }

        public static Test operator /(Test t1, Test t2)
        {
            return new Test(t1.a / t2.a, t1.b / t2.b);
        }
        public static Test operator /(Test t1, int i)
        {
            return new Test(t1.a / i, t1.b / i);
        }
        public static Test operator /(int i, Test t1)
        {
            return new Test(i / t1.a, i / t1.b);
        }

        public static Test operator %(Test t1, Test t2)
        {
            return new Test(t1.a % t2.a, t1.b % t2.b);
        }
        public static Test operator %(Test t1, int i)
        {
            return new Test(t1.a % i, t1.b % i);
        }
        public static Test operator %(int i, Test t1)
        {
            return new Test(i % t1.a, i % t1.b);
        }

        public static Test operator -(Test t1, Test t2)
        {
            return new Test((t1.a - t2.a), (t1.b - t2.b));
        }
        public static Test operator -(Test t, int i)
        {
            return new Test(t.a - i, t.b - i);
        }
        public static Test operator -(int i, Test t)
        {
            return new Test(i - t.a, i - t.b);
        }

        public static Test operator -(Test t)
        {
            return new Test(-t.a, -t.b);
        }
        public static Test operator +(Test t)
        {
            return new Test(+t.a, +t.b);
        }

        public static Test operator ++(Test t)
        {
            return new Test(t.a + 1, t.b + 1);
        }
        public static Test operator --(Test t)
        {
            return new Test(t.a - 1, t.b - 1);
        }

        public static Test operator +(Test t1, Test t2)
        {
            return new Test((t1.a + t2.a), (t1.b + t2.b));
        }
        public static Test operator +(Test t, int i)
        {
            return new Test(t.a + i, t.b + i);
        }
        public static Test operator +(int i, Test t)
        {
            return new Test(t.a + i, t.b + i);
        }

        public static bool operator <(Test t1, Test t2)
        {
            if ((t1.a + t1.b) < (t2.a + t2.b)) return true;
            else return false;
        }
        public static bool operator >(Test t1, Test t2)
        {
            if ((t1.a + t1.b) > (t2.a + t2.b)) return true;
            else return false;
        }
        public static bool operator <=(Test t1, Test t2)
        {
            if ((t1.a + t1.b) <= (t2.a + t2.b)) return true;
            else return false;
        }
        public static bool operator >=(Test t1, Test t2)
        {
            if ((t1.a + t1.b) >= (t2.a + t2.b)) return true;
            else return false;
        }
        public static bool operator ==(Test t1, Test t2)
        {
            if ((t1.a == t2.a) && (t1.b == t2.b)) return true;
            else return false;
        }
        public static bool operator !=(Test t1, Test t2)
        {
            if ((t1.a + t1.b) != (t2.a + t2.b)) return true;
            else return false;
        }

        public static bool operator false(Test t)
        {
            if ((t.a == 0) && (t.b == 0)) return true;
            else return false;
        }
        public static bool operator true(Test t)
        {
            if ((t.a != 0) || (t.b != 0)) return true;
            else return false;
        }

        public static bool operator |(Test t1, Test t2)
        {
            if ((t1.a != 0) || (t1.b != 0) | (t2.a != 0) || (t2.b != 0)) return true;
            else return false;
        }
        public static bool operator &(Test t1, Test t2)
        {
            if ((t1.a != 0) && (t1.b != 0) & (t2.a != 0) && (t2.b != 0)) return true;
            else return false;
        }
        public static bool operator !(Test test)
        {
            if ((test.a != 0) || (test.b != 0)) return false;
            else return true;
        }

        public static explicit operator Test(int arg)
        {
            return new Test(arg, arg);
        }

        public override string ToString()
        {
            return $"x = {a};\ny = {b};\n";
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(a, b);
        }
        public override bool Equals(object? obj)
        {
            if (obj is Test t)
            {
                return (this.a == t.a) && (this.b == t.b);
            }
            else return false;
        }
    }

    internal class Program
    {
        private static int Main(string[] args)
        {
            int a = 9;
            var obj1 = new Test();
            var obj2 = new Test();
            var obj3 = 1 + obj1 + obj2 + 12;
            var obj4 = (Test)a;
            Console.WriteLine(obj4);
            Console.WriteLine(obj1 | obj2);
            //Console.WriteLine(obj1.Equals(obj2));
            return 0;
        }
    }
}