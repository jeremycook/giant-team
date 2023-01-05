using GiantTeam.Postgres.Models;
using GiantTeam.Text;
using Npgsql;
using System.Collections.Immutable;
using System.Data.Common;

namespace GiantTeam.Postgres
{
    public abstract class PgDataServiceBase
    {
        protected abstract ILogger Logger { get; }
        protected abstract string ConnectionString { get; }

        public virtual NpgsqlDataSource CreateDataSource()
        {
            return NpgsqlDataSource.Create(ConnectionString);
        }

        public async Task<int> ExecuteAsync(params FormattableString[] commands)
        {
            await using var batch = new NpgsqlBatch();
            foreach (var command in commands.Select(Sql.Format))
            {
                batch.BatchCommands.Add(command);
            }
            return await ExecuteAsync(batch);
        }

        public async Task<int> ExecuteAsync(NpgsqlBatch batch)
        {
            await using var dataSource = CreateDataSource();
            await using var connection = await dataSource.OpenConnectionAsync();
            batch.Connection = connection;
            try
            {
                return await batch.ExecuteNonQueryAsync();
            }
            catch (DbException ex)
            {
                Logger.LogError(ex, "Error executing batch {BatchCommandsText}", batch.BatchCommands.OfType<NpgsqlBatchCommand>().Select(cmd => cmd.CommandText));
                throw;
            }
        }

        public async Task<int> ExecuteUnsanitizedAsync(string unsanitizedSql)
        {
            await using var dataSource = CreateDataSource();
            await using var connection = await dataSource.OpenConnectionAsync();
            await using var cmd = connection.CreateCommand();
            cmd.CommandText = unsanitizedSql;
            try
            {
                return await cmd.ExecuteNonQueryAsync();
            }
            catch (DbException ex)
            {
                Logger.LogError(ex, "Error executing command {CommandText}", cmd.CommandText);
                throw;
            }
        }

        public async Task<QueryTable> QueryTableAsync(Sql sql)
        {
            await using var dataSource = CreateDataSource();
            await using var batch = dataSource.CreateBatch();
            batch.BatchCommands.Add(sql);

            if (batch.BatchCommands[0].StatementType == StatementType.Select)
            {
                throw new ArgumentException($"The {nameof(sql)} argument must be a SELECT statement.", nameof(sql));
            }

            try
            {
                await using var reader = await batch.ExecuteReaderAsync();
                IReadOnlyCollection<Npgsql.Schema.NpgsqlDbColumn> columnSchema = await reader.GetColumnSchemaAsync();

                var cols = columnSchema.Select(o => o.ColumnName).ToArray();
                var rows = new List<object?[]>();
                while (await reader.ReadAsync())
                {
                    var row = new object?[cols.Length];
                    reader.GetValues(row!);
                    for (int i = 0; i < cols.Length; i++)
                    {
                        if (row[i] == DBNull.Value)
                            row[i] = null;
                    }
                    rows.Add(row);
                }

                return new()
                {
                    Columns = cols,
                    Rows = rows,
                };
            }
            catch (DbException ex)
            {
                Logger.LogError(ex, "Error executing query {BatchCommandText}", batch.BatchCommands[0]);
                throw;
            }
        }

        public async Task<T?> SingleOrDefaultAsync<T>(Func<NpgsqlDataReader, T>? factory = null)
        {
            int count = 0;
            T? lastItem = default;
            await foreach (var item in QueryAsync(Sql.Format($"SELECT {GetColumnIdentifiers<T>()} FROM {GetTableIdentifier<T>()} LIMIT 2"), factory))
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
            await foreach (var item in QueryAsync(Sql.Format($"SELECT {GetColumnIdentifiers<T>()} FROM {GetTableIdentifier<T>()} LIMIT 2"), factory))
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

        public async Task<List<T>> ListAsync<T>(Sql? sql = null, Func<NpgsqlDataReader, T>? itemFactory = null)
        {
            var list = new List<T>();
            await foreach (var item in QueryAsync<T>(sql, itemFactory))
            {
                list.Add(item);
            }
            return list;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="itemFactory"></param>
        /// <returns></returns>
        public async IAsyncEnumerable<T> QueryAsync<T>(Sql? sql = null, Func<NpgsqlDataReader, T>? itemFactory = null)
        {
            // TODO: LRU cache generated stuff

            Type type = typeof(T);

            sql ??= Sql.Format($"SELECT {GetColumnIdentifiers(type)} FROM {GetTableIdentifier(type)}");

            await using var dataSource = CreateDataSource();
            await using var batch = dataSource.CreateBatch();
            batch.BatchCommands.Add(sql);

            if (batch.BatchCommands[0].StatementType == StatementType.Select)
            {
                throw new ArgumentException($"The {nameof(sql)} argument must be a SELECT statement.", nameof(sql));
            }

            await using var reader = await batch.ExecuteReaderAsync();

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

        public static Sql GetTableIdentifier<T>()
        {
            return GetTableIdentifier(typeof(T));
        }

        public static Sql GetTableIdentifier(Type type)
        {
            return Sql.Identifier(TextTransformers.Snakify(type.Name));
        }

        public static IEnumerable<Sql> GetColumnIdentifiers<T>()
        {
            return GetColumnIdentifiers(typeof(T));
        }

        public static IEnumerable<Sql> GetColumnIdentifiers(Type type)
        {
            return type.GetProperties().Select(p => p.Name).Select(TextTransformers.Snakify).Select(Sql.Identifier);
        }
    }
}
