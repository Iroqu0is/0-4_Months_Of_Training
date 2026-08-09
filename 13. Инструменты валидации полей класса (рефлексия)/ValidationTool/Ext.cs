using System.Reflection;

namespace ValidationTool
{
    public static class Ext
    {
        public static bool InRange(this object? obj)
        {
            if (obj is null) throw new ArgumentNullException(nameof(obj));
            var type = obj.GetType();
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (fields.Length == 0) return true;
            foreach (var field in fields)
            {
                var attr = field.GetCustomAttribute<Range>();
                if (attr is null) continue;
                var tmp = field.GetValue(obj);
                if (tmp is IComparable value)
                {
                    if (value.CompareTo(attr.LeftBound) < 0 || value.CompareTo(attr.RightBound) > 0) return false;
                }
            }
            return true;
        }
    }
}