namespace GiantTeam.Organizations.Organization.Services
{
    public class SpaceInfo
    {
        public SpaceInfo(string? id)
        {
            SchemaName = id;
        }

        public string? SchemaName { get; }
    }
}
