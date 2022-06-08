namespace WebApp.DatabaseModel;

public class TableIndex
{
    public TableIndex(string name, bool isUnique)
    {
        Name = name;
        IsUnique = isUnique;
    }

    public string Name { get; }
    public bool IsUnique { get; }
    public List<string> Columns { get; } = new();
}
