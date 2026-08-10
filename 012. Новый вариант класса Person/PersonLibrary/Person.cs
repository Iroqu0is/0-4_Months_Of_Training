namespace PersonLibrary
{
    public class Person : IComparable<Person>, IEquatable<Person>, ICloneable, IComparer<Person>
    {
        private static int counter;
        private static readonly byte stringLimit;
        static Person()
        {
            counter = 0;
            stringLimit = byte.MaxValue;
        }

        private readonly int id;
        private readonly string fn;
        private readonly string ln;
        private readonly DateTime bd;

        public int Id { get { return id; } }
        public string Name { get { return fn; } }
        public string Surname { get { return ln; } }
        public DateTime BirthDate { get { return bd; } }
        public static int CreatedCount { get { return counter; } }


        public string Fullname { get { return $"{fn} {ln}"; } }
        public int Age { get { return DateTime.Today.Year - bd.Year; } }

        protected Person(Person person) : this(person.fn, person.ln, person.bd) { }
        protected Person(string firstName, string lastName, DateTime dateTime)
        {
            id = Interlocked.Increment(ref counter);
            fn = firstName;
            ln = lastName;
            bd = dateTime;
        }

        public static Person? Build(Person? person)
        {
            if (!person) throw new PersonException($"Invalid parameter {nameof(person)}", typeof(Person));
            return new Person(person!);
        }
        public static Person? Build(string? firstName, string? lastName, DateTime dateTime)
        {
            if (!CheckStr(firstName)) throw new PersonException($"Invalid parameter {nameof(firstName)}", typeof(Person));
            if (!CheckStr(lastName)) throw new PersonException($"Invalid parameter {nameof(lastName)}", typeof(Person));
            return new Person(firstName!, lastName!, dateTime);
        }

        private static bool Equals(Person? p1, Person? p2)
        {
            if (ReferenceEquals(p1, p2)) return true;
            if (p1 is null || p2 is null) return false;
            if (string.Compare(p1.fn, p2.fn, StringComparison.CurrentCultureIgnoreCase) == 0 &&
                string.Compare(p2.ln, p2.ln, StringComparison.CurrentCultureIgnoreCase) == 0 &&
                p1.bd == p2.bd)
                return true;
            return false;
        }
        private static int CompareTo(Person? p1, Person? p2)
        {
            if (ReferenceEquals(p1, p2)) return 0;
            if (p1 is null) return -1;
            if (p2 is null) return 1;
            var compareResult = string.Compare(p1.ln, p2.ln, StringComparison.CurrentCultureIgnoreCase);
            if (compareResult != 0) return compareResult;
            compareResult = string.Compare(p1.fn, p2.fn, StringComparison.CurrentCultureIgnoreCase);
            if (compareResult != 0) return compareResult;
            return p1.bd.CompareTo(p2.bd);
        }
        private static bool CheckStr(string? str)
        {
            if (string.IsNullOrWhiteSpace(str) || str.Length >= stringLimit) return false;
            for (int i = 0; i < str.Length; i++)
            {
                if (!char.IsLetter(str[i]) && str[i] != '-') return false;
            }
            return true;
        }

        public object Clone()
        {
            return new Person(this);
        }
        public bool Equals(Person? person)
        {
            return Person.Equals(this, person);
        }
        public int Compare(Person? p1, Person? p2)
        {
            return Person.CompareTo(p1, p2);
        }
        public int CompareTo(Person? person)
        {
            return Person.CompareTo(this, person);
        }

        public override bool Equals(object? obj)
        {
            var person = obj as Person;
            return Person.Equals(this, person);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(string.GetHashCode(fn, StringComparison.CurrentCultureIgnoreCase),
                                    string.GetHashCode(ln, StringComparison.CurrentCultureIgnoreCase),
                                    bd);
        }
        public override string ToString()
        {
            return $"{fn} {ln}, {Age}y.o.";
        }

        public static bool operator true(Person? person)
        {
            return person is not null;
        }
        public static bool operator false(Person? person)
        {
            return person is null;
        }
        public static bool operator !(Person? person)
        {
            return person is null;
        }

        public static bool operator ==(Person? p1, Person? p2)
        {
            return Person.Equals(p1, p2);
        }
        public static bool operator !=(Person? p1, Person? p2)
        {
            return !Person.Equals(p1, p2);
        }

        public static bool operator <(Person? p1, Person? p2)
        {
            return Person.CompareTo(p1, p2) < 0;
        }
        public static bool operator >(Person? p1, Person? p2)
        {
            return Person.CompareTo(p1, p2) > 0;
        }
        public static bool operator <=(Person? p1, Person? p2)
        {
            return Person.CompareTo(p1, p2) <= 0;
        }
        public static bool operator >=(Person? p1, Person? p2)
        {
            return Person.CompareTo(p1, p2) >= 0;
        }
    }
}