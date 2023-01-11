namespace GiantTeam.Organization.Resources
{
    internal static class OrganizationResources
    {
        static Type Type { get; } = typeof(OrganizationResources);

        public static string ScriptOrganizationObjectsSql { get; } = ReadAllText("script_organization_objects.sql");

        public static string ReadAllText(string name)
        {
            string fullName = Type.Namespace + "." + name;
            using var stream =
                Type.Assembly.GetManifestResourceStream(fullName) ??
                throw new ArgumentException($"Resource not found: {fullName}");

            using var reader = new StreamReader(stream);
            var text = reader.ReadToEnd();

            return text;
        }
    }
}
