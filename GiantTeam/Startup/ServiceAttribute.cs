namespace GiantTeam.Startup
{
    /// <summary>
    /// Specify that a type is a service implementation.
    /// Optionally, specify the <see cref="ServiceType"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ServiceAttribute : Attribute
    {
        /// <summary>
        /// When set this will be the service that the class
        /// this annotates implements. When left unset the
        /// class this attribute is applied to will be the
        /// service and the implementation.
        /// </summary>
        public Type? ServiceType { get; set; }
    }
}
