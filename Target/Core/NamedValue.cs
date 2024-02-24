namespace Core

{
    public class NamedValue : INamed
    {
        public NamedValue() { }
        public NamedValue(string name, object value)
        {
            Value = value;
            Name = name;
        }
        public object Value { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            var name = Name.Replace(" ", "");
            return name;
        }
    }


}