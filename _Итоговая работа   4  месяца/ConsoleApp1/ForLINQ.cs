namespace ConsoleApp1
{
    public static class ForLINQ// проба пера, пока попробую один метод
    {
        //public static IEnumerable<T> myWhere<T>(this IEnumerable<T>? args, Func<T, bool> function)
        //{
        //    if (args is null) throw new ArgumentNullException(nameof(args));
        //    foreach (var tmp in args)
        //    {
        //        if (function(tmp)) yield return tmp;
        //    }
        //}

        // Новый вариант, после замечаний (до этого я сам в начале не додумался)
        public static IEnumerable<T> myWhere<T>(this IEnumerable<T>? args, Func<T, bool> function)
        {
            if (args is null) throw new ArgumentNullException(nameof(args));
            if (function is null) throw new ArgumentNullException(nameof(function));
            return DoSomeWoorks(args, function);
        }
        private static IEnumerable<T> DoSomeWoorks<T>(this IEnumerable<T>? args, Func<T, bool> function)
        {
            foreach (var tmp in args!)
            {
                if (function(tmp)) yield return tmp;
            }
        }
    }
}