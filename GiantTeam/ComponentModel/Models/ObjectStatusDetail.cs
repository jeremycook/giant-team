namespace GiantTeam.ComponentModel.Models
{
    public class ObjectStatusDetail
    {
        public ObjectStatusDetail(string message, IEnumerable<string> members)
        {
            if (string.IsNullOrEmpty(message))
            {
                throw new ArgumentException($"'{nameof(message)}' cannot be null or empty.", nameof(message));
            }

            Message = message;
            Members = members ?? Enumerable.Empty<string>();
        }

        public string Message { get; }
        public IEnumerable<string> Members { get; }
    }
}