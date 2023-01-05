namespace GiantTeam.Startup
{
    /// <summary>
    /// Specify that a type should not be considered a service even if it matches the service conventions.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class IgnoreServiceAttribute : Attribute
    {
    }
}
