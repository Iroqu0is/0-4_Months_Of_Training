namespace ConsoleApp1
{
    using ResE = (Employee? Instance, bool IsDone, string? Message);
    public enum Gender : byte { Undecided, Male, Female }
    public class Employee : Person, IEquatable<Employee>, IComparable<Employee>
    {
        protected byte exp;
        protected Gender gen;

        public byte Experience { get { return exp; } }
        public Gender Gender { get { return gen; } }

        protected Employee(Person person, byte experience, Gender gender) : base(person)
        {
            exp = experience;
            gen = gender;
        }
        protected Employee(string name, string surname, byte age, byte experience, Gender gender) : base(name, surname, age)
        {
            exp = experience;
            gen = gender;
        }
        public static ResE Build(string? name, string? surname, byte? age, byte experience, Gender gender = Gender.Undecided)
        {
            var tmp = Person.Build(name, surname, age);
            if (!tmp.IsDone) return (null, false, tmp.Message);
            if (experience > 50) return (null, false, "Invalid experience.");
            return (new Employee(tmp.Instance!, experience, gender), true, "Instance created.");
        }

        private static bool Equals(Employee? e1, Employee? e2)
        {
            return Person.Equals(e1, e2) && (e1!.exp == e2!.exp) && (e1!.gen == e2!.gen);
        }
        private static int CompareTo(Employee? e1, Employee? e2)
        {
            var res = Person.CompareTo(e1, e2);
            if (res != 0) return res;
            res = e1!.exp.CompareTo(e2!.exp);
            if (res != 0) return res;
            return e1!.gen.CompareTo(e2!.gen);
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
            return HashCode.Combine(base.GetHashCode(), exp, (byte)gen);
        }
        public override string ToString()
        {
            return $"{base.ToString()} Experience: {exp}, gender: {gen}.";
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

        public static bool operator true(Employee? employee)
        {
            return employee is not null;
        }
        public static bool operator false(Employee? employee)
        {
            return employee is null;
        }
        public static bool operator !(Employee? employee)
        {
            return !(employee is not null);
        }
    }
}