namespace GiantTeam.Postgres
{
    public class SqlBuilder
    {
        private readonly List<Sql> commands = new();

        public SqlBuilder Add(FormattableString sql)
        {
            commands.Add(Sql.Format(sql));
            return this;
        }

        public SqlBuilder AddRaw(string rawSql)
        {
            commands.Add(Sql.Raw(rawSql));
            return this;
        }
    }
}
