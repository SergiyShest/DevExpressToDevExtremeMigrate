namespace Core

{
    public class NamedInt : INamedInt
    {
        public NamedInt() { }
        
        public NamedInt(int? id, string name)
        {
            Id = (int)id;
            Name = name;
        }
        
        public int Id { get; set; }
        
        public string Name { get; set; }

        public override string ToString()
        {
            var name = Name.Replace(" ", "");
            return name;
        }
    }

}