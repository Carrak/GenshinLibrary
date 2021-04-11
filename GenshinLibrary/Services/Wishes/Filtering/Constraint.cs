namespace GenshinLibrary.Services.Wishes
{
    public class Constraint<T>
    {
        public string Operator { get; }
        public T Value { get; set; }

        public Constraint(string op, T value)
        {
            Operator = op;
            Value = value;
        }
    }
}
