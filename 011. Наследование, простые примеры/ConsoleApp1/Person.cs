namespace ConsoleApp1
{
    using ResP = (Person? Instance, bool IsDone, string? Message);
    public class Person : ICloneable, IEquatable<Person>, IComparable<Person>, IComparer<Person>
    {
        protected static int counter;
        static Person()
        {
            counter = 0;
        }

        private string fname;
        private string lname;
        private byte age;
        private int id;

        public bool IsSynchronized { get { return false; } }
        public bool IsReadOnly { get { return true; } }

        public string Surname
        {
            get { return lname; }
            protected set { lname = value; }
        }
        public string Name
        {
            get { return fname; }
            protected set { fname = value; }
        }
        public byte Age
        {
            get { return age; }
            protected set { age = value; }
        }
        public int Id
        {
            get { return id; }
            protected set { id = value; }
        }

        protected Person(Person person) : this(person.fname, person.lname, person.age) { }
        protected Person(string f, string l, byte a)
        {
            id = Interlocked.Increment(ref counter);
            fname = f;
            lname = l;
            age = a;
        }

        public static ResP Build(string? firstName, string? lastName, byte? age, byte? stringLengthLimit = 80)
        {
            if (!stringLengthLimit.HasValue) stringLengthLimit = 80;
            if (!CheckName(firstName, stringLengthLimit.Value)) return (null, false, "Invalid name.");
            if (!CheckName(lastName, stringLengthLimit.Value)) return (null, false, "Invalid surname.");
            if (!age.HasValue || (age.HasValue && age.Value > 99)) return (null, false, "Invalid age.");
            return (new Person(firstName!, lastName!, age.Value), true, "Instance created.");
        }
        public static ResP Build(Person? person)
        {
            if (person is null) return (null, false, $"{nameof(person)} is null.");
            return (new Person(person), true, "Instance created.");
        }

        protected static bool Equals(Person? p1, Person? p2)
        {
            if (ReferenceEquals(p1, p2)) return true;
            if (p1 is null || p2 is null) return false;
            if (string.Compare(p1.fname, p2.fname, StringComparison.InvariantCultureIgnoreCase) == 0 &&
                string.Compare(p1.lname, p2.lname, StringComparison.InvariantCultureIgnoreCase) == 0 &&
                p1.age == p2.age)
                return true;
            return false;
        }
        protected static int CompareTo(Person? p1, Person? p2)
        {
            if (Person.Equals(p1, p2)) return 0;
            if (p1 is null) return -1;
            if (p2 is null) return 1;
            var result = string.Compare(p1.lname, p2.lname, StringComparison.InvariantCultureIgnoreCase);
            if (result != 0) return result;
            result = string.Compare(p1.fname, p2.fname, StringComparison.InvariantCultureIgnoreCase);
            if (result != 0) return result;
            return p1.age.CompareTo(p2.age);
        }
        protected static bool CheckName(string? str, byte limitLength)
        {
            if (string.IsNullOrWhiteSpace(str) || str.Length > limitLength) return false;
            for (int i = 0; i < str.Length; i++)
            {
                if (char.IsPunctuation(str[i]) && str[i] != '`' && str[i] != '-') return false;
            }
            return true;
        }

        public object Clone()
        {
            return new Person(this.fname, this.lname, this.age);
        }
        public int Compare(Person? p1, Person? p2)
        {
            return Person.CompareTo(p1, p2);
        }
        public int CompareTo(Person? person)
        {
            return Person.CompareTo(this, person);
        }
        public bool Equals(Person? person)
        {
            return Person.Equals(this, person);
        }
        public override bool Equals(object? obj)
        {
            var person = obj as Person;
            return Person.Equals(this, person);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(string.GetHashCode(fname, StringComparison.InvariantCultureIgnoreCase),
                                    string.GetHashCode(lname, StringComparison.InvariantCultureIgnoreCase),
                                    age);
        }
        public override string ToString()
        {
            return $"Record({id}): {fname} {lname}, {age}.";
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
            return !(person is not null);
        }
    }
}