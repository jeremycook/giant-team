namespace System.Text
{
    public static class GiantTeamStringBuilderExtensions
    {
        public static StringBuilder AppendLF(this StringBuilder stringBuilder)
        {
            stringBuilder.Append('\n');
            return stringBuilder;
        }

        public static StringBuilder AppendLF(this StringBuilder stringBuilder, string value)
        {
            stringBuilder.Append(value);
            stringBuilder.Append('\n');
            return stringBuilder;
        }
    }
}
