namespace GiantTeam.Organizations.Organization.Services
{
    /// <summary>
    /// Access information about the current organization.
    /// </summary>
    public class OrganizationInfo
    {
        public OrganizationInfo(string? id)
        {
            DatabaseName = id;
        }

        public string? DatabaseName { get; }
    }
}
