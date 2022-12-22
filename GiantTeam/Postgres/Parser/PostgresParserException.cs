namespace GiantTeam.Postgres.Parser
{
    [Serializable]
    public class PostgresParserException : Exception
    {
        protected PostgresParserException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
        }

        public PostgresParserException(
            ReadOnlySpan<char> expected,
            int positionFromEnd,
            Exception? innerException = null)
        : base($"Expected {expected}.", innerException)
        {
            PositionFromEnd = positionFromEnd;
        }

        internal void FixUp(ReadOnlySpan<char> source)
        {
            int position = source.Length - PositionFromEnd;

            ReadOnlySpan<char> segment = source[..position];

            int line = 1;
            foreach (char ch in segment)
            {
                if (ch == '\n')
                    line++;
            }

            int columnNumber = position - segment.LastIndexOf('\n');

            Position = position;
            LineNumber = line;
            ColumnNumber = columnNumber;
        }

        public int PositionFromEnd { get; }
        public int Position { get; private set; } = -1;
        public int LineNumber { get; private set; } = -1;
        public int ColumnNumber { get; private set; } = -1;
    }
}