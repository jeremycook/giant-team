using GiantTeam.Postgres;

namespace GiantTeam.Organization.Etc;

public class SqlMetadata : SqlMetadataBase
{
    public override string DefaultSchema { get; } = "etc";
}
