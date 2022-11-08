using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class GiantTeamServicesCollectionExtensions
    {
        public static IServiceCollection AddScopedFromAssembly(this IServiceCollection services, Assembly assembly)
        {
            Type objectType = typeof(object);
            foreach (Type type in
                from t in assembly.ExportedTypes
                where
                    t.Name.EndsWith("Service") &&
                    !t.IsAbstract &&
                    t.Namespace!.EndsWith(".Services")
                select t)
            {
                // Last wins
                if (type.BaseType is not null && type.BaseType != objectType)
                {
                    // Assume replacement
                    services.RemoveAll(type.BaseType);
                    services.AddScoped(type.BaseType, type);
                }
                else
                {
                    services.AddScoped(type);
                }
            }

            return services;
        }
    }
}
