namespace PersonalInfo
{
    public class Person : IComparable<Person>, IEquatable<Person>, IComparer<Person>, ICloneable
    {
        private static int counter;

        static Person()
        {
            counter = 0;
        }

        private readonly int id;
        private readonly string firstName;
        private readonly string lastName;
        private readonly DateTime birthDate;

        public int Id { get { return id; } }
        public string FullName { get { return $"{firstName} {lastName}"; } }
        public string Name { get { return firstName; } }
        public string Surname { get { return lastName; } }
        public DateTime BirthDate { get { return birthDate; } }
        public int Age { get { return (int)((DateTime.Today - birthDate).TotalDays / 365.25); } }

        protected Person(Person person) : this(person.firstName, person.lastName, person.birthDate) { }
        protected Person(string name, string surname, DateTime birthday)
        {
            id = Interlocked.Increment(ref counter);
            firstName = name;
            lastName = surname;
            birthDate = birthday;
        }

        public static Person? Create(Person? person, Action<ServiceMessage>? handler = null)
        {
            if (person is null)
            {
                handler?.Invoke(new ServiceMessage($"Parameter {nameof(person)} is null"));
                return null;
            }
            return new Person(person);
        }
        public static Person? Create(string? firstName, string? lastName,
                                     DateTime birthDate, byte nameLengthLimit = 80,
                                     Action<ServiceMessage>? handler = null)
        {
            if (!IsValid(firstName, nameLengthLimit))
            {
                handler?.Invoke(new ServiceMessage($"Invalid parameter {nameof(firstName)}"));
                return null;
            }
            if (!IsValid(lastName, nameLengthLimit))
            {
                handler?.Invoke(new ServiceMessage($"Invalid parameter {nameof(lastName)}"));
                return null;
            }
            return new Person(firstName!, lastName!, birthDate);
        }

        private static bool Equals(Person? p1, Person? p2)
        {
            if (ReferenceEquals(p1, p2)) return true;
            if (p1 is null || p2 is null) return false;
            if (string.Compare(p1.firstName, p2.firstName, StringComparison.CurrentCultureIgnoreCase) == 0 &&
                string.Compare(p1.lastName, p2.lastName, StringComparison.CurrentCultureIgnoreCase) == 0 &&
                p1.birthDate == p2.birthDate)
                return true;
            return false;
        }
        private static int CompareTo(Person? p1, Person? p2)
        {
            if (ReferenceEquals(p1, p2)) return 0;
            if (p1 is null) return -1;
            if (p2 is null) return 1;
            var res = string.Compare(p1.lastName, p2.lastName, StringComparison.CurrentCultureIgnoreCase);
            if (res != 0) return res;
            res = string.Compare(p1.firstName, p2.firstName, StringComparison.CurrentCultureIgnoreCase);
            if (res != 0) return res;
            return p1.birthDate.CompareTo(p2.birthDate);
        }
        private static bool IsValid(string? str, byte length)
        {
            if (string.IsNullOrWhiteSpace(str) || str.Length > length) return false;
            for (int i = 0; i < str.Length; i++)
            {
                if (char.IsPunctuation(str[i]) && str[i] != '-') return false;
            }
            return true;
        }

        public object Clone()
        {
            return new Person(this);
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
            return HashCode.Combine(string.GetHashCode(firstName), string.GetHashCode(lastName), birthDate);
        }
        public override string ToString()
        {
            return $"Record({id}): {firstName} {lastName}, {Age:d}";
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
        public static bool operator ==(Person? p1, Person? p2)
        {
            return Person.Equals(p1, p2);
        }
        public static bool operator !=(Person? p1, Person? p2)
        {
            return !Person.Equals(p1, p2);
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
    }
}