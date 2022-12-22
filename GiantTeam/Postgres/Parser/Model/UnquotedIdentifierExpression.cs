namespace GiantTeam.Postgres.Parser.Model
{
    public class UnquotedIdentifierExpression : IdentifierExpression
    {
        public UnquotedIdentifierExpression(string name)
        : base(name)
        {
        }

        public override string ToString()
        {
            return Name;
        }
    }

}
