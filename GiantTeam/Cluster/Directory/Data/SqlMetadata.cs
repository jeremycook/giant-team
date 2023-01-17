using GiantTeam.Postgres;

namespace GiantTeam.Cluster.Directory.Data;

public class SqlMetadata : SqlMetadataBase
{
    public override string DefaultSchema { get; } = "directory";
}
