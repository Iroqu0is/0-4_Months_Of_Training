namespace ConsoleApp2
{
    public static class CheckExt
    {
        public static T MyMin<T>(this T[]? arr) where T : struct, INumber<T> // АЙНамбер Джемини подсказал
        {
            if (arr is null) throw new ArgumentNullException(nameof(arr));
            if (arr.Length == 0) throw new InvalidOperationException("Array is empty.");
            T min = arr[0];
            for (int i = 1; i < arr.Length; i++)
            {
                if (arr[i] < min) min = arr[i];
            }
            return min;
        }
        public static T MyMax<T>(this T[]? arr) where T : struct, INumber<T>
        {
            if (arr is null) throw new ArgumentNullException(nameof(arr));
            if (arr.Length == 0) throw new InvalidOperationException("Array is empty");
            T max = arr[0];
            for (int i = 1; i < arr.Length; i++)
            {
                if (arr[i] > max) max = arr[i];
            }
            return max;
        }
        public static T MySum<T>(this T[]? arr) where T : struct, INumber<T>
        {
            if (arr is null) throw new ArgumentNullException(nameof(arr));
            T sum = T.Zero;// default(T)
            for (int i = 0; i < arr.Length; i++)
            {
                checked { sum += arr[i]; }
            }
            return sum;
        }
        public static decimal MyAverage<T>(this T[]? arr) where T : struct, INumber<T>
        {
            if (arr is null) throw new ArgumentNullException(nameof(arr));
            return Convert.ToDecimal(arr.MySum()) / arr.Length;
        }
        public static IEnumerable<TResult> MySelect<TSource, TResult>(this TSource[]? arr, Func<TSource, TResult>? func)
                                                                         where TSource : struct, INumber<TSource>
                                                                         where TResult : struct, INumber<TResult>
        {
            if ((arr is null) || (func is null)) yield break;
            for (int i = 0; i < arr.Length; i++)
            {
                yield return func(arr[i]);
            }

        }
        public static IEnumerable<T> Where<T>(this T[]? arr, Func<T, bool>? func) where T : struct, INumber<T>
        {
            if (arr is null) throw new ArgumentNullException(nameof(arr));
            if (func is null) new ArgumentNullException(nameof(func));
            for (int i = 0; i < arr.Length; i++)
            {
                if (func!.Invoke(arr[i])) yield return arr[i];
            }
        }
        public static bool CheckAll<T>(this T[]? arr, Func<T, bool>? func) where T : struct, INumber<T>
        {
            if ((arr is null) || (func is null)) return false;
            if (arr.Length == 0) return true;
            for (int i = 0; i < arr.Length; i++)
            {
                if (!func.Invoke(arr[i])) return false;
            }
            return true;
        }
        public static bool CheckAny<T>(this T[]? arr, Func<T, bool>? func) where T : struct, INumber<T>
        {
            bool result = false;
            if ((arr is null) || (arr.Length == 0)) return result;
            if (func is null) return result;
            for (int i = 0; i < arr.Length; i++)
            {
                result = func.Invoke(arr[i]);
                if (result) break;
            }
            return result;
        }
    }

    internal class Program
    {
        private static int Main(string[] args)
        {
            var arr = new int[50];
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = Random.Shared.Next(-10, 11);
            }
            Console.WriteLine($"Any: {arr.CheckAny(IsPositive)}");
            Console.WriteLine($"All: {arr.CheckAll(IsPositive)}");
            return 0;
        }
        public static bool IsPositive<T>(T arg) where T : struct, INumber<T>
        {
            return arg > T.Zero;
        }
    }
}