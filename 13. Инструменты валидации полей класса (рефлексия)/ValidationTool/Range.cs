namespace ValidationTool
{
    [AttributeUsage(AttributeTargets.Field)]
    public class Range : Attribute
    {
        private readonly IComparable lb;
        private readonly IComparable rb;

        public IComparable LeftBound { get { return lb; } }
        public IComparable RightBound { get { return rb; } }

        public Range(object? leftbound, object? rightbound)
        {
            if (leftbound is null) throw new ArgumentNullException($"{nameof(leftbound)} is null");
            if (rightbound is null) throw new ArgumentNullException($"{nameof(rightbound)} is null");

            var _lb = leftbound as IComparable;
            if (_lb is null || !_lb.GetType().IsValueType) throw new ArgumentException($"{nameof(leftbound)} не является числом");

            var _rb = rightbound as IComparable;
            if (_rb is null || !_rb.GetType().IsValueType) throw new ArgumentException($"{nameof(rightbound)} не является числом");

            if (_lb.GetType().Name != _rb.GetType().Name) throw new ArgumentException("Параметры должны быть одного типа");
            if (_lb.CompareTo(_rb) >= 0) throw new ArgumentException("Неправильно задан диапазон");

            lb = _lb;
            rb = _rb;
        }
        public static bool operator true(Range? attr)
        {
            return attr is not null;
        }
        public static bool operator false(Range? attr)
        {
            return attr is null;
        }
        public static bool operator !(Range? attr)
        {
            return attr is null;
        }
    }
}
