namespace GiantTeam.Postgres.Parser.Model
{
    public class QuotedIdentifierExpression : IdentifierExpression
    {
        public QuotedIdentifierExpression(string name)
        : base(name)
        {
        }

        public override string ToString()
        {
            return $"\"{Name}\"";
        }
    }

}
