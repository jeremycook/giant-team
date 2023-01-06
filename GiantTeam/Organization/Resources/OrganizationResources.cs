namespace GiantTeam.Organization.Resources
{
    internal static class OrganizationResources
    {
        static Type Type { get; } = typeof(OrganizationResources);

        /// <summary>
        /// The contents of "spaces.sql".
        /// </summary>
        public static string SpacesSql { get; } = ReadAllText("spaces.sql");

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
