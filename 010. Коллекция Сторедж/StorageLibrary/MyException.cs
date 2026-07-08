namespace StorageLibrary
{
    public class MyException : Exception
    {
        private readonly object? _value;
        public object? Value { get { return _value; } }
        public MyException(object? obj = null) : base() { _value = obj; }
        public MyException(string? str, object? obj = null) : base(str) { _value = obj; }
        public MyException(string? str, Exception? inner, object? obj = null) : base(str, inner) { _value = obj; }

        public override string ToString()
        {
            if (_value is null) return Message;
            return $"Main error message: {Message}\nAdditional data: {_value}.";
        }
    }
}