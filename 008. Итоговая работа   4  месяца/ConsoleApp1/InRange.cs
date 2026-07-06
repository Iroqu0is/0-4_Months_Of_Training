namespace ConsoleApp1
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    //public class InRangeAttribute : Attribute
    //{
    //    private readonly double lbound;
    //    private readonly double rbound;

    //    public double LBound { get { return lbound; } }
    //    public double RBound { get { return rbound; } }

    //    public InRangeAttribute(double lb = double.MinValue, double rb = double.MaxValue)
    //    {
    //        if (lb > rb) throw new MyException("Ты все перепутал :)");
    //        lbound = lb;
    //        rbound = rb;
    //    }
    //}

    public class InRange : Attribute
    {
        private readonly IComparable lbound;
        private readonly IComparable rbound;

        public IComparable LBound { get { return lbound; } }
        public IComparable RBound { get { return rbound; } }

        public InRange(object? lb, object? rb)
        {
            if ((lb is null) ||
               (lb is not IComparable) ||
               (rb is null) ||
               (rb is not IComparable) ||
               (lb.GetType() != rb.GetType())) throw new MyException("Аргументы должны быть числами одинакового типа.");

            var _lb = lb as IComparable;
            var _rb = rb as IComparable;

            if ((_lb is null) || (_rb is null)) throw new MyException("Аргументы должны быть числами.");
            if (_lb.CompareTo(_rb) > 0) throw new MyException("Ты все перепутал :)");

            lbound = _lb;
            rbound = _rb;
        }// Уверен, Джемини написал бы короче, пусть будет пока так
    }
}