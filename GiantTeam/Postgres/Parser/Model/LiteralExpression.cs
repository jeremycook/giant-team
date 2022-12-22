namespace GiantTeam.Postgres.Parser.Model
{
    public class LiteralExpression : Expression
    {
        public LiteralExpression(string text)
        {
            Text = text;
        }

        public string Text { get; }

        public override string ToString()
        {
            return $"'{Text}'";
        }
    }

}
