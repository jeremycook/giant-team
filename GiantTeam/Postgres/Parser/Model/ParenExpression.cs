namespace GiantTeam.Postgres.Parser.Model
{
    public class ParenExpression : Expression
    {
        public ParenExpression(Expression? expression)
        {
            Expression = expression;
        }

        public Expression? Expression { get; }

        public override string ToString()
        {
            return $"({Expression})";
        }
    }

}
