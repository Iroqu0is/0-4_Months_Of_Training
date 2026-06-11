namespace ConsoleApp2
{
    internal class Point
    {
        public static int counter;
        static Point()
        {
            counter = 1;
        }

        public string Name { get; set; }
        public double X { get; private set; }
        public double Y { get; private set; }
        public Quarter Location { get; private set; }

        public Point(Point? point = default(Point)) : this(point?.X ?? 0, point?.Y ?? 0) { }
        public Point(double both) : this(both, both) { }
        public Point(double x = 0, double y = 0)
        {
            Name = $"{this.GetType().Name.ToLower()}({counter})";
            X = x;
            Y = y;
            Location = FindQuarter();
            Interlocked.Increment(ref counter);
        }

        private Quarter FindQuarter()
        {
            if ((X == 0) && (Y == 0)) return Quarter.OutOfSpaceAndTimeOrZero;// вне пространства и времени :)
            else if (X == 0) return Quarter.LayOnY;
            else if (Y == 0) return Quarter.LayOnX;
            else if ((X > 0) && (Y > 0)) return Quarter.First;
            else if ((X > 0) && (Y < 0)) return Quarter.Fourth;
            else if ((X < 0) && (Y < 0)) return Quarter.Third;
            return Quarter.Second;
        }
        private string GetFullInformation()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Summary information: ");
            sb.AppendLine($"Instance name: {Name};");
            sb.AppendLine($"Сoordinate X: {X};");
            sb.AppendLine($"Coordinate Y: {Y};");
            sb.AppendLine($"Location: {Location}");
            sb.AppendLine($"Distance from start point(zero): {this.Distance(null):f2}");
            return sb.ToString();
        }

        public static Point Create(double x = 0, double y = 0)
        {
            return new Point(x, y);
        }
        public static Point Create(Point? point)
        {
            if (point is null) return new Point(0, 0);
            else return new Point(point.X, point.Y);
        }


        public static bool operator ==(Point point1, Point point2)
        {
            return (point1.X == point2.X) && (point1.Y == point2.Y);
        }
        public static bool operator !=(Point point1, Point point2)
        {
            return (point1.X != point2.X) || (point1.Y != point2.Y);
        }
        public static Point operator +(Point point1, Point point2)
        {
            if (point1 is null) point1 = new Point(0, 0);
            if (point2 is null) point2 = new Point(0, 0);
            return new Point((point1.X + point2.X), (point1.Y + point2.Y));
        }
        public static Point operator +(Point point, double arg)
        {
            if (point is null) point = new Point(0, 0);
            return new Point((point.X + arg), (point.Y + arg));
        }
        public static Point operator +(double arg, Point point)
        {
            if (point is null) point = new Point(0, 0);
            return new Point((point.X + arg), (point.Y + arg));
        }
        public static Point operator -(Point point, double arg)
        {
            if (point is null) point = new Point(0, 0);
            return new Point((point.X - arg), (point.Y - arg));
        }
        public static Point operator *(Point point, double arg)
        {
            if (point is null) point = new Point(0, 0);
            return new Point((point.X * arg), (point.Y * arg));
        }

        public double Distance(Point? point)
        {
            if (point is null) point = new Point(0, 0);
            return Sqrt(Pow((point.X - this.X), 2) + Pow((point.Y - this.Y), 2));
        }
        public static double Distance(Point? point1, Point? point2)
        {
            if (point1 is null) point1 = new Point(0, 0);
            if (point2 is null) point2 = new Point(0, 0);
            return Sqrt(Pow((point2.X - point1.X), 2) + Pow((point2.Y - point1.Y), 2));
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }
        public override bool Equals(object? obj)
        {
            if (obj is not null)
            {
                Point point = (Point)obj;
                return (this.X == point.X) && (this.Y == point.Y);
            }
            return false;
        }
        public string GetInfo(Info info = Info.Full)
        {
            if (info == Info.Full) return GetFullInformation();
            return ToString();
        }
        public override string ToString()
        {
            return $"x = {X:f2}, y = {Y:f2}, location: {Location}.";
        }
    }
    internal enum Info { Full = 0, Short = 2 }
    internal enum Quarter { First = 0, Second = 2, Third = 4, Fourth = 8, LayOnX = 16, LayOnY = 32, OutOfSpaceAndTimeOrZero = 64 }
    internal class MyException : Exception
    {
        public MyException() : base() { }
        public MyException(string str) : base(str) { }
        public MyException(string str, Exception inner) : base(str, inner) { }
    }
    internal class Program
    {
        private static int Main(string[] args)
        {
            var point1 = new Point(22, -4);
            var point2 = new Point(22, 66);
            Console.WriteLine(point1.GetInfo());
            Console.WriteLine(point2.GetInfo());
            return 0;
        }
    }
}