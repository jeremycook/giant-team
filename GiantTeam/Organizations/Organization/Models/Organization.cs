namespace GiantTeam.Organizations.Organization.Models
{
    public class Organization
    {
        public Organization(string id, Database database)
        {
            Id = id;
            Database = database;
        }

        public string Id { get; }
        public Database Database { get; }
    }
}
