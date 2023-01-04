namespace GiantTeam.Postgres
{
    public class PgIdentifier
    {
        public static implicit operator string(PgIdentifier pgIdentifier) => pgIdentifier.ToString();
        public static explicit operator PgIdentifier(string text) => new(text);

        private readonly string value;

        public PgIdentifier(string value)
        {
            this.value = value;
        }

        public override string ToString()
        {
            return PgQuote.Identifier(value);
        }
    }
}
