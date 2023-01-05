namespace GiantTeam.Postgres.Models
{
    public class QueryTable
    {
        public string[] Columns { get; init; } = null!;
        public List<object?[]> Rows { get; init; } = null!;
    }
}