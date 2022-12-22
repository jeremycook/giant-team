namespace GiantTeam.Postgres.Parser.Model
{
    public abstract class IdentifierExpression : Expression
    {
        public IdentifierExpression(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }

}
