using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;

namespace GiantTeam.Startup
{
    public static class GiantTeamServicesCollectionExtensions
    {
        public static IServiceCollection AddScopedFromAssembly(this IServiceCollection services, Assembly assembly)
        {
            var types = from t in assembly.ExportedTypes
                        where
                            t.Name.EndsWith("Service") &&
                            !t.IsAbstract &&
                            t.Namespace!.EndsWith(".Services")
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
}
