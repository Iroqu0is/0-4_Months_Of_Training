namespace PersonLibrary
{
    public class PersonException : Exception
    {
        private readonly object? _value;

        public PersonException(object? value = null) { _value = value; }
        public PersonException(string? str, object? value = null) : base(str) { _value = value; }
        public PersonException(string? str, Exception? inner, object? value = null) : base(str, inner) { _value = value; }

        public override string ToString()
        {
            if (_value is null) return $"{Message}";
            return $"Message: {Message}, additional: {_value}";
        }
    }
}