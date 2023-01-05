using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;

namespace GiantTeam.Startup;

public static class GiantTeamServicesCollectionExtensions
{
    /// <summary>
    /// Add or replaces services from provided the <paramref name="assembly"/>.
    /// Types must either have the <see cref="ServiceAttribute"/>,
    /// or follow the convention that they are not abstract,
    /// their type name ends with "Service", 
    /// are in a namespace that ends with ".Services",
    /// and do not have the <see cref="NotAServiceAttribute"/>.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="assembly"></param>
    /// <returns></returns>
    public static IServiceCollection AddOrReplaceScopedFromAssembly(this IServiceCollection services, Assembly assembly)
    {
        var types = from t in assembly.ExportedTypes
                    where
                        t.GetCustomAttribute<ServiceAttribute>() is not null ||
                        (
                            !t.IsAbstract &&
                            t.Name.EndsWith("Service") &&
                            t.Namespace!.EndsWith(".Services") &&
                            t.GetCustomAttribute<NotAServiceAttribute>() is null
                        )
                    select t;

        foreach (Type type in types)
        {
            Type serviceType;
            if (type.GetCustomAttribute<ServiceAttribute>() is ServiceAttribute serviceAttribute &&
                serviceAttribute.ServiceType is not null)
            {
                serviceType = serviceAttribute.ServiceType;
            }
            else
            {
                serviceType = type;
            }

            // Last wins
            services.RemoveAll(serviceType);
            services.AddScoped(serviceType, type);
        }

        return services;
    }
}
