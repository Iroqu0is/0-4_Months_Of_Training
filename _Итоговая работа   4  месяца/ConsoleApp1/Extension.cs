namespace ConsoleApp1
{
    public static class Extension
    {
        //----------------------------------------------------------------------------------
        // этот метод переработать полностью, очень строгая проверка
        //public static bool Check(this string? arg, byte nameMaxLength = 80)
        //{
        //    if (string.IsNullOrWhiteSpace(arg)) return false;
        //    var str = arg.Trim();
        //    if (str.Length <= 1 || str.Length > nameMaxLength) return false;
        //    byte specSymbCount = 0;
        //    char specSymb = char.MinValue;
        //    if (char.IsLetter(str[0]) && char.IsLetterOrDigit(str[^1]))
        //    {
        //        foreach (var sym in str)
        //        {
        //            if (!char.IsLetter(sym))
        //            {
        //                specSymb = sym;
        //                specSymbCount++;
        //            }
        //            if (specSymbCount > 1) return false;
        //        }
        //        if ((specSymbCount == 0) || (specSymbCount == 1 && specSymb == '-')) return true;
        //    }
        //    return false;
        //}
        //----------------------------------------------------------------------------------------------

        public static bool Check(this string? arg, byte nameMaxLength = 80)
        {
            if (string.IsNullOrWhiteSpace(arg)) return false;
            var str = arg.Trim();
            if (!char.IsLetter(str[0]) || !char.IsLetterOrDigit(str[^1]) || str.Length < 2 || str.Length > nameMaxLength) return false;//цифры надо отавить, потому что у некоторых "персонажей" есть цифры в имени.
            foreach (var symb in str)
            {
                if (char.IsPunctuation(symb) && (symb != '`')) return false;
            }
            return true;
        }
        public static bool InRange(this object? person)
        {
            if (person is null) throw new ArgumentNullException(nameof(person));
            bool result = true;
            var fields = person.GetType().GetFields(BindingFlags.Public |
                                                  BindingFlags.NonPublic |
                                                  BindingFlags.Instance); // здесь после замечания убрал статику, метод стал менее универсальным.
            foreach (var field in fields)
            {
                var attr = field.GetCustomAttribute<InRange>();
                if (attr is null) continue;
                if (field.GetValue(person) is IComparable value)
                {
                    if ((value.CompareTo(attr.LBound) < 0) || (value.CompareTo(attr.RBound) > 0)) result = false;
                }
            }
            return result;
        }
    }
}