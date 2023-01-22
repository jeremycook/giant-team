using GiantTeam.Postgres;
using GiantTeam.Text;

namespace GiantTeam.Organization.Etc.Data;

public class SqlMetadata : SqlMetadataBase
{
    private const string _recordWord = "Record";

    public override string DefaultSchema { get; } = "etc";

    public override string GetTableName(Type tableType)
    {
        return
            tableType.Name.EndsWith(_recordWord) ? TextTransformers.Snakify(tableType.Name[..^_recordWord.Length]) :
            base.GetTableName(tableType);
    }
}
