namespace GiantTeam.Postgres.Parser.Model
{
    public class StatementExpression : Expression
    {
        public StatementExpression(string name, IReadOnlyCollection<Expression> arguments)
        {
            Name = name;
            Arguments = arguments;
        }

        public string Name { get; }
        public IReadOnlyCollection<Expression> Arguments { get; }

        public override string ToString()
        {
            return $"{Name} {string.Join(" ", Arguments)}";
        }
    }

}
