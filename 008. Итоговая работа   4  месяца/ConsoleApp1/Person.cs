namespace ConsoleApp1
{
    using ResultP = (Person? Person, bool IsCreate, string? ErrorMessage);
    public class Person : IComparable<Person>, IEquatable<Person>, ICloneable, IUseless<byte>
    {

        public static event EventHandler<ErrorMessage>? Notify;

        private static int counter;
        static Person()
        {
            counter = 0;
        }

        private readonly int recordNumber;
        protected readonly string fname;
        protected readonly string lname;

        [InRange(18, 100)]
        protected readonly byte age;

        public string Name { get { return fname; } }
        public string Surname { get { return lname; } }
        public byte Age { get { return age; } }

        [JsonConstructor]// - это Джемини подсказал
        protected Person(string name, string surname, byte age)
        {
            recordNumber = Interlocked.Increment(ref counter);
            fname = name;
            lname = surname;
            this.age = age;
        }
        protected Person(Person person)
        {
            this.recordNumber = person.recordNumber;
            this.fname = person.fname;
            this.lname = person.lname;
            this.age = person.age;
        }

        public static ResultP BuildFullCopy(Person? person)
        {
            if (person is null) return (null, false, $"{nameof(person)} is null.");
            return (new Person(person), true, "No errors.");
        }
        public static ResultP Build(string? name, string? surname, byte age, byte nameLength = 80)
        {
            if (!name.Check(nameLength))
            {
                Notify?.Invoke(typeof(Person), new ErrorMessage("Invalid or empty name."));
                return (null, false, "Invalid or empty name.");
            }
            if (!surname.Check(nameLength))
            {
                Notify?.Invoke(typeof(Person), new ErrorMessage("Invalid or empty surname."));
                return (null, false, "Invalid or empty surname.");
            }
            if (age > 100)
            {
                Notify?.Invoke(typeof(Person), new ErrorMessage("Invalid age."));
                return (null, false, "Invalid age.");
            }
            return (new Person(name!, surname!, age), true, "No errors.");
        }

        protected static bool Equals(Person? p1, Person? p2)
        {
            if (ReferenceEquals(p1, p2)) return true;
            if ((p1 is null) || (p2 is null)) return false;
            if ((string.Compare(p1.Name, p2.Name, StringComparison.OrdinalIgnoreCase) == 0) &&
                (string.Compare(p1.Surname, p2.Surname, StringComparison.OrdinalIgnoreCase) == 0) &&
                (p1.Age == p2.Age)) return true;
            return false;
        }
        protected static int CompareTo(Person? p1, Person? p2)
        {
            if (ReferenceEquals(p1, p2)) return 0;
            if (p1 is null) return -1;
            if (p2 is null) return 1;
            int result = string.Compare(p1.Surname, p2.Surname, StringComparison.OrdinalIgnoreCase);
            if (result != 0) return result;
            result = string.Compare(p1.Name, p2.Name, StringComparison.OrdinalIgnoreCase);
            if (result != 0) return result;
            return p1.Age.CompareTo(p2.Age);
        }

        public virtual object Clone()// не знаю как правильно, поэтому, данные будут такие же кроме уникального номера, он сгенерится новый (пока так)
        {
            return new Person(this.fname, this.lname, this.age);
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
            return HashCode.Combine(fname.ToUpperInvariant(), lname.ToUpperInvariant(), age);
        }
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Summary info:");
            sb.AppendLine($"Record number: [{recordNumber}]");
            sb.AppendLine($"Name: {Name}");
            sb.AppendLine($"Surname: {Surname}");
            sb.AppendLine($"Age: {Age}");
            return sb.ToString();
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