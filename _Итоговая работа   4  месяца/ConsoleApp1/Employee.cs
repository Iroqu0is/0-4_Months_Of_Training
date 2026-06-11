namespace ConsoleApp1
{
    using ResultE = (Employee? Employee, bool IsCreate, string? ErrorMessage);
    public sealed class Employee : Person, IComparable<Employee>, IEquatable<Employee>, ICloneable, IUseless<byte>
    {
        private readonly string company;
        public string Company { get { return company; } }

        [JsonConstructor]// - это Джемини подсказал
        private Employee(string name, string surname, byte age, string company) : base(name, surname, age)
        {
            this.company = company;
        }
        public static ResultE Build(string? name, string? surname, byte age, string company = "N/A", byte nameLength = 80)
        {
            var person = Person.Build(name, surname, age, nameLength);
            if (person.Person is null) return (null, false, person.ErrorMessage);
            return (new Employee(name!, surname!, age, company), true, null);
        }

        public static int CompareTo(Employee? e1, Employee? e2)
        {
            if (ReferenceEquals(e1, e2)) return 0;
            if (e1 is null) return -1;
            if (e2 is null) return 1;
            var result = Person.CompareTo(e1, e2);
            if (result != 0) return result;
            return string.Compare(e1.Company, e2.Company, StringComparison.OrdinalIgnoreCase);
        }
        public static bool Equals(Employee? e1, Employee? e2)
        {
            if (ReferenceEquals(e1, e2)) return true;
            if (e1 is null || e2 is null) return false;
            return Person.Equals(e1, e2) &&
                               string.Equals(e1.Company, e2.Company, StringComparison.OrdinalIgnoreCase);
        }

        public override object Clone()
        {
            return new Employee(this.fname, this.lname, this.age, this.company);
        }
        public int CompareTo(Employee? employee)
        {
            return Employee.CompareTo(this, employee);
        }
        public bool Equals(Employee? employee)
        {
            return Employee.Equals(this, employee);
        }

        public override bool Equals(object? obj)
        {
            var employee = obj as Employee;
            return Employee.Equals(this, employee);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(fname.ToUpperInvariant(), lname.ToUpperInvariant(), age, company.ToUpperInvariant());
        }
        public override string ToString()
        {
            var sb = new StringBuilder(base.ToString());
            sb.AppendLine($"Company name: {Company}");
            return sb.ToString();
        }

        public static bool operator ==(Employee? e1, Employee? e2)
        {
            return Employee.Equals(e1, e2);
        }
        public static bool operator !=(Employee? e1, Employee? e2)
        {
            return !Employee.Equals(e1, e2);
        }

        public static bool operator <(Employee? e1, Employee? e2)
        {
            return Employee.CompareTo(e1, e2) < 0;
        }
        public static bool operator >(Employee? e1, Employee? e2)
        {
            return Employee.CompareTo(e1, e2) > 0;
        }

        public static bool operator <=(Employee? e1, Employee? e2)
        {
            return Employee.CompareTo(e1, e2) <= 0;
        }
        public static bool operator >=(Employee? e1, Employee? e2)
        {
            return Employee.CompareTo(e1, e2) >= 0;
        }
    }
}
