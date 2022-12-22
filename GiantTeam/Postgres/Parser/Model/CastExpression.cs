namespace GiantTeam.Postgres.Parser.Model
{
    public class CastExpression : Expression
    {
        public CastExpression(Expression expression, string typeName)
        {
            Expression = expression;
            TypeName = typeName;
        }

        public Expression Expression { get; }
        public string TypeName { get; }

        public override string ToString()
        {
            return $"{Expression}::{TypeName}";
        }
    }

}
