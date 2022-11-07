namespace GiantTeam.DatabaseModel
{
    public class UniqueConstraint
    {
        public UniqueConstraint(string name, bool isPrimaryKey)
        {
            Name = name;
            IsPrimaryKey = isPrimaryKey;
        }

        public string Name { get; }
        public bool IsPrimaryKey { get; }
        public List<string> Columns { get; } = new();
    }
}