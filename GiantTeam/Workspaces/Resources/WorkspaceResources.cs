namespace GiantTeam.Workspaces.Resources
{
    internal static class WorkspaceResources
    {
        static Type Type { get; } = typeof(WorkspaceResources);

        /// <summary>
        /// The contents of "ws.sql".
        /// </summary>
        public static string WsSql { get; } = ReadAllText("ws.sql");

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
