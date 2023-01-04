namespace GiantTeam.Startup
{
    /// <summary>
    /// Specify that a type is a service implementation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ServiceAttribute : Attribute
    {
        public Type? ServiceType { get; set; }
    }
}
