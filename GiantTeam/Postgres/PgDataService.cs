using GiantTeam.Linq;
using GiantTeam.Text;
using Npgsql;
using System.Collections.Immutable;

namespace GiantTeam.Postgres
{
    public abstract class PgDataService
    {
        public abstract string ConnectionString { get; }

        public virtual NpgsqlDataSource CreateDataSource()
        {
            return NpgsqlDataSource.Create(ConnectionString);
        }

        public async Task<int> ExecuteAsync(params FormattableString[] commands)
        {
            await using var dataSource = CreateDataSource();
            await using var batch = dataSource.CreateBatch();
            foreach (var command in commands.Select(Sql.Format))
            {
                batch.BatchCommands.Add(command);
            }
            return await batch.ExecuteNonQueryAsync();
        }

        public async Task<int> ExecuteAsync(NpgsqlBatch batch)
        {
            await using var dataSource = CreateDataSource();
            await using var connection = await dataSource.OpenConnectionAsync();
            batch.Connection = connection;
            return await batch.ExecuteNonQueryAsync();
        }

        public async Task<T?> SingleOrDefaultAsync<T>(Func<NpgsqlDataReader, T>? factory = null)
        {
            int count = 0;
            T? lastItem = default;
            await foreach (var item in QueryAsync(factory, (props, table) => $"SELECT {props.Join(",")} FROM {table} ORDER BY 1 LIMIT 2"))
            {
                lastItem = item;
                count++;
            }

            if (count <= 1)
            {
                return lastItem;
            }
            else
            {
                throw new InvalidOperationException("More than one element.");
            }
        }

        public async Task<T> SingleAsync<T>(Func<NpgsqlDataReader, T>? factory = null)
        {
            int count = 0;
            T? lastItem = default;
            await foreach (var item in QueryAsync(factory, (props, table) => $"SELECT {props.Join(",")} FROM {table} ORDER BY 1 LIMIT 2"))
            {
                lastItem = item;
                count++;
            }

            if (count == 1)
            {
                return lastItem!;
            }
            else if (count == 0)
            {
                throw new InvalidOperationException("No elements.");
            }
            else
            {
                throw new InvalidOperationException("More than one element.");
            }
        }

        public async Task<List<T>> ListAsync<T>(Func<NpgsqlDataReader, T>? itemFactory = null, Func<IEnumerable<string>, string, string>? sqlFactory = null)
        {
            var list = new List<T>();
            await foreach (var item in QueryAsync<T>(itemFactory, sqlFactory))
            {
                list.Add(item);
            }
            return list;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="itemFactory"></param>
        /// <param name="sqlFactory">Defaults to <code>(props, tableName) => $"SELECT {props.Join(",")} FROM {tableName}"</code></param>
        /// <returns></returns>
        public async IAsyncEnumerable<T> QueryAsync<T>(Func<NpgsqlDataReader, T>? itemFactory = null, Func<IEnumerable<string>, string, string>? sqlFactory = null)
        {
            // TODO: LRU cache generated stuff

            sqlFactory ??= (props, tableName) => $"SELECT {props.Join(",")} FROM {tableName}";

            Type type = typeof(T);

            var props = type.GetProperties().Select(p => p.Name).Select(TextTransformers.Snakify).Select(PgQuote.Identifier);
            var tableName = PgQuote.Identifier(TextTransformers.Snakify(type.Name));
            var sql = sqlFactory(props, tableName);

            await using var dataSource = CreateDataSource();
            await using var cmd = dataSource.CreateCommand(sql);
            await using var reader = await cmd.ExecuteReaderAsync();

            if (itemFactory is null)
            {

                var columns = (await reader.GetColumnSchemaAsync())
                    .OrderBy(o => o.ColumnOrdinal)
                    .Select(o => type.GetProperty(o.ColumnName)!)
                    .ToImmutableArray();

                itemFactory = (NpgsqlDataReader reader) =>
                {
                    T item = Activator.CreateInstance<T>();
                    for (int i = 0; i < columns.Length; i++)
                    {
                        var value = reader.GetValue(i);
                        if (value == DBNull.Value)
                        {
                            value = null;
                        }
                        columns[i].SetValue(item, value);
                    }
                    return item;
                };
            }

            while (await reader.ReadAsync())
            {
                var item = itemFactory(reader);
                yield return item;
            }
        }
    }
}
