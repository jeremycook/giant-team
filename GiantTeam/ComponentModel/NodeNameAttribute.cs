using System.ComponentModel.DataAnnotations;

namespace GiantTeam.ComponentModel
{
    /// <summary>
    /// Requires the value match <see cref="NodeNamePattern"/>.
    /// </summary>
    public class NodeNameAttribute : RegularExpressionAttribute
    {
        public const string NodeNamePattern = "^[^<>:\"/\\|?*]+$";

        public NodeNameAttribute() : base(NodeNamePattern)
        {
            ErrorMessage = "The {0} cannot contain ^ < > : \" / \\ | ? * ] + or $. (Periods are allowed.)";
        }
    }
}
