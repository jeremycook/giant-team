using Npgsql;
using System.Data;
using System.Text.Json;

namespace GiantTeam.Postgres
{
    public static class GiantTeamNpgsqlDataReaderExtensions
    {
        public static T? GetFieldValue<T>(this NpgsqlDataReader reader, string name, JsonSerializerOptions jsonSerializerOptions)
        {
            return reader
                .GetFieldValue<JsonDocument>(name)
                .Deserialize<T>(jsonSerializerOptions);
        }

        public static T? GetFieldValue<T>(this NpgsqlDataReader reader, int ordinal, JsonSerializerOptions jsonSerializerOptions)
        {
            return reader
                .GetFieldValue<JsonDocument>(ordinal)
                .Deserialize<T>(jsonSerializerOptions);
        }

        public static T GetRequiredFieldValue<T>(this NpgsqlDataReader reader, string name, JsonSerializerOptions jsonSerializerOptions)
        {
            return
                reader.GetFieldValue<T>(name, jsonSerializerOptions) ??
                throw new NullReferenceException($"The \"{name}\" column is null.");
        }

        public static T GetRequiredFieldValue<T>(this NpgsqlDataReader reader, int ordinal, JsonSerializerOptions jsonSerializerOptions)
        {
            return
                reader.GetFieldValue<T>(ordinal, jsonSerializerOptions) ??
                throw new NullReferenceException($"Column number {ordinal} is null.");
        }
    }
}
