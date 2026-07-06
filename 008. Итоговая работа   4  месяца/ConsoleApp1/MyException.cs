namespace ConsoleApp1
{
    public class MyException : Exception
    {
        private readonly object? value;
        public Object? Value { get { return value; } }
        public MyException(object? obj = null) : base() { value = obj; }
        public MyException(string? str, object? obj = null) : base(str) { value = obj; }
        public MyException(string? str, Exception? inner, object? obj = null) : base(str, inner) { value = obj; }
        public override string ToString()
        {
            string errorMessage = base.ToString();
            if (value is null) return errorMessage;
            var sb = new StringBuilder(errorMessage);
            sb.AppendLine();
            sb.AppendLine($"Additional data (Value): {value}");// исправил после замечание, было (Value), внутри класса - только поля (так учит Великий Размышлятор Джемини :). 
            return sb.ToString();
        }
    }
}