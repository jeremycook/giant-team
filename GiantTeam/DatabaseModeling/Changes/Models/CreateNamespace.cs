namespace GiantTeam.DatabaseModeling.Changes.Models
{
    public class CreateNamespace : DatabaseChange
    {
        public CreateNamespace(string namespaceName)
            : base(nameof(CreateNamespace))
        {
            NamespaceName = namespaceName;
        }

        public string NamespaceName { get; }
    }
}